using System;
using System.Threading.Tasks;
using Player.ItemOverlap;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HookRopeBridge : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    [SerializeField] private Vector2 bridgeDirection;
    private BoxCollider2D _collider;
    private BoxCollider2D _sideOne;
    private BoxCollider2D _sideTwo;
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
        return;
        Debug.Log(direction);

        void HorizontalWall()
        {
            foreach (var col in _depthsCol)
            {
                BoxCollider2D colUp = gameObject.AddComponent<BoxCollider2D>();
                BoxCollider2D colDown  = gameObject.AddComponent<BoxCollider2D>();
                // _sideOne.offset = _collider.offset;
                // _sideTwo.offset = _collider.offset;
                // _sideOne.size = _collider.size;
                // _sideTwo.size = _collider.size;
                // _sideTwo.offset += Vector2.right;
                // _sideOne.offset += Vector2.left;
            }
        }
        
        void VerticalWall()
        {
            foreach (var col in _depthsCol)
            {
                BoxCollider2D colRight = gameObject.AddComponent<BoxCollider2D>();
                BoxCollider2D colLeft  = gameObject.AddComponent<BoxCollider2D>();
                // _sideOne.offset = _collider.offset;
                // _sideTwo.offset = _collider.offset;
                // _sideOne.size = _collider.size;
                // _sideTwo.size = _collider.size;
                // _sideTwo.offset += Vector2.right;
                // _sideOne.offset += Vector2.left;
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
        if (other.CompareTag("Player"))
        {
            // Physics2D.IgnoreCollision(other.GetComponent<Collider2D>(), blackBoxCollider, true);

            foreach (var col in _depthsCol)
            {
                Physics2D.IgnoreCollision(other.GetComponent<Collider2D>(), col, true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            // Physics2D.IgnoreCollision(other.GetComponent<Collider2D>(), blackBoxCollider, false);
            foreach (var col in _depthsCol)
            {
                Physics2D.IgnoreCollision(other.GetComponent<Collider2D>(), col, false);
            }
        }
    }
}
