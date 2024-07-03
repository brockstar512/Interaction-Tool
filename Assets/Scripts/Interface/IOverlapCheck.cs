using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOverlapCheck
{
   //public InteractableBase GetOverlapObject(Vector2 lookDirection);
   public void UpdateCheckPosition(Vector2 lookDirection,Vector3 characterPos);
}
