using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HookRopeBridge : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    [SerializeField] private Vector2 bridgeDirection;



    public void Connect(Vector3 start, Vector3 end)
    {
        this.transform.position = start;
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, end);
    }
}
