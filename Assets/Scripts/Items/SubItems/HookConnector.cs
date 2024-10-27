using System;
using System.Collections.Generic;
using UnityEngine;
using Interface;
using Unity.VisualScripting;

namespace Items.SubItems
{
    [RequireComponent(typeof(Collider2D))]
    public class HookConnector : MonoBehaviour, IInteractWithHookProjectile
    {
        private LineRenderer _lineRenderer;
        [SerializeField] private HookRopeBridge hookRopeBridgePrefab;
       

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            GetComponent<Collider2D>().isTrigger = true;
        }

        public void InteractWithHookProjectile(HookProjectile projectile)
        {
            if (projectile.hookConnectorStartPin.gameObject == projectile.hookConnectorEndPin.gameObject)
            {
                return;
            }

            
            Instantiate(hookRopeBridgePrefab).Connect(projectile.hookConnectorStartPin.transform.position,projectile.hookConnectorEndPin.transform.position);
        }
        
       

       private void OnDestroy()
       {
           Collider2D attachedCollider2D = this.GetComponent<Collider2D>();
           Destroy(attachedCollider2D);
       }
    }
}
