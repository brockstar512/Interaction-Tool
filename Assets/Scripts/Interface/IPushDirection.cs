using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPushDirection
{
    int Push { get; }
    int Hold { get; }
    public abstract bool IsInputInDirection(Vector2 input);



}
