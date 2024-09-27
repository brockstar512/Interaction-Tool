using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interface;
using Items.SubItems;

public class EnemyPlaceholder : MonoBehaviour, IHurt, IInteractWithHookProjectile
{
    
    public void ApplyDamage(IDamage damagingThing)
    {
        Debug.Log("apply damage");
        Destroy(this.gameObject);
    }

    public void InteractWithHookProjectile(HookProjectile projectile)
    {
        ApplyDamage(projectile);
    }
}
