using UnityEngine;

namespace KeySystem
{

//should be abstract? then slideable time bomb, slideable symbol, slideable plain, moveable? moveable is not a key port its just 
//a landing area so this name should prbably cahneg
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
