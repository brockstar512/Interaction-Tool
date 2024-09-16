using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "CandleItemObject", menuName = "ScriptableObjects/Candle")]
    public class CandleItem : Item
    {
        public override void Use()
        {
            Debug.Log("Candle");

        }
    }
}
