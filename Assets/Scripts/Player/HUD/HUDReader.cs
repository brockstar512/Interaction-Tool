using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HUDReader : MonoBehaviour
{
    
    public static HUDReader Instance { get; private set; }

    //public PlayerStatus player;
    private List<PlayerStatus> currentPlayers;
    //this will subscribe to all events
    //damage/ health
    [SerializeField] Transform PlayerHolder;
    [SerializeField] Transform PlayerUIPrefab;

    //[SerializeField] Timer timer;

    //seperate player UI logic for mulitple players?-> player status

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        
    }

    void Start()
    {
        
    }

    private void Update()
    {
        ////on space create new player to get UI better
        //if(Input.GetKeyDown(KeyCode.Space))
        //{


        //}

    }


    public PlayerStatusHUD InitializePlayerHUD(PlayerStatus player)
    {
        //return the player
        PlayerStatusHUD result = null;
        for (int i = 0; i < this.transform.childCount; i ++)
        {
            if(!this.transform.GetChild(i).GetComponent<PlayerStatusHUD>().HasPlayer())
            {
                result = this.transform.GetChild(i).GetComponent<PlayerStatusHUD>();
                //player.AssignHealthHUD(result);
            }

        }

        return result;
    }
    public PlayerStatusHUD DeitializePlayerHUD(PlayerStatus player)
    {
        //return the player
        PlayerStatusHUD result = null;
        

        return result;
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
