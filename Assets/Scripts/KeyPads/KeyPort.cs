using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace KeySystem
{


    public class KeyPort : MonoBehaviour
    {
        [SerializeField] private Utilities.KeyTypes keyPort;
        
        public bool Lock(Utilities.KeyTypes keyType)
        {
            bool result = keyType == keyPort ? true : false ;
            Debug.Log(result);

            //make this so no other keys can check it.
            //animate it
            return result;
        }
        
    }
}
/*
 * private void OnTriggerEnter2D(Collider2D other)
   {
       Debug.Log("Trigger" + other.gameObject.name);
       var key = other.GetComponent<OverlapTargetCheck>();
       if (key != null)
       {
           Debug.Log($"Slidable is here {key.gameObject.name}");
           Debug.Log($"Slidable is here {GetBounds}");
       
       }
   }
 */