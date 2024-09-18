using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.SubItems;
namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "GrapplingHookItemObject", menuName = "ScriptableObjects/Grappling Hook")]
    public class GrapplingHookItem : Item
    {
        [SerializeField] private HookProjectile projectile;
        
        public override void Use()
        {
            Debug.Log("Grappling hook");
        }
    }
}
