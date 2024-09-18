using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "BellItemObject", menuName = "ScriptableObjects/Bell")]
    public class BellItem : Item
    {
        //animations should be here
        public override void Use(Vector3 playerDirection)
        {
            Debug.Log($"Bell {playerDirection}");

        }
    }
}
