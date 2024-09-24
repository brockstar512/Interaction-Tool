using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using Player.ItemOverlap;
using Unity.VisualScripting;

namespace Items.SubItems{
    
    public class HookProjectile : MonoBehaviour
    {
        [SerializeField] private LineRenderer line;
        [SerializeField] private Vector3 origin;
        [SerializeField] private float speed;
        [SerializeField] private Sprite rightSprite;
        [SerializeField] private Sprite leftSprite;
        [SerializeField] private Sprite downSprite;
        [SerializeField] private Sprite upSprite;
        private SpriteRenderer _sr;
        private OverlapHookCheck _overlapHookCheck;

        private void Awake()
        {
            _overlapHookCheck = GetComponentInChildren<OverlapHookCheck>();
        }

        public HookProjectile Init(Vector3 originPoint)
        {
            this.origin = originPoint;
            //AddDetectionLayers();
            return this;
        }

        private void FixedUpdate()
        {
            Collider2D col = _overlapHookCheck.GetMostOverlappedCol();
            if (col != null)
            {
                //we only care about hooks or throwables
                Debug.Log($"This is the one we want {col.gameObject.name}");

                
            }
        }

        public void BuildCallback()
        {
            //pass in the argument with what I need to do whe I hit something... that means the state ..
            //this could be better set up in the action function of the item
            //the outcomes are 
            //1) it hits and objectruction
            //2) its hits and interactbale
            //3) it hits a bad guy
            //4) it hits a hook for a path
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
