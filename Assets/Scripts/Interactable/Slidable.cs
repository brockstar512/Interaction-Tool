using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using System.Threading.Tasks;
using Player.ItemOverlap;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


[RequireComponent(typeof(SpriteRenderer))]
public class Slidable : InteractableBase
{
    [SerializeField] private Utilities.KeyTypes key;
    public LayerMask obstructionLayer;
    const int animationDelay = 250;
    [SerializeField] OverlapMoveDamageCheck moverCheckPrefab;
    [SerializeField]  OverlapTargetCheck targetCheckPrefab;
    const int ContactPointCount = 3;
    OverlapTargetCheck _targetCheck;
    OverlapMoveDamageCheck _moverCheck;
    private Collider2D _col;
    private Tweener slideAnimation;
    

    

    void Awake()
    {
        obstructionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.SlidableObstructionLayer);
        obstructionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.InteractableLayer);

        _col = GetComponent<Collider2D>();
        UpdateLayerName();
    }

    public override bool Interact(PlayerStateMachineManager state)
    {
       return CanMove(state.currentState.LookDirection);
    }
    
    ClosestContactPointHelper GetClosestColliderHit(Vector2 direction)
    {
        List<ClosestContactPointHelper> contactPoints = new List<ClosestContactPointHelper>();
        for (int i = 0; i < ContactPointCount; i++)
        {
            //get the dierctions of raycasts
            ClosestContactPointHelper closestContactPointHelper = new ClosestContactPointHelper(direction,obstructionLayer);
            
            if (direction == Vector2.down || direction == Vector2.up)
            {
                //set their starting pos
                closestContactPointHelper.SetOriginPointX(_col,i);
            }
            if(direction == Vector2.left || direction == Vector2.right)
            {
                //set their starting pos
                closestContactPointHelper.SetOriginPointY(_col,i);
            }
            closestContactPointHelper.SetColliderHit();
            contactPoints.Add(closestContactPointHelper);
        }
        
        return contactPoints.OrderBy(dis => dis.Distance).ToList()[0];
    }

    bool ItemAgainstTheWall(Vector2 direction, ClosestContactPointHelper hit)
    {
        float extendSideItem = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y)) == Vector2.right
            ? _col.bounds.extents.x
            : _col.bounds.extents.y;
        float extendSideObstruction = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y)) == Vector2.right
            ? hit.Col.bounds.extents.x
            : hit.Col.bounds.extents.y;

        float colliderInternalTotal = extendSideItem + extendSideObstruction;

        return hit.Distance - colliderInternalTotal < 1f ? true : false;
    }
    
    bool CanMove(Vector2 direction)
    {
        ClosestContactPointHelper hit = GetClosestColliderHit(direction);

        if (hit.Col != null)
        {
            if (ItemAgainstTheWall(direction,hit))
            {
                return false;
            }
            
            //slide the item
            SlideItem(direction, hit);

            return true;
        }
        return false;
    }
    
    async void SlideItem(Vector2 direction, ClosestContactPointHelper hit)
    {
        
        Vector3 currColSize = _col.bounds.extents; 
        Vector3 targetColSize = hit.Col.bounds.extents; 
        float travelDistance = hit.Distance; 
        //this is so is we are traveling up or right it will subtract to be on the lower side of the destinations position and add
        //if we are traveling left or down to be above or to the right of desitination position
        Vector2 sideOfDestination = direction * -1;
        
        Vector3 destination = Vector3.zero;
        Vector3 currentLocation = this.transform.position;
        
        //do this if we are trying to slide it up or down. only deal with the y direction
        if (direction == Vector2.down || direction == Vector2.up)
        {
            float bufferMargin = currColSize.y + targetColSize.y;
            bufferMargin *= sideOfDestination.y;
          
            destination = new Vector2(currentLocation.x,currentLocation.y +(travelDistance* direction.y) +bufferMargin);
            //the total distance is the margin of the center of the both items
        }
        //do this if we are trying to slide it right or left. only deal with the x direction
        if (direction == Vector2.right || direction == Vector2.left)
        {
            float bufferMargin = currColSize.x + targetColSize.x;
            bufferMargin *= sideOfDestination.x;

            destination = new Vector2(currentLocation.x +(travelDistance* direction.x) +bufferMargin, currentLocation.y);

        }
        //get the time for how long it will take to animate there
        float time = MeasureTime(hit.Distance);
        //pause so the character can animate
        await Task.Delay(animationDelay);
        //create the movement checks
        _moverCheck =Instantiate( moverCheckPrefab, moverCheckPrefab.transform.position, Quaternion.identity, this.transform);
        _moverCheck.SetDirectionOfOverlap(direction*-1);
        _moverCheck.SetEmergencyStop(EmergencyStopTween);
        _targetCheck =Instantiate( targetCheckPrefab, this.transform.position + targetCheckPrefab.transform.position, Quaternion.identity, this.transform);
        //animate this to the location and give it a callback when complete
        slideAnimation = transform.DOMove(destination, time);
        slideAnimation.onComplete = CleanUp;
    }

    void EmergencyStopTween()
    {
        //if we hit an obstructable stop immediatly
        slideAnimation.Kill();
        CleanUp();
    }

    float MeasureTime(float distance)
    {
        const float speed = 15;
        return (distance / speed);
    }

    public override void Release(PlayerStateMachineManager player)
    {
        
    }
    
    async void CleanUp()
    {
        bool isPlaced = await _targetCheck.IsOnKeyPort(key);
        _moverCheck.CleanUp();
        _targetCheck.CleanUp();
        if (isPlaced)
        {
            Destroy(this);
        }

    }

    private struct ClosestContactPointHelper
    {
        public Collider2D Col;
        public float Distance;
        private readonly Vector2 _direction;
        private Vector2 _originPoint;
        private readonly LayerMask _obstructionLayer;
        //todo dynaimcal set the origin points based off the size so that if I want to make the size of the slidable bigger
        //it will dynamically create the points based off how big it is so it can be more than 3

        public ClosestContactPointHelper(Vector2 direction, LayerMask detectionLayer)
        {
            _direction = direction;
            Distance = Mathf.Infinity;
            Col = null;
            _originPoint = Vector3.zero;
            _obstructionLayer = detectionLayer;
        }
        public void SetOriginPointY(Collider2D callersCol, int index)
        {
            _originPoint =  callersCol.bounds.center;

                switch (index)
                {
                 case 0:
                     _originPoint.y += callersCol.bounds.extents.y;
                     break;
                 case 2:
                     _originPoint.y -= callersCol.bounds.extents.y;
                     break;
                 default:
                     break;
                }
        }
        
        public void SetOriginPointX(Collider2D callersCol, int index)
        {
            _originPoint = callersCol.bounds.center;
            
                switch (index)
                {
                    case 0:
                        _originPoint.x += callersCol.bounds.extents.x;
                        break;
                    case 2:
                        _originPoint.x -= callersCol.bounds.extents.x;
                        break;
                    default:
                        break;
                }
        }
        
       public void SetColliderHit()
        {
            //ignore collider on querying object
            Physics2D.queriesStartInColliders = false;
            RaycastHit2D hit = Physics2D.Raycast(_originPoint, _direction ,int.MaxValue,_obstructionLayer);
            Debug.DrawRay(_originPoint,_direction, Color.blue);
            
            if (hit.collider!=null)
            {
                Col = hit.collider;
               Distance = (_direction == Vector2.right ||_direction == Vector2.left) ? Mathf.Abs(_originPoint.x - hit.collider.transform.position.x) : Mathf.Abs(_originPoint.y - hit.collider.transform.position.y);

            }

        }
        
    }
    
}


//create a struct for each point

/*
 *
 *     // (string kind, string colour, int length) rayCastSet = ("annual", "blue", 1);
   // (string kind, string colour, int length)[] colliderSets = new[]
   // {
   //     //rayCastSet,
   //     ("annual", "blue", 1),
   // };

   private (Collider2D col, float distance, int length)[] _colliderSets =
       new (Collider2D col, float distance, int length)[3];
       
       
               float GetAbsoluteValueOfDistance(float from, float to)
   {
       return Mathf.Abs(from - to);
   }
       
 */