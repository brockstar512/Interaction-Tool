using System.Collections.Generic;
using UnityEngine;

public class MultipleDependentDoubleDoor : MonoBehaviour
{
    [SerializeField] List<InterfaceReference<IDependencySource<bool>>> sources = new();

}
