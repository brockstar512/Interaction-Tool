using System.Threading.Tasks;
using UnityEngine;

public interface IGetAllOverlap<T> where T : Component
{
    T[] GetAllOverlapObject(Bounds areaChecker);
}
