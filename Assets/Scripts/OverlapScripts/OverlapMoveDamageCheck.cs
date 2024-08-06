using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OverlapHelpers;
namespace Player.ItemOverlap
{
    public class OverlapMoveDamageCheck : OverlapObjectCheck, IDamage
    {

        OverlapMoveDamageCheckHelper _helper;
        void Awake()
        {
            //remove
            //detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.InteractableLayer));
            _helper = new OverlapMoveDamageCheckHelper();
        }
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
        
        public void SetDirectionOfOverlap(Vector3 playerLookDirection)
        {
            this.transform.localScale = _helper.UpdateScale(playerLookDirection);
            this.transform.localPosition = _helper.UpdatePosition(playerLookDirection);
        }

        void SlideCollision(Collider2D collision)
        {
            Debug.Log($"Collided with {collision.gameObject.name}");
            IHurt collidedSubject = collision.GetComponent<IHurt>();
            if (collidedSubject is not null)
            {
                
                //
                Debug.Log("hur item");
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
