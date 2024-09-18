using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "PlankItemObject", menuName = "ScriptableObjects/Plank")]

    public class PlankItem : Item
    {
        public override void Use(Vector3 playerDirection)
        {
            Debug.Log($"PLank {playerDirection}");

        }
    }
}
    
