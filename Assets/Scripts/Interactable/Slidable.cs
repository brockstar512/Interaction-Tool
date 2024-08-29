using DG.Tweening;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening.Core;
using Player.ItemOverlap;
using Unity.VisualScripting;


[RequireComponent(typeof(SpriteRenderer))]
public class Slidable : InteractableBase
{
    [SerializeField] private Utilities.KeyTypes key;
    [SerializeField] private LayerMask obstructionLayer;
    [SerializeField] OverlapMoveDamageCheck moverCheckPrefab;
    [SerializeField]  OverlapTargetCheck targetCheckPrefab;
    private OverlapTargetCheck _targetCheck;
    private OverlapMoveDamageCheck _moverCheck;
    private MoveSlideable _mover;
    private Collider2D _col;
    private const int AnimationDelay = 250;


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
            Vector3 getColWidth = _col.bounds.size;
            if (Mathf.Abs(startPos.x - hit.collider.transform.position.x) <= getColWidth.x && Mathf.Abs(direction.x) == 1)
            {
                return false;
            }
            if (Mathf.Abs(startPos.y - hit.collider.transform.position.y) <= getColWidth.y && Mathf.Abs(direction.y) == 1)
            {
                return false;
            }
        }
        //slide the item
        SlideItem(direction, hit);

        return true;
    }

    async void SlideItem(Vector2 direction, RaycastHit2D hit)
    {
        //pause so the character can animate
        await Task.Delay(AnimationDelay);
        //create the movement checks
        _moverCheck =Instantiate( moverCheckPrefab, moverCheckPrefab.transform.position, Quaternion.identity, this.transform);
        _moverCheck.SetDirectionOfOverlap(direction*-1);
        _moverCheck.SetEmergencyStop(EmergencyStop);
        _targetCheck =Instantiate( targetCheckPrefab, this.transform.position + targetCheckPrefab.transform.position, Quaternion.identity, this.transform);
        //animate this to the location and give it a callback when complete
        _mover =this.AddComponent<MoveSlideable>().Init(direction: direction);
        //if the target is on something like water...
        //if the slide damage runs into anything that is not a player

    }
    void EmergencyStop()
    {
        CleanUp();
    }
    
    public override void Release(PlayerStateMachineManager player)
    {
        
    }

    async void CleanUp()
    {
        bool isPlaced = await _targetCheck.IsOnKeyPort(key);
        _moverCheck.CleanUp();
        _targetCheck.CleanUp();
        Destroy(_mover);
        if (isPlaced)
        {
            Destroy(this);
        }

    }
}
