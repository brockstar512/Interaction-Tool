using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.SubItems{
    
    public class HookProjectile : MonoBehaviour
    {
        [SerializeField] private LineRenderer line;
        [SerializeField] private Vector3 origin;
        [SerializeField] private Vector3 direction;
        [SerializeField] private float speed;


        public HookProjectile Init(Vector3 origin, Vector3 direction)
        {
            //should the grappling hook item be in control of moving it... this should just be instantiated and keep track of what interects it
            this.origin = origin;
            this.direction = direction;
            return this;
        }
        
       
        void Update()
        {
            Move();
            line.positionCount = 2;
            line.SetPosition(0, origin);
            line.SetPosition(1, this.transform.position);
        }

        void Move()
        {
            transform.Translate(direction* speed * Time.deltaTime);
        }
        
    }
}
