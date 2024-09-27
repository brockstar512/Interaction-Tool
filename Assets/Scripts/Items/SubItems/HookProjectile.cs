using UnityEngine;
using Player.ItemOverlap;
using System;
using Interface;

namespace Items.SubItems{
    
    public class HookProjectile : MonoBehaviour, IDamage
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
        Action<Collider2D> _hitSomethingCallback;
        private void Awake()
        {
            _overlapHookCheck = GetComponentInChildren<OverlapHookCheck>();
        }

        public HookProjectile Init(Vector3 originPoint, Action<Collider2D> hitSomethingCallback)
        {
            this.origin = originPoint;
            _hitSomethingCallback = hitSomethingCallback;
            //AddDetectionLayers();
            return this;
        }

        private void FixedUpdate()
        {
            Collider2D col = _overlapHookCheck.GetMostOverlappedCol();
            //this will only run is we hit something and the callback is not null
            //once we invoke the call back it will be null
            if (col is null && _hitSomethingCallback is not null)
                return;
            _hitSomethingCallback?.Invoke(col);
            _hitSomethingCallback = null;
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
            DrawLineConnector();
        }

        void DrawLineConnector()
        {
            line.positionCount = 2;
            line.SetPosition(0, origin);
            line.SetPosition(1, this.transform.position);
        }
        
    }
}
