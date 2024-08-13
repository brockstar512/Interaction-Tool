
using UnityEngine;

public static class Utilities 
{


    public const string InteractableLayer = "Interactable";
    public const string InteractingLayer = "Interacting";
    public const string SlidableObstructionLayer = "Obstruction";
    public const string PlayerLayer = "Player";
    public const string KeyPortLayer = "KeyPort";
    public const string TargetOverlapLayer = "TargetOverlapLayer";

    
    public enum KeyTypes
    {
        Door,
        MasterDoor,
        SlidingBlock,
        MovingBlock,
    }
    
    public static void PutObjectOnLayer(string objectsNewLayer,GameObject gameObject)
    {
        gameObject.layer = LayerMask.NameToLayer(objectsNewLayer);
    }
   

    public static T CreateObjectFromClass<T>(T classType, string objectName) where T : Component
    {
        return new GameObject(objectName).AddComponent<T>();
    }

    

}
