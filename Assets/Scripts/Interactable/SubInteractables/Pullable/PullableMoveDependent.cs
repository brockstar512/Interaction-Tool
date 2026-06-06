using UnityEngine;
//this should only be attached to one pullable so it does not conflict with others. it should subscrie and pullable not
//know about it so multiple dependants can subscribe to pullable.
public class PullableMoveDependent : MonoBehaviour, IPullDependent
{
    private enum Axis { Horizontal, Vertical }

    [SerializeField] private Axis axis = Axis.Vertical;
    [SerializeField] private float distance = 3f;   // travel at full pull; negative flips the direction

    private Vector3 _origin;

    private void Awake() => _origin = transform.position;

    public void OnPullChanged(float amount)
    {
        Vector3 dir = axis == Axis.Horizontal ? Vector3.right : Vector3.up;
        transform.position = _origin + dir * (distance * amount);
    }
}