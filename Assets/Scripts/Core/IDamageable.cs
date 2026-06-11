using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Core
{
    public interface IDamageable 
    {
        //this applies hurt to self
        public void ApplyDamage(IDamage damagingThing);
        //public void ApplyDamage<T>(T damagingThing) where IDamage;

    }
}
