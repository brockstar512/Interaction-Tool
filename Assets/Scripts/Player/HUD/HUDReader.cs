using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDReader : MonoBehaviour
{
    //singleton?
    public PlayerStatus player;
    private List<PlayerStatus> currentPlayers;
    //this will subscribe to all events
    //damage/ health
    [SerializeField] Transform PlayerHolder;
    [SerializeField] Transform PlayerUIPrefab;

    //[SerializeField] Timer timer;

    //seperate player UI logic for mulitple players?-> player status


    void Start()
    {
        
    }

    private void Update()
    {
        //on space create new player to get UI better
        if(Input.GetKeyDown(KeyCode.Space))
        {


        }

    }

    public void Init()
    {
     //return the player   
    }

    void HealthUI(int HealthPoints)
    {

    }

    void ItemUI()
    {

    }

    void LivesUI()
    {

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
