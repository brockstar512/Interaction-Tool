using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusHUD : MonoBehaviour
{
    [SerializeField] Transform Health;
    [SerializeField] Transform Icon;
    [SerializeField] Transform InventoryHolder;
    [SerializeField] Transform Lives;


    //


    public bool HasPlayer()
    {
        return true;
    }

    public void Init()
    {
        
    }


}
