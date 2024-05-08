using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDReader : MonoBehaviour
{
    public PlayerStatus player;
    private List<PlayerStatus> currentPlayers;
    //this will subscribe to all events
    //damage/ health 

    void Start()
    {
        
    }

    public void Init()
    {
     //return the player   
    }

    /*
     * public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
{
    T tmp = list[indexA];
    list[indexA] = list[indexB];
    list[indexB] = tmp;
    return list;
}
    */
}
