using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.SubItems{
    
    public class HookProjectile : MonoBehaviour
    {
        [SerializeField] private LineRenderer line;
        [SerializeField] private Transform origin;
        
       
        void Update()
        {
            line.positionCount = 2;
            line.SetPosition(0,origin.transform.position);
            line.SetPosition(1,this.transform.position);

            
        }
    }
}
