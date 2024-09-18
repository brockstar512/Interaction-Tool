using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.SubItems;
namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "GrapplingHookItemObject", menuName = "ScriptableObjects/Grappling Hook")]
    public class GrapplingHookItem : Item
    {
        [SerializeField] private HookProjectile projectilePrefab;
        HookProjectile _projectile;
        
        public override void Use(Vector3 playerLocation, Vector3 playerDirection)
        {
            _projectile = Instantiate(projectilePrefab,playerLocation,Quaternion.identity).Init(playerLocation, playerDirection);
            Debug.Log($"Grappling hook {playerDirection}");
        }
    }
}
