using UnityEngine;

public interface IGetAllOverlap<T>
{
    public T[] GetAllOverlapObject(Vector2 areaCheckerBounds);

    public void Hello();

}
