using DG.Tweening;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening.Core;
using Player.ItemOverlap;


[RequireComponent(typeof(SpriteRenderer))]
public class Slidable : InteractableBase
{
    [SerializeField] private Utilities.KeyTypes key;
    public LayerMask obstructionLayer;
    Vector3 GetWidth { get { return _col.bounds.size; } }
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

    public override bool Interact(PlayerStateMachineManager state)
    {
        
       return CanMove(state.currentState.LookDirection);
    }

    bool CanMove(Vector2 direction)
    {
        //ignore collider on querying object
        Physics2D.queriesStartInColliders = false;
        //start at the center of the bounds
        Vector3 startPos = _col.bounds.center;
        //his the destination
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction * 100,int.MaxValue,obstructionLayer);
        //if we hit a obstruction layer
        if (hit.collider != null)
        {
            //Debug.Log(hit.collider.gameObject.name);
            //if we are not right up next to the wall
            if (Mathf.Abs(startPos.x - hit.collider.transform.position.x) <= GetWidth.x && Mathf.Abs(direction.x) == 1)
            {
                return false;
            }
            if (Mathf.Abs(startPos.y - hit.collider.transform.position.y) <= GetWidth.y && Mathf.Abs(direction.y) == 1)
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
        // Debug.Log(_col.bounds.center);
        // Debug.Log(this.transform.position);
        //(-1,-.37,0.0)
        //(-1,0,0)

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
            float distanceMargin = hit.collider.transform.position.y;
            //radius time the vector pointing from hit to us
            float myWidthWithSidePos = (GetWidth.y / 2) * (direction.y * -1);
            
            //mark the destination as the distance you have to travel + the -of the hits radius + - of myRadius of the side that will be touching
            //+GetWidth.y will get me to halway on the collider... //distanceMargin + hitItemsRadius.y + myWidthWithSidePos + GetWidth.y;
            //we want to get the different of the radius of the collider and the sprtie image if we are going up to account for 3D
            Debug.Log((transform.GetComponent<SpriteRenderer>().bounds.center.y / 2) - (GetWidth.y / 2) + " or " + GetWidth.y);
            float yMargin = direction == Vector2.up ? 
                distanceMargin + hitItemsRadius.y + myWidthWithSidePos + (transform.GetComponent<SpriteRenderer>().bounds.center.y / 2) - (GetWidth.y / 2) + GetWidth.y :
                distanceMargin + hitItemsRadius.y + myWidthWithSidePos + GetWidth.y;
            
            destination = new Vector2(currentLocation.x, yMargin);
            //the total distance is the margin of the center of the both items
            distance = Mathf.Abs(_col.bounds.center.y - hit.collider.transform.position.y);
        }
        //do this if we are trying to slide it right or left. only deal with the x direction
        if (direction == Vector2.right || direction == Vector2.left)
        {
            float distanceMargin = hit.collider.transform.position.x;
            float myWidthWithSidePos = (GetWidth.x / 2) * (direction.x * -1);
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
