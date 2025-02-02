using System.Threading.Tasks;
using UnityEngine;

public interface IGetAllOverlap<T> where T : Component
{
    Task<T[]> GetAllOverlapObject(Bounds areaChecker);
}
