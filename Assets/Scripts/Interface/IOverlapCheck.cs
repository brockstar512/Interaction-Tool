using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOverlapCheck
{
   public InteractableBase GetOverlapObject();
   public void UpdateCheckPosition(Vector2 lookDirection);
}
