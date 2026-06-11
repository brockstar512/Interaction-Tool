using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Enemies
{
    using IT.Core;
    using IT.Items.GrapplingHook;

    public class EnemyDummy : MonoBehaviour, IDamageable, IGrappleTarget
    {
        //todo i need to handle the logic with how the grappling hook hits the ememy
        public void ApplyDamage(IDamage damagingThing)
        {
            Debug.Log("apply damage");
            Destroy(this.gameObject);
        }

        public void InteractWithHookProjectile(GrappleProjectile projectile)
        {
            Debug.Log("apply grappling hook damage");

            ApplyDamage(projectile);
        }
    }
}
