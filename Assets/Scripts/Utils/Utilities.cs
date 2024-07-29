
using UnityEngine;

public static class Utilities 
{
    public const string InteractableLayer = "Interactable";
    public const string InteractingLayer = "Interacting";
    public const string SlidableObstructionLayer = "Obstruction";
    public const string PlayerLayer = "Player";
    public const string EverythingLayer = "Everything";




//        this.gameObject.layer = LayerMask.NameToLayer(Utilities.InteractableLayer);

//add layer. remove layer
    // public static void AddFocusLayer(string layerToAdd, LayerMask layer)
    // {
    //     layer |= 0x1 << LayerMask.NameToLayer(layerToAdd);
    // }
    public static void RemoveFocusLayer(string layerToRemove)
    {
        
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
