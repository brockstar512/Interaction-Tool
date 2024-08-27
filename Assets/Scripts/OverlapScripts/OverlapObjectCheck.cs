using System;
using UnityEngine;
using OverlapHelpers;
namespace Player.ItemOverlap
{
    

public class OverlapObjectCheck : MonoBehaviour, IGetMostOverlap
{ 
    public Vector2 _areaTopRightCornerAABB,_areaBottomLeftCornerAABB = Vector2.zero;
    OverlapCheckHelper _helper;
    [SerializeField] protected LayerMask detectionLayer;
    private SpriteRenderer _sr;


    // Start is called before the first frame update
    void Start()
    {
        AddDetectionLayers();
        _sr = GetComponent<SpriteRenderer>();
        _helper = new OverlapCheckHelper();
    }
    
    private void AddDetectionLayers()
    {
        detectionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.InteractableLayer);

    }

    
    private void SetMovingOverlappingArea(Vector2 characterPos)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float centerX = sr.bounds.center.x; 
        float centerY = sr.bounds.center.y;
        float extendsX = sr.bounds.extents.x; 
        float extendsY = sr.bounds.extents.y;
        
        _areaTopRightCornerAABB = new Vector2(centerX +extendsX ,centerY +extendsY);
        _areaBottomLeftCornerAABB = new Vector2(centerX -extendsX,centerY -extendsY);
    }
    
    private Collider2D GetMostOverlappedCol()
    {
        // Physics2D.queriesStartInColliders = false;
        Collider2D[] overlappingCols = Physics2D.OverlapAreaAll(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB,detectionLayer);
        if (overlappingCols.Length == 0)
            return null;

        Collider2D col = DetermineMostOverlap(overlappingCols);
        //Debug.Log(col.gameObject.name);
        return col;
    }
    
    private Collider2D DetermineMostOverlap(Collider2D[] lib)
    {
        Collider2D result = lib[0];
        float currentResult = 0;
        foreach(var col in lib)
        {
            float currentArea = GetOverlappingArea(col);

            if ( currentArea > currentResult)
            {
                currentResult = currentArea;
                result = col;

            }
        }

        return result;
    }
    
    private float GetOverlappingArea(Collider2D overlappingObject)
    { 
        
        (Vector2 overlappingTopRightCornerAABB,Vector2 overlappingBottomLeftCornerAABB) = GetAABBCorners(overlappingObject);

        float xLength = Mathf.Min(_areaTopRightCornerAABB.x,overlappingTopRightCornerAABB.x)-Mathf.Max(_areaBottomLeftCornerAABB.x,overlappingTopRightCornerAABB.x);
        float yLength = Mathf.Min(_areaTopRightCornerAABB.y,overlappingBottomLeftCornerAABB.y)-Mathf.Max(_areaBottomLeftCornerAABB.y,overlappingBottomLeftCornerAABB.y);
        return xLength * yLength;
    }
    
    private (Vector2, Vector2) GetAABBCorners(Collider2D overlappingObject)
    {   

        Bounds objectsBound = overlappingObject.bounds;
        
        Vector2 topRightCorner = new Vector2(
            objectsBound.center.x + objectsBound.extents.x,
            objectsBound.center.y + objectsBound.extents.y);
        
        Vector2 bottomLeftCorner = new Vector2(
            objectsBound.center.x - objectsBound.extents.x,
            objectsBound.center.y - objectsBound.extents.y);
        
        return (topRightCorner, bottomLeftCorner);
    }
    
    private void OnDrawGizmos()
    {

        CustomDebug.DrawRectange(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB); 

    }

    public InteractableBase GetOverlapObject(Vector2 characterPos, Vector2 lookDirection)
    {
        this.transform.localScale = _helper.UpdateScale(lookDirection);
        this.transform.localPosition = _helper.UpdatePosition(lookDirection);
        SetMovingOverlappingArea(characterPos);
        Collider2D overlappingObject = GetMostOverlappedCol();
        return overlappingObject?.GetComponent<InteractableBase>();
    }
    
    class OverlapCheckHelper
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
    
}

}

