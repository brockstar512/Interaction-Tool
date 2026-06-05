using System;

public interface ICondition
{
    bool IsMet { get; }
    event Action OnChanged;   // raised whenever IsMet flips; listeners re-read IsMet
}