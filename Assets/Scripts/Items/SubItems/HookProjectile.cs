using UnityEngine;
using Player.ItemOverlap;
using System;
namespace Items.SubItems{
    
    public class HookProjectile : MonoBehaviour, IDamage
    {
        [SerializeField] private LineRenderer line;
        [SerializeField] private Vector3 origin;
        [SerializeField] private Vector3 playerPos;
        [SerializeField] private float speed;
        [SerializeField] private Sprite rightSprite;
        [SerializeField] private Sprite leftSprite;
        [SerializeField] private Sprite downSprite;
        [SerializeField] private Sprite upSprite;
        [SerializeField] private OverlapHookSocketCheck hookOverlapPrefab;
        private SpriteRenderer _sr;
        private OverlapHookCheck _overlapHookCheck;
        OverlapHookSocketCheck _hookStartOverlap;
        OverlapHookSocketCheck _hookEndOverlap;
        private bool _hasStartPin = false;
        public HookConnector hookConnectorStartPin { get; private set; } = null;
        public HookConnector hookConnectorEndPin { get; private set; } = null;
         

        Action<Collider2D> _hitSomethingCallback;
        private void Awake()
        {
            _overlapHookCheck = GetComponentInChildren<OverlapHookCheck>();
        }

        public HookProjectile Init(Vector3 gunBarrel, Action<Collider2D> hitSomethingCallback,Vector3 playerLocation)
        {
            this.playerPos = playerLocation;
            this.origin = gunBarrel;
            _hitSomethingCallback = hitSomethingCallback;
            _hookStartOverlap = Instantiate(hookOverlapPrefab, playerLocation, Quaternion.identity);
            _hookEndOverlap = Instantiate(hookOverlapPrefab, playerLocation, Quaternion.identity);

            CheckForStartPin();
            return this;
        }

        async void CheckForStartPin()
        {
            hookConnectorStartPin = await _hookStartOverlap.GetMostOverlappedHookStartCol(playerPos);
            if (hookConnectorStartPin is null)
                return;
            
            Utilities.PutObjectOnLayer(Utilities.SocketUsedLayer,hookConnectorStartPin.gameObject);
            _hasStartPin = true;
        }
        
        void CheckForEndPin()
        {
            //Debug.Log(this.transform.position);
            // _hookEndOverlap.gameObject.transform.name = "End checker";
            hookConnectorEndPin = _hookEndOverlap.GetMostOverlappedHookEndCol(this.transform.position);
            // Debug.Log($"Looking for end overlap did I find it? {hookConnectorEndPin is not null}");
            if (hookConnectorEndPin is not null)
            {
                Utilities.PutObjectOnLayer(Utilities.SocketUsedLayer,hookConnectorStartPin.gameObject);
                ConnectPin();

            }
            
        }

        void ConnectPin()
        {
            if (hookConnectorEndPin is not null && hookConnectorStartPin is not null)
            {
                Utilities.PutObjectOnLayer(Utilities.SocketUsedLayer,hookConnectorEndPin.gameObject);
                hookConnectorStartPin.InteractWithHookProjectile(this);
                Destroy(this.gameObject);
            }
        }
        
//todo put items on back on unhooked layers on destroy. in interactwithhook projectils destroy the other items

        private void FixedUpdate()
        {
            //ConnectBridge();
            if (_hasStartPin)
            {
                CheckForEndPin();
            }
            
            Collider2D col = _overlapHookCheck.GetMostOverlappedCol();
            //this will only run is we hit something and the callback is not null
            //once we invoke the call back it will be null
            if (col is null && _hitSomethingCallback is not null)
                return;
            // Debug.Log($"Hit {col.gameObject.name}");
            // _hitSomethingCallback?.Invoke(col);
            // _hitSomethingCallback = null;
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
        

        void OnDestroy()
        {

            if (hookConnectorStartPin != null)
            {
                Utilities.PutObjectOnLayer(Utilities.SocketUnusedLayer, hookConnectorStartPin.gameObject);
            }

            if (hookConnectorEndPin != null)
            {
                Utilities.PutObjectOnLayer(Utilities.SocketUnusedLayer, hookConnectorEndPin.gameObject);
            }


            Destroy(_hookStartOverlap.gameObject);
            Destroy(_hookEndOverlap.gameObject);

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
