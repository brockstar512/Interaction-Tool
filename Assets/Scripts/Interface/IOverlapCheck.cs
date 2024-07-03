using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOverlapCheck
{
   public InteractableBase GetOverlapObject(Vector3 characterPos);
   public void UpdateCheckPosition(Vector2 lookDirection);
}
