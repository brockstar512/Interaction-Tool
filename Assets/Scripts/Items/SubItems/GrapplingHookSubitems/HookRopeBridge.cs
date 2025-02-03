using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HookRopeBridge : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    [SerializeField] private Vector2 bridgeDirection;
    private BoxCollider2D _collider;
    private Collider2D[] _depthsCol;
    private IGetAllOverlap<Collider2D> overlappingDepth { get; set; }

    private void Awake()
    {
        overlappingDepth = GetComponent<IGetAllOverlap<Collider2D>>();
    }

    public async Task Connect(Vector3 start, Vector3 end)
    {
        _collider = GetComponent<BoxCollider2D>();
        this.transform.position = start;
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, end);
        Vector2 min = Vector2.Min(_lineRenderer.GetPosition(0), _lineRenderer.GetPosition(_lineRenderer.positionCount-1));
        Vector2 max = Vector2.Max(_lineRenderer.GetPosition(0), _lineRenderer.GetPosition(_lineRenderer.positionCount-1));
        Vector2 center = (min+max) / 2;
        _collider.offset = (center - (Vector2)transform.position);
        _collider.size = _lineRenderer.bounds.size;
        _depthsCol = await overlappingDepth.GetAllOverlapObject(_lineRenderer.bounds);
        BuildWalls(_depthsCol,Utilities.GetDirectionFromTwoPoints(start, end));
    }

    void BuildWalls(Collider2D[] _depthsCol,Vector2 direction)
    {
        Transform ropeTrans = transform;
        float resizeVal = .25f;

        
        void HorizontalWall()
        {
            foreach (var col in _depthsCol)
            {
                Vector2 moveDestination = new Vector2(col.bounds.center.x - ropeTrans.position.x,_collider.offset.y);
                SpriteRenderer colliderSprite = col.transform.GetComponent<SpriteRenderer>();
                BoxCollider2D colUp = gameObject.AddComponent<BoxCollider2D>();
                BoxCollider2D colDown  = gameObject.AddComponent<BoxCollider2D>();
                colUp.offset = moveDestination;
                colDown.offset = moveDestination;
                Vector2 resizeCompensate = new Vector2(0,resizeVal);
                colUp.size = new Vector2(colliderSprite.bounds.size.x,resizeVal * 2);
                colDown.size = new Vector2(colliderSprite.bounds.size.x,resizeVal * 2);
                colUp.offset += Vector2.up - resizeCompensate;
                colDown.offset += Vector2.down + resizeCompensate;

            }
        }
        
        void VerticalWall()
        {
            foreach (var col in _depthsCol)
            {
                Vector2 moveDestination = new Vector2(_collider.offset.x, col.bounds.center.y - ropeTrans.position.y);
                SpriteRenderer colliderSprite = col.transform.GetComponent<SpriteRenderer>();
                BoxCollider2D colRight = gameObject.AddComponent<BoxCollider2D>();
                BoxCollider2D colLeft  = gameObject.AddComponent<BoxCollider2D>();
                colRight.offset = moveDestination;
                colLeft.offset = moveDestination;
                //resize so it is smaller and we can compansate for it's size not being 1...
                //_collider.size.x=size of the rope. we don't need it to be bulky so
                //i'll make it .25f but it's .25f from its center so it needs to be moves back
                //a little from the difference that it's i taken off of it meaning if I am moving
                //right it's colRight.offset -= (.25f * 2) it's *2 because it needs to compensate 
                //for it's center but this does not work for other compensation resizes ...
                //i can redo the math and figure it out if i want that flexibilty to resize and adjust dynamically
                //(if I want it same size as rope bridge it was _collider.size.x)
                Vector2 resizeCompensate = new Vector2(resizeVal,0);
                colRight.size = new Vector2(resizeVal * 2, colliderSprite.bounds.size.y);
                colLeft.size = new Vector2(resizeVal * 2, colliderSprite.bounds.size.y);
                colRight.offset += Vector2.right - resizeCompensate;
                colLeft.offset += Vector2.left + resizeCompensate;
            }
        }

        if (direction == Vector2.down || direction == Vector2.up)
        {
            VerticalWall();
        }
        if (direction == Vector2.right || direction == Vector2.left)
        {
            HorizontalWall();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(Utilities.PlayerLayer))
        {
            foreach (var col in _depthsCol)
            {
                Physics2D.IgnoreCollision(other.GetComponent<Collider2D>(), col, true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(Utilities.PlayerLayer))
        {
            foreach (var col in _depthsCol)
            {
                Physics2D.IgnoreCollision(other.GetComponent<Collider2D>(), col, false);
            }
        }
    }
}
