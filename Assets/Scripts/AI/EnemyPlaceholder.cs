using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlaceholder : MonoBehaviour, IHurt
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplyDamage(IDamage damagingThing)
    {
        Debug.Log("Reached apply damage");
        Destroy(this.gameObject);
    }
    

}
