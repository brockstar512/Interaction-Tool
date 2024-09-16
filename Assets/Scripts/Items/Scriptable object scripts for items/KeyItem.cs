using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Scriptable_object_scripts_for_items
{
   [CreateAssetMenu(fileName = "KeyItemObject", menuName = "ScriptableObjects/Key")]
   public class Key : Item
   {
      public override void Use()
      {
         Debug.Log("Key");

      }
   }
}
