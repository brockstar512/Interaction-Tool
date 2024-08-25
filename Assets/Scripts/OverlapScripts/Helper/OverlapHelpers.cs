using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverlapHelpers
{


    public class OverlapCheckHelper
    {
        readonly Vector2 verticalScale= new Vector2(.25f, .25f);
        readonly Vector2 horizontalScale = new Vector2(0.25f,0.25f);
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

    public class OverlapMovePullCheckHelper 
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
    
    public class OverlapMovePushCheckHelper 
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
