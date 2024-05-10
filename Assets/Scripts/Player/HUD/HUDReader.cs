using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class HUDReader : MonoBehaviour
{
    
    public static HUDReader Instance { get; private set; }
    private List<PlayerStatusHUD> currentPlayers;
    [SerializeField] PlayerStatusHUD PlayerHUDPrefab;
    const int MAX_PLAYERS = 2;

    //this will subscribe to all events
    //damage/ health

    //[SerializeField] Timer timer;


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
        currentPlayers = new List<PlayerStatusHUD>();
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


    public PlayerStatusHUD InitializePlayerHUD(PlayerStateMachineManager player)
    {
        if (currentPlayers.Count > MAX_PLAYERS)
            return null;

        PlayerStatusHUD result = Instantiate(PlayerHUDPrefab, this.transform);
        result.BuildHUD(player);
        currentPlayers.Add(result);
        return result;
    }

    public void DestoryPlayerHUD(PlayerStatusHUD playersHUD)
    {
        PlayerStatusHUD leaving = playersHUD;
        currentPlayers.Remove(leaving);
        Destroy(leaving.gameObject);
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
