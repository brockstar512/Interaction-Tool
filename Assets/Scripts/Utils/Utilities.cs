
using UnityEngine;

public static class Utilities 
{
    //originalLayerMask &= ~(1 << layerToRemove);
    public const string NoneLayer = "None";
    public const string InteractableLayer = "Interactable";
    public const string InteractingLayer = "Interacting";
    public const string SlidableObstructionLayer = "Obstruction";
    public const string PlayerLayer = "Player";
    public const string KeyPortLayer = "KeyPort";
    public const string TargetOverlapLayer = "TargetOverlapLayer";
    public const string SocketUsedLayer = "HookSocketUsed";
    public const string SocketUnusedLayer = "HookSocketUnused";
    public const string DoorLayer = "Door";


    
    
    public enum KeyTypes
    {
        None,
        Key,
        MasterKey,
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
   
    public static Vector2 GridDirection(this Vector3 vector)
    {
        vector = vector.normalized;
        return new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
    }

    // //not tested... didnt work
    // public static T GetClassComponent<T>(this Collider2D collider2D, T component)
    // {
    //     T componentType = collider2D.GetComponent<T>();
    //     return componentType;
    // }
    

}
