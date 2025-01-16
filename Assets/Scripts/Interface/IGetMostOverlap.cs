using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGetMostOverlap<T>
{
   public T GetOverlapObject(Vector2 characterPos, Vector2 lookDirection);
}
