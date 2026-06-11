using System.Threading.Tasks;
using UnityEngine;

namespace IT.Overlap
{
    public interface IGetAllOverlap<T> where T : Component
    {
        T[] GetAllOverlapObject(Bounds areaChecker);
    }
}
