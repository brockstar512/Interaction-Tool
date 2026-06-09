using System;
using UnityEngine;
using UnityEngine.Serialization;

//this should only be attached to one pullable so it does not conflict with others. it should subscrie and pullable not
//know about it so multiple dependants can subscribe to pullable.
public class PullableMoveDependent : MonoBehaviour, IDependencySource<float>
{
    private enum Axis { Horizontal, Vertical }

    [SerializeField] private Axis axis = Axis.Vertical;
    private const float MaxDistance = 3f;   // travel at full pull; negative flips the direction
    private Vector3 _origin;
    public event Action<float> Apply;
    public float Value { get; private set; }


    private void Awake()
    { 
        _origin = transform.position;
        Apply = MoveToLocation;
    }

    private void MoveToLocation(float updateDependentValue)
    {
        Value = updateDependentValue;
        Vector3 dir = axis == Axis.Horizontal ? Vector3.right : Vector3.up;
        transform.position = _origin + dir * (MaxDistance * Value);
    }

}