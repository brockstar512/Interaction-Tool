using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDoesOverlapLayer
{
    public bool DoesOverlap(Vector2 itemLocation);
    
    public void SetDirectionOfOverlap(Vector3 playerLookDirection);
}
