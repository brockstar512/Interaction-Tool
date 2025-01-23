using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(LineRenderer))]
public class HookRopeBridge : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    [SerializeField] private Vector2 bridgeDirection;
    private BoxCollider2D _collider;




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
        //_collider.size = _lineRenderer.bounds.size/2;

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
