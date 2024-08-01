using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOverlapTarget
{
    public float GetPercentOfOverlap(Bounds a, Bounds b);
}
