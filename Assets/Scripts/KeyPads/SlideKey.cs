using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class SlideKey : MonoBehaviour
{
    public Bounds GetBounds => this.GetComponent<SpriteRenderer>().bounds;

    void Unlocked()
    {
        
    }
}
