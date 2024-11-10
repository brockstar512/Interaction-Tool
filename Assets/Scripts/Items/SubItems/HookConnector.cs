using UnityEngine;
using Interface;

namespace Items.SubItems
{
    [RequireComponent(typeof(Collider2D))]
    public class HookConnector : MonoBehaviour, IInteractWithHookProjectile
    {
        //the prefab for the rope bridge for when we connect with two pins
        [SerializeField] private HookRopeBridge hookRopeBridgePrefab;
        
        private void Awake()
        {
            //make sure we our trigger is true so we can walk over it
            GetComponent<Collider2D>().isTrigger = true;
        }

        public void InteractWithHookProjectile(HookProjectile projectile)
        {
            //make sure that the hook that you are connecting with is not itself
            if (projectile.hookConnectorStartPin.gameObject == projectile.hookConnectorEndPin.gameObject)
            {
                return;
            }
            //instantiate the bridge
            Instantiate(hookRopeBridgePrefab).Connect(projectile.hookConnectorStartPin.transform.position,projectile.hookConnectorEndPin.transform.position);
        }
        
       private void OnDestroy()
       {
           //the projectile will detsroy this if it has something to connect to
           Collider2D attachedCollider2D = this.GetComponent<Collider2D>();
           Destroy(attachedCollider2D);
       }
    }
}
