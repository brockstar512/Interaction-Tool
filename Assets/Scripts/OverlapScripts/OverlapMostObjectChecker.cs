using UnityEngine;

namespace Player.ItemOverlap
{
    // For checkers that return the single most-overlapped component of type T.
    public abstract class OverlapMostObjectChecker<T> : OverlapAreaChecker, IGetMostOverlap<T> where T : class
    {
        public T GetOverlapObject(Vector2 characterPos, Vector2 lookDirection)
        {
            Collider2D overlappingObject = GetMostOverlappedCol(characterPos, lookDirection);
            return overlappingObject == null ? null : overlappingObject.GetComponent<T>();
        }
    }
}