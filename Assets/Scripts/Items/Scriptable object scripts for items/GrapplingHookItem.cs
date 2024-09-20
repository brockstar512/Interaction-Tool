using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.SubItems;
using System;
namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "GrapplingHookItemObject", menuName = "ScriptableObjects/Grappling Hook")]
    public class GrapplingHookItem : Item
    {
        [SerializeField] private HookProjectile projectilePrefab;
        HookProjectile _projectile;
        
        public override void Use(Vector3 playerLocation, Vector3 playerDirection, Action<DefaultState> callbackAction, DefaultState defaultStateArg)
        {
            ItemFinishedCallback = callbackAction;
            DefaultState = defaultStateArg;
            _projectile = Instantiate(projectilePrefab,playerLocation,Quaternion.identity).Init(playerLocation, playerDirection);
            Debug.Log($"Grappling hook {playerDirection}");
        }
        
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(DefaultState);
        }
        
    }
}
