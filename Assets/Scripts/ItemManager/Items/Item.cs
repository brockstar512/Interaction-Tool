using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item", order = 1)]
public class Item : ScriptableObject, IItem
{
    [SerializeField] Sprite sprite;
    public Sprite Sprite
    {
         get { return sprite; }
       
    }


    //attack points
    public float Damage = 10;
    
    //the pickabable holds this
    public void Use()
    {
        Debug.Log("Attack:" + Damage);
       
    }

}

