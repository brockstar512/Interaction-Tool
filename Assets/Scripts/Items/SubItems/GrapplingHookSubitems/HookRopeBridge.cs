using System;
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
    // [SerializeField] OverlapRopeObstructionCheck overlappingDepthPrefab;

    private IGetAllOverlap<Collider2D> overlappingDepth { get; set; }

    private void Awake()
    {
        overlappingDepth = GetComponent<IGetAllOverlap<Collider2D>>();
    }

    public void Connect(Vector3 start, Vector3 end)
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

        //this will be different depending on the direction
        // _sideOne = gameObject.AddComponent<BoxCollider2D>();
        // _sideTwo = gameObject.AddComponent<BoxCollider2D>();
        // _sideOne.offset = _collider.offset;
        // _sideTwo.offset = _collider.offset;
        // _sideOne.size = _collider.size;
        // _sideTwo.size = _collider.size;
        // _sideTwo.offset += Vector2.right;
        // _sideOne.offset += Vector2.left;
        //iterate over each deptj collider and instanitate a blocker for each size that it overlaps
        overlappingDepth.Hello();
        Collider2D[] depths = overlappingDepth.GetAllOverlapObject(_lineRenderer.bounds.size);
        // overlappingDepth =Instantiate(overlappingDepthPrefab);//.Connect(projectile.hookConnectorStartPin.transform.position,projectile.hookConnectorEndPin.transform.position);
        //Debug.Break();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Physics2D.IgnoreCollision(other.GetComponent<Collider2D>(), blackBoxCollider, true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            // Physics2D.IgnoreCollision(other.GetComponent<Collider2D>(), blackBoxCollider, false);
        }
    }
}
