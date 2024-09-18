using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "SwordItemObject", menuName = "ScriptableObjects/Sword")]
    public class SwordItem : Item
    {
        public override void Use(Vector3 playerDirection)
        {
            Debug.Log($"Sword {playerDirection}");

        }
    }
}
