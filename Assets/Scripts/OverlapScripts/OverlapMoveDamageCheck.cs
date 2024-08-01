using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.ItemOverlap
{
    public class OverlapMoveDamageCheck : OverlapMoveCheck
    {

        // Update is called once per frame
        void FixedUpdate()
        {

        }

        public void CleanUp()
        {
            Destroy(this.gameObject);
        }

    }
}
