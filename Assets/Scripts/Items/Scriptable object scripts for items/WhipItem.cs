using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "WhipItemObject", menuName = "ScriptableObjects/Whip")]

    public class WhipItem : Item
    {
        public override void Use(Vector3 playerDirection)
        {
            Debug.Log($"Whip {playerDirection}");

        }
    }
}
