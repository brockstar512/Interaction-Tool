using System;
using DG.Tweening;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening.Core;
using Player.ItemOverlap;
using Unity.VisualScripting;
using System.Linq;


[RequireComponent(typeof(SpriteRenderer))]
public class Slidable : InteractableBase
{
    [SerializeField] private Utilities.KeyTypes key;
    public LayerMask obstructionLayer;
    Vector3 getColWidth { get { return _col.bounds.size; } }
    const int animationDelay = 250;
    [SerializeField] OverlapMoveDamageCheck moverCheckPrefab;
    [SerializeField]  OverlapTargetCheck targetCheckPrefab;
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
        Collider2D resultingHit = null;
        Physics2D.queriesStartInColliders = false;

        Vector3 startPos = _col.bounds.center;
        Vector3 sideOnePos = _col.bounds.center;
        Vector3 sideTwoPos = _col.bounds.center;


        if (direction == Vector2.left ||direction == Vector2.right )
        {
            sideOnePos.y += _col.bounds.extents.y;
            sideTwoPos.y -= _col.bounds.extents.y;

        }
        
        if (direction == Vector2.up ||direction == Vector2.down )
        {
            sideOnePos.x += _col.bounds.extents.x;
            sideTwoPos.x -= _col.bounds.extents.x;
        }
        

        RaycastHit2D hitSideOne = Physics2D.Raycast(sideOnePos, direction ,int.MaxValue,obstructionLayer);
        Debug.DrawRay(sideOnePos,direction, Color.blue);
        
        RaycastHit2D hitMiddle = Physics2D.Raycast(startPos, direction ,int.MaxValue,obstructionLayer);
        Debug.DrawRay(startPos,direction, Color.red);

        RaycastHit2D hitSideTwo = Physics2D.Raycast(sideTwoPos, direction ,int.MaxValue,obstructionLayer);
        Debug.DrawRay(sideTwoPos,direction, Color.green);
        
            // float distanceOne = float.MaxValue;
            // float distanceTwo = float.MaxValue;
            // float distanceMiddle = float.MaxValue;
      
        if (direction == Vector2.left || direction == Vector2.right)
        {
            float[] numbers = new float[]
            {
                float.MaxValue,
                float.MaxValue,
                float.MaxValue,
            };

            if (hitSideOne.collider!=null)
            {
                numbers[0] = Mathf.Abs(hitSideOne.transform.position.x - this.transform.position.x);

            }
            if (hitSideTwo.collider!=null)
            {
                numbers[1] = Mathf.Abs(hitSideTwo.transform.position.x -this.transform.position.x);

            }
            if (hitMiddle.collider!=null)
            {
                numbers[2] = Mathf.Abs(hitMiddle.transform.position.x - this.transform.position.x);

            }
            
            //float distanceOne = hitSideOne.collider!=null ? Mathf.Abs(hitSideOne.transform.position.x -this.transform.position.x) : int.MaxValue;
            Debug.Log(numbers[0]);
           // float distanceTwo = hitSideTwo.collider==null ? int.MaxValue: Mathf.Abs(hitSideTwo.transform.position.x -this.transform.position.x);
            Debug.Log(numbers[1]);
            //float distanceMiddle =  hitMiddle.collider!=null ?Mathf.Abs(hitMiddle.transform.position.x -this.transform.position.x) : int.MaxValue;
            Debug.Log(numbers[2]);

            float singleCoordDestination = numbers.Min();
            Debug.Log("Winner: "+ singleCoordDestination);




        }   


        return resultingHit;
    }

    float GetAbsoluteValueOfDistance(float from, float to)
    {
        return Mathf.Abs(from - to);
    }
    

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
}
