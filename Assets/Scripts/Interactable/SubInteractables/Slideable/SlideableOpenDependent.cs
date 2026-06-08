using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideableOpenDependent : MonoBehaviour, IPullDependent
{
    
    public void OnPullChanged(float amount)
    {
        throw new System.NotImplementedException();
    }
}
