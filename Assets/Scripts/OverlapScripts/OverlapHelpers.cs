using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverlapHelpers
{


    public class OverlapCheckHelper
    {
        readonly Vector2 verticalScale= new Vector2(.5f, .25f);
        readonly Vector2 horizontalScale = new Vector2(0.25f,0.5f);
        readonly Vector2 upPos = new Vector2(0,0.5f);
        readonly Vector2 downPos = new Vector2(0,0);
        readonly Vector2 rightPos = new Vector2(.2f,.2f);
        readonly Vector2 leftPos = new Vector2(.2f,.2f);

 

        
        public Vector2 UpdateScale(Vector2 lookDirection)
        {
            Vector2 updateScale = Vector2.zero;
            
            if (lookDirection == Vector2.down || lookDirection == Vector2.up)
            {
                updateScale = verticalScale;
            }
            if (lookDirection == Vector2.right ||lookDirection == Vector2.left)
            {
                updateScale = horizontalScale;
            }
            
            return updateScale;
        }
        public Vector2 UpdatePosition(Vector2 lookDirection)
        {
            //Debug.Log(lookDirection);
            Vector2 updatePosition = Vector2.zero;
            
            if (lookDirection == Vector2.down)
            {
                updatePosition = downPos;
            }
            if (lookDirection == Vector2.right || lookDirection == Vector2.left)
            {
                updatePosition = rightPos;
            }
            if (lookDirection == Vector2.up)
            {
                updatePosition = upPos;
            }

    
            return updatePosition;
        }
    }
    //could probably redo this. only need to move things when i am pulling because that's when the clipping will happen
    //can be handled similarly to slidable except the actual slideable or moveable object determines which direction to place the field on
    public class OverlapMoveCheckHelper 
    {
        readonly Vector2 verticalScale = new Vector2(1.0f, .1f);
        readonly Vector2 horizontalScale = new Vector2(0.1f,0.75f);
        readonly Vector2 upPos = new Vector2(0,-0.7f);
        readonly Vector2 downPos= new Vector2(0,0);
        readonly Vector2  leftPos = new Vector2(.5f,-.34f);
        readonly Vector2 rightPos = new Vector2(-.5f,-.34f);
        
        public Vector2 UpdateScale(Vector2 lookDirection)
        {
            Vector2 updateScale = Vector2.zero;
            
            if (lookDirection == Vector2.down || lookDirection == Vector2.up)
            {
                updateScale = verticalScale;
            }
            if (lookDirection == Vector2.right ||lookDirection == Vector2.left)
            {
                updateScale = horizontalScale;
            }
            
            return updateScale;
        }
        public Vector2 UpdatePosition(Vector2 lookDirection)
        {
            //Debug.Log(lookDirection);
            Vector2 updatePosition = Vector2.zero;
            
            if (lookDirection == Vector2.down)
            {
                updatePosition = downPos;
            }
            if (lookDirection == Vector2.right)
            {
                updatePosition = rightPos;
            }

            if (lookDirection == Vector2.left)
            {
                updatePosition = leftPos;
            }
            if (lookDirection == Vector2.up)
            {
                updatePosition = upPos;
            }

    
            return updatePosition;
        }
    }
}
