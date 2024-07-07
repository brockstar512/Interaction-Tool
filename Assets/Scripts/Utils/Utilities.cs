
using UnityEngine;

public static class Utilities 
{
    public const string InteractableLayer = "Interactable";
    public const string SlidableObstructionLayer = "Obstruction";

//        this.gameObject.layer = LayerMask.NameToLayer(Utilities.InteractableLayer);


   

    public static T CreateObjectFromClass<T>(T classType, string objectName) where T : Component
    {
        return new GameObject(objectName).AddComponent<T>();
    }

    

}
