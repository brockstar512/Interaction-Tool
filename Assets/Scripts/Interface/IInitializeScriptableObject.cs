using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public interface IInitializeScriptableObject<T> where T : ScriptableObject
{
   public void Init();
}
