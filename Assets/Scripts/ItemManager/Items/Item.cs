using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item", order = 1)]
public class Item : ScriptableObject, IItem
{
    //icon
    public Sprite Sprite;
    //animation controller
    //[SerializeField] Animation Animation;
    //[SerializeField] Animator Animation;

    //attack points
    float Damage = 10;
    
    //the pickabable holds this
    public void Use()
    {
        Debug.Log("Attack:" + Damage);
       
    }

}

