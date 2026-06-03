using System;
using UnityEngine;

public class Obstruction : MonoBehaviour
{
   private void Awake()
   {
      UpdateLayerName();
   }
   
   protected void UpdateLayerName()
   {
      this.gameObject.layer = LayerMask.NameToLayer(Utilities.SlidableObstructionLayer);
   }
}
