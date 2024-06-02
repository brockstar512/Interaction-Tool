using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HealthBox : MonoBehaviour
{
    //heath
    [SerializeField] PlayerStatus player;
        //this other object hits trigger enter calls itself to get this script
        //effect health so it also controlls you twitch sprite and collider
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if(collision.transform.GetComponent<AnimationPushAndPull>() != null)
        //    {
        //}


            
    }
}
