using System;
using UnityEngine;

namespace IT.Interactables.Slidable
{
    using IT.Core.Utilities;

    public class Obstruction : MonoBehaviour
    {
       private void Awake()
       {
          UpdateLayerName();
       }
   
       protected void UpdateLayerName()
       {
          this.gameObject.layer = Layers.SlidableObstruction;
       }
    }
}
