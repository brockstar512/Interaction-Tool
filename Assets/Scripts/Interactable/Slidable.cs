using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening.Core;
using Player.ItemOverlap;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.UIElements;


[RequireComponent(typeof(SpriteRenderer))]
public class Slidable : InteractableBase
{
    [SerializeField] private Utilities.KeyTypes key;
    public LayerMask obstructionLayer;
    Vector3 getColWidth { get { return _col.bounds.size; } }
    const int animationDelay = 250;
    [SerializeField] OverlapMoveDamageCheck moverCheckPrefab;
    [SerializeField]  OverlapTargetCheck targetCheckPrefab;
    [SerializeField] const int contactPointCount = 3;
    OverlapTargetCheck _targetCheck;
    OverlapMoveDamageCheck _moverCheck;
    private Collider2D _col;
    private Tweener slideAnimation;
    private List<ClosestContactPointHelper> contactPoints = new List<ClosestContactPointHelper>();
    

    

    void Awake()
    {
        obstructionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.SlidableObstructionLayer);
        _col = GetComponent<Collider2D>();
        UpdateLayerName();
        
        
        
    }

    private void FixedUpdate()
    {
        GetClosestRaycastHit(Vector2.right);
    }

    public override bool Interact(PlayerStateMachineManager state)
    {
        
       return CanMove(state.currentState.LookDirection);
    }


    

    Collider2D GetClosestRaycastHit(Vector2 direction)
    {
        
        for (int i = 0; i < contactPointCount; i++)
        {
            //get the dierctions of raycasts
            ClosestContactPointHelper closestContactPointHelper = new ClosestContactPointHelper(direction,obstructionLayer);
            
            if (direction == Vector2.down || direction == Vector2.up)
            {
                //set their starting pos
                closestContactPointHelper.SetOriginPointX(_col.bounds.extents,i);
            }
            if(direction == Vector2.left || direction == Vector2.right)
            {
                //set their starting pos
                closestContactPointHelper.SetOriginPointY(_col.bounds.extents,i);
            }

            contactPoints.Add(closestContactPointHelper);
        }
        
        
/*
        RaycastHit2D hitSideOne = Physics2D.Raycast(sideOnePos, direction ,int.MaxValue,obstructionLayer);
        Debug.DrawRay(sideOnePos,direction, Color.blue);
        
        RaycastHit2D hitMiddle = Physics2D.Raycast(startPos, direction ,int.MaxValue,obstructionLayer);
        Debug.DrawRay(startPos,direction, Color.red);

        RaycastHit2D hitSideTwo = Physics2D.Raycast(sideTwoPos, direction ,int.MaxValue,obstructionLayer);
        Debug.DrawRay(sideTwoPos,direction, Color.green);

      
        if (direction == Vector2.left || direction == Vector2.right)
        {
            float[] singleCoordinate = 
            {
                float.MaxValue,
                float.MaxValue,
                float.MaxValue,
            };

            if (hitSideOne.collider!=null)
            {
                //singleCoordinate[0] = GetAbsoluteValueOfDistance(hitSideOne.transform.position.x,this.transform.position.x);

            }
            if (hitSideTwo.collider!=null)
            {
               // singleCoordinate[1] =GetAbsoluteValueOfDistance(hitSideTwo.transform.position.x,this.transform.position.x); 

            }
            if (hitMiddle.collider!=null)
            {
                //singleCoordinate[2] = GetAbsoluteValueOfDistance(hitMiddle.transform.position.x,this.transform.position.x);

            }
            
            //float distanceOne = hitSideOne.collider!=null ? Mathf.Abs(hitSideOne.transform.position.x -this.transform.position.x) : int.MaxValue;
            Debug.Log(singleCoordinate[0]);
           // float distanceTwo = hitSideTwo.collider==null ? int.MaxValue: Mathf.Abs(hitSideTwo.transform.position.x -this.transform.position.x);
            Debug.Log(singleCoordinate[1]);
            //float distanceMiddle =  hitMiddle.collider!=null ?Mathf.Abs(hitMiddle.transform.position.x -this.transform.position.x) : int.MaxValue;
            Debug.Log(singleCoordinate[2]);

            //float singleCoordDestination = singleCoordinate.Min();
            int index = -1;
            float singleCoordDestination = float.MaxValue;
            for (int i = 0; i < singleCoordinate.Length; i++)
            {
                if (singleCoordDestination >= singleCoordinate[i])
                {
                    singleCoordDestination = singleCoordinate[i];
                    index = i;
                }
                
            }

            
            Debug.Log($"Winner:  {singleCoordDestination} at index {index}");

        }   

*/
        return null;
    }


  
    // float GetAbsoluteValueOfDistance(float from, float to)
    // {
    //     return Mathf.Abs(from - to);
    // }
    

    bool CanMove(Vector2 direction)
    {
        //ignore collider on querying object
        Physics2D.queriesStartInColliders = false;
        //start at the center of the bounds
        Vector3 startPos = _col.bounds.center;
        //his the destination
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction * 100,int.MaxValue,obstructionLayer);
        //Collider2D closestHit = GetClosestRaycastHit(direction);
        //if we hit a obstruction layer
        //todo if any racast is less than 1
        if (hit.collider != null)
        {
            //Debug.Log(hit.collider.gameObject.name);
            //if we are not right up next to the wall
            if (Mathf.Abs(startPos.x - hit.collider.transform.position.x) <= getColWidth.x && Mathf.Abs(direction.x) == 1)
            {
                return false;
            }
            if (Mathf.Abs(startPos.y - hit.collider.transform.position.y) <= getColWidth.y && Mathf.Abs(direction.y) == 1)
            {
                return false;
            }
            
            //slide the item
            SlideItem(direction, hit);

            return true;
        }
        return false;
    }

    async void SlideItem(Vector2 direction, RaycastHit2D hit)
    {
        
        //get a reference to the size of the item we are hitting
        Vector3 hitSr = hit.collider.GetComponent<SpriteRenderer>().bounds.size; 
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
            float hitLocation = hit.collider.transform.position.y;
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
            distance = Mathf.Abs(_col.bounds.center.y - hit.collider.transform.position.y);
        }
        //do this if we are trying to slide it right or left. only deal with the x direction
        if (direction == Vector2.right || direction == Vector2.left)
        {
            float distanceMargin = hit.collider.transform.position.x;
            float myWidthWithSidePos = (getColWidth.x / 2) * (direction.x * -1);
            destination = new Vector2(distanceMargin + hitItemsRadius.x + myWidthWithSidePos, currentLocation.y);
            distance = Mathf.Abs(_col.bounds.center.x - hit.collider.transform.position.x);

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


        public ClosestContactPointHelper(Vector2 direction, LayerMask detectionLayer)
        {
            _direction = direction;
            Distance = Mathf.Infinity;
            Col = null;
            _originPoint = Vector3.zero;
            _obstructionLayer = detectionLayer;
        }
        //if (direction == Vector2.left ||direction == Vector2.right )
        public void SetOriginPointY(Vector3 callersBoundsExtends, int index)
        {
            _originPoint = callersBoundsExtends;

                float parentSizeY = callersBoundsExtends.y;
                switch (index)
                {
                 case 0:
                     _originPoint.y += parentSizeY;
                     break;
                 case 2:
                     _originPoint.y -= parentSizeY;
                     break;
                 default:
                     break;
                }
        }
        
        //(direction == Vector2.up ||direction == Vector2.down )
        public void SetOriginPointX(Vector3 callersBoundsExtends, int index)
        {
            _originPoint = callersBoundsExtends;
            
                float parentSizeX = callersBoundsExtends.x;
                switch (index)
                {
                    case 0:
                        _originPoint.x += parentSizeX;
                        break;
                    case 2:
                        _originPoint.x -= parentSizeX;
                        break;
                    default:
                        break;
                }
                return;
        }
        
       Collider2D GetCollider()
        {
            RaycastHit2D hit = Physics2D.Raycast(_originPoint, _direction ,int.MaxValue,_obstructionLayer);
            Debug.DrawRay(_originPoint,_direction, Color.blue);
            
            if (hit.collider!=null)
            {
                Col = hit.collider;
               Distance = Mathf.Abs(_originPoint.x - hit.collider.transform.position.x);

            }

            return hit.collider;
        }
       
       
        float GetAbsoluteValueOfDistance(float from, float to)
        {
            return Mathf.Abs(from - to);
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
       
 */