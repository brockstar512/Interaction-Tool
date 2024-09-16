using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "BellItemObject", menuName = "ScriptableObjects/Bell")]
    public class BellItem : Item
    {
        public override void Use()
        {
            Debug.Log("Bell");

        }
    }
}
