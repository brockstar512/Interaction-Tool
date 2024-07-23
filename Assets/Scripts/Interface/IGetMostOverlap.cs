using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGetMostOverlap
{
   public InteractableBase GetOverlapObject(Vector2 characterPos, Vector2 lookDirection);
}
