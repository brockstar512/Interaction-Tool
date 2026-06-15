// Assets/Scripts/Interactables/IDestructible.cs
using System;

namespace IT.Interactables
{
    public interface IDestructible
    {
        event Action Destroyed;
    }
}