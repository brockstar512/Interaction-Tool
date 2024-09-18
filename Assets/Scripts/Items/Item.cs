using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item", order = 1)]
    public abstract class Item : ScriptableObject, IItem
    {
        //could use a factory to generate a concret class rather than pass around scriptable objects
        [SerializeField] Sprite sprite;
        public Sprite Sprite => sprite;

        public abstract void Use(Vector3 playerLocation, Vector3 playerDirection);

    }
}

