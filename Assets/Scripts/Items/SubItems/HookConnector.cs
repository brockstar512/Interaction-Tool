using UnityEngine;
using Interface;
namespace Items.SubItems
{
    [RequireComponent(typeof(Collider2D))]
    public class HookConnector : MonoBehaviour, IInteractWithHookProjectile
    {
        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            GetComponent<Collider2D>().isTrigger = true;
        }

        public void InteractWithHookProjectile(HookProjectile projectile)
        {
            
        }
    }
}
