using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

//https://yal.cc/top-down-bouncing-loot-effects/
class BombThrowable : Throwable
{
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //update the layer so we can interact with it
        UpdateLayerName();
    }
    
    protected void FixedUpdate()
    {
        //if it's thrown run the in air logic
        if (_isThrown)
        {
            InAir();
        }
        
        //determine if it needs to go to the next bounce
        if (_isThrown && Vector2.Distance(_startingPoint, transform.position) >= DistanceLimit)
        {
            _isThrown = false;
            //if it does not have more distance to go
            if ( currentBounceIndex < bounceSequence.Count-1)
            {
                //increase tp the next curve
                currentBounceIndex++;
                Speed *= .75f;
                //when we leave the state release will be called and it will be thrown and we immediately go to 
                DistanceLimit = bounceSequence[currentBounceIndex].keys[1].time;
                //throw the item 
                Toss(_throwDirection);            
            }
            
        }
    }
    
    
    
    
}
