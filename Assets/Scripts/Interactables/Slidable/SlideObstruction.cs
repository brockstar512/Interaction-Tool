using System;
using UnityEngine;

namespace IT.Interactables.Slidable
{
    using IT.Core.Utilities;

    public class SlideObstruction : MonoBehaviour
    {
       private void Awake()
       {
          UpdateLayerName();
       }
   
       protected void UpdateLayerName()
       {
          this.gameObject.layer = LayerIndex.SlidableObstruction;
       }
    }
}
