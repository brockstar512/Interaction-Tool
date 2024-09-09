using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using System.Threading.Tasks;
// using DG.Tweening.Core;
using Player.ItemOverlap;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


[RequireComponent(typeof(SpriteRenderer))]
public class Slidable : InteractableBase
{
    [SerializeField] private Utilities.KeyTypes key;
    public LayerMask obstructionLayer;
    Vector3 getColWidth { get { return _col.bounds.size; } }
    const int animationDelay = 250;
    [SerializeField] OverlapMoveDamageCheck moverCheckPrefab;
    [SerializeField]  OverlapTargetCheck targetCheckPrefab;
    const int contactPointCount = 3;
    OverlapTargetCheck _targetCheck;
    OverlapMoveDamageCheck _moverCheck;
    private Collider2D _col;
    private Tweener slideAnimation;
    

    

    void Awake()
    {
        obstructionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.SlidableObstructionLayer);
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
        for (int i = 0; i < contactPointCount; i++)
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

        // foreach (var item in contactPoints)
        // {
        //     Debug.Log($"item {item.Distance} for {item.Col.gameObject.name}");
        // }

        var results  = contactPoints.OrderBy(dis => dis.Distance).ToList();
        //Debug.Log($"Winner is {results[0].Col.gameObject.name} at {results[0].Distance} and {direction}");
        //Debug.Break();
        return contactPoints.OrderBy(dis => dis.Distance).ToList()[0];
    }

    bool CanMove(Vector2 direction)
    {
        ClosestContactPointHelper hit = GetClosestColliderHit(direction);
        
        
        //todo if any racast is less than 1
        if (hit.Col != null)
        {
            //Debug.Log(hit.collider.gameObject.name);
            //if we are not right up next to the wall
            if (hit.Distance <= getColWidth.x)
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
        
        //get a reference to the size of the item we are hitting
        Vector3 hitItemsRadius = hit.Col.bounds.extents; 
        //get the radius so we have the distance to stop from its center
        // Vector2 hitItemsRadius = new Vector2(hitSr.x / 2, hitSr.y / 2);
        //get a vector pointing to our sliding object
        Vector2 sideOfDestination = direction * -1;
        //get the location of the radius we need to stop at
        hitItemsRadius *= sideOfDestination;
        //get our current location of the object we are sliding
        Vector3 currentLocation = _col.transform.position;
        Vector2 destination = Vector2.zero;

        //do this if we are trying to slide it up or down. only deal with the y direction
        if (direction == Vector2.down || direction == Vector2.up)
        {

            //get the location we hit
            float hitLocation = hit.Col.transform.position.y;
            //radius time the vector pointing from hit to us
            
            float yMargin =  hitLocation + hitItemsRadius.y + (transform.GetComponent<SpriteRenderer>().bounds.extents.y)  * (direction.y * -1);;
            //set the destination to the it you needs to travel
            destination = new Vector2(currentLocation.x, yMargin);
            //the total distance is the margin of the center of the both items
        }
        //do this if we are trying to slide it right or left. only deal with the x direction
        if (direction == Vector2.right || direction == Vector2.left)
        {
            float distanceMargin = hit.Col.transform.position.x;
            float myWidthWithSidePos = (getColWidth.x / 2) * (direction.x * -1);
            destination = new Vector2(distanceMargin + hitItemsRadius.x + myWidthWithSidePos, currentLocation.y);

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
/*
    async void SlideItem(Vector2 direction, ClosestContactPointHelper hit)
    {
        
        //get a reference to the size of the item we are hitting
        Vector3 hitSr = hit.Col.bounds.size; 
        //get the radius so we have the distance to stop from its center
        Vector2 hitItemsRadius = new Vector2(hitSr.x / 2, hitSr.y / 2);
        //get a vector pointing to our sliding object
        Vector2 sideOfDestination = direction * -1;
        //get the location of the radius we need to stop at
        hitItemsRadius *= sideOfDestination;
        //get our current location of the object we are sliding
        Vector3 currentLocation = this.transform.position;
        Vector2 destination = Vector2.zero;
        float distance = 0;
        //do this if we are trying to slide it up or down. only deal with the y direction
        if (direction == Vector2.down || direction == Vector2.up)
        {

            //get the location we hit
            float hitLocation = hit.Col.transform.position.y;
            //radius time the vector pointing from hit to us
            float myWidthWithSidePos = (getColWidth.y / 2) * (direction.y * -1);
            
            //up (first explaination): it's the location of the hit + the width of the hit position + radius of the sliding block - collider radius + collider width * the direction it is sliding
            //(we are trying to account for the  margin of the top of the collider to the point of the center of the sliding image)
            //down (second explaination): it's the location of the hit + radius of the hit object + radius of the image that is sliding * the direction it is sliding
            //(we are trying to account for the bottom of the moving sprite)
            float yMargin = direction == Vector2.up ? 
                hitLocation + hitItemsRadius.y + myWidthWithSidePos + (transform.GetComponent<SpriteRenderer>().bounds.center.y / 2) - (getColWidth.y / 2) + getColWidth.y:
                hitLocation + hitItemsRadius.y + (transform.GetComponent<SpriteRenderer>().bounds.size.y / 2)  * (direction.y * -1);;
            //set the destination to the it you needs to travel
            destination = new Vector2(currentLocation.x, yMargin);
            //the total distance is the margin of the center of the both items
            distance = Mathf.Abs(_col.bounds.center.y - hit.Col.transform.position.y);
        }
        //do this if we are trying to slide it right or left. only deal with the x direction
        if (direction == Vector2.right || direction == Vector2.left)
        {
            float distanceMargin = hit.Col.transform.position.x;
            float myWidthWithSidePos = (getColWidth.x / 2) * (direction.x * -1);
            destination = new Vector2(distanceMargin + hitItemsRadius.x + myWidthWithSidePos, currentLocation.y);
            distance = Mathf.Abs(_col.bounds.center.x - hit.Col.transform.position.x);

        }
        //get the time for how long it will take to animate there
        float time = MeasureTime(distance);
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
    */
    void EmergencyStopTween()
    {
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
        //todo dynaimcal set the origin points based off the size so that if I want to make the size of the slidable bigger it will dynamically create the points based off how big it is so it can be more than 3

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