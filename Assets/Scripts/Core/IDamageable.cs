using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Core
{
    public interface IDamageable
    {
        void ApplyDamage(int amount, Vector2 sourcePosition);
    }
}
