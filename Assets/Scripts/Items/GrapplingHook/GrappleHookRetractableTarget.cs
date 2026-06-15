using System;
using UnityEngine;

namespace IT.Interactables.Throwable
{
    using IT.Items.GrapplingHook;

    [RequireComponent(typeof(Collider2D))]
    public class GrappleHookRetractableTarget : MonoBehaviour, IGrappleTarget
    {
        //fired when the carrier should drop its reference
        //(destroyed, exploded, released)
        public event Action Detached;
        //fired when grapple attaches; useful for things like pausing a bomb fuse
        public event Action Grappled;

        GrappleProjectile _carrier;
        Collider2D _col;

        void Awake() => _col = GetComponent<Collider2D>();

        public void InteractWithHookProjectile(GrappleProjectile projectile)
        {
            _carrier = projectile;
            //make it a trigger so it can be retracted without interrupting anything on the way back
            _col.isTrigger = true;
            //parent to the projectile so it rides the rope back
            transform.SetParent(projectile.transform);
            Grappled?.Invoke();
        }

        void OnDestroy()
        {
            //if we die mid-flight (bomb exploded, enemy killed, scene unloaded),
            //tell the carrier so it doesn't hold a dead reference.
            Detached?.Invoke();
        }
    }
}