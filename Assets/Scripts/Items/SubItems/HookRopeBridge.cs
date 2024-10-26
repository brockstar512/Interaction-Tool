using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HookRopeBridge : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    [SerializeField] private Vector2 bridgeDirection;



    public void Connect(Vector2 start, Vector2 end)
    {
        this.transform.position = start;
        Vector2 currentDirection = (end - start).normalized;
        Debug.Log($"this incoming direction {currentDirection} and this is the start of the end bridge direction {bridgeDirection}");
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, end);
    }
}
