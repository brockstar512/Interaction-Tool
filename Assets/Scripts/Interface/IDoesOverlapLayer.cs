using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDoesOverlapLayer
{
    public bool DoesOverlap();
    public float GetPercentOfOverlap(Bounds a, Bounds b);
    public void SetDirectionOfOverlap(Vector3 playerLookDirection);
}
