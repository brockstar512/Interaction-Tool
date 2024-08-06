using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.ItemOverlap
{
    public class OverlapMoveDamageCheck : OverlapMoveCheck, IDamage
    {


        // Update is called once per frame
        private void Update()
        {
            SetMovingOverlappingArea(this.transform.position);
        }

        void FixedUpdate()
        {
            //not updating in the right position
            
            Collider2D[] col = GetAllOverlappedCol();
            if (col.Length < 1)
            {
                return;
            }

            foreach (Collider2D collision in col)
            {
                SlideCollision(collision);
            }
            
        }

        void SlideCollision(Collider2D collision)
        {
            //Debug.Log($"Collided with {collision.gameObject.name}");
            IHurt collidedSubject = collision.GetComponent<IHurt>();
            if (collidedSubject is not null)
            {
                
                //
               // Debug.Log("hur item");
                //IDamage dagamer = this;
                collidedSubject.ApplyDamage(this);
                
            }
            //Destroy(collision.gameObject);
        }
        
        

        public void CleanUp()
        {
            Destroy(this.gameObject);
        }

    }
}
