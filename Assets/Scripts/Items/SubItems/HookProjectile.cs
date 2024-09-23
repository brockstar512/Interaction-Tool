using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.SubItems{
    
    public class HookProjectile : MonoBehaviour
    {
        [SerializeField] private LineRenderer line;
        [SerializeField] private Vector3 origin;
        // [SerializeField] private Vector3 direction;
        [SerializeField] private float speed;
        [SerializeField] private Sprite rightSprite;
        [SerializeField] private Sprite leftSprite;
        [SerializeField] private Sprite downSprite;
        [SerializeField] private Sprite upSprite;
        private SpriteRenderer _sr;
        public HookProjectile Init(Vector3 originPoint)
        {
            this.origin = originPoint;
            return this;
        }
        
        public void SetHookSprite(Vector3 spriteDirection)
        {   
            _sr = GetComponent<SpriteRenderer>();
            if (spriteDirection == Vector3.right)
            {
                _sr.sprite = rightSprite;
            }
            if (spriteDirection == Vector3.left)
            {
                _sr.sprite = leftSprite;
            }
            if (spriteDirection == Vector3.up)
            {
                _sr.sprite = upSprite;
            }
            if (spriteDirection == Vector3.down)
            {
                _sr.sprite = downSprite;

            }
            
        }
        
       
        void Update()
        {
            line.positionCount = 2;
            line.SetPosition(0, origin);
            line.SetPosition(1, this.transform.position);
        }
        
        
    }
}
