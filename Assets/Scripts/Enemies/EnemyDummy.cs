using UnityEngine;

namespace IT.Enemies
{
    using IT.Core;
    using IT.Items.GrapplingHook;

    public class EnemyDummy : MonoBehaviour, IDamageable, IGrappleTarget
    {
        public void ApplyDamage(int amount, Vector2 sourcePosition)
        {
            Debug.Log("apply damage");
            Destroy(this.gameObject);
        }

        public void InteractWithHookProjectile(GrappleProjectile projectile)
        {
            Debug.Log("apply grappling hook damage");
            ApplyDamage(1, projectile.transform.position);
        }
    }
}
