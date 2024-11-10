using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interface;
using Items.SubItems;

public class EnemyPlaceholder : MonoBehaviour, IHurt, IInteractWithHookProjectile
{
    //todo i need to handle the logic with how the grappling hook hits the ememy
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
