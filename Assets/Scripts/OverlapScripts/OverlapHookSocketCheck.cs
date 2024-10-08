using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.SubItems;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Player.ItemOverlap
{
    public class OverlapHookSocketCheck : MonoBehaviour
    {
        [SerializeField] Vector2 areaTopRightCornerAABB, areaBottomLeftCornerAABB = Vector2.zero;
        [SerializeField] protected LayerMask detectionLayer;
        private SpriteRenderer _sr;

        void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
        }
        

        private void SetMovingOverlappingArea(Vector2 characterPos)
        {
            float centerX = _sr.bounds.center.x;
            float centerY = _sr.bounds.center.y;
            float extendsX = _sr.bounds.extents.x;
            float extendsY = _sr.bounds.extents.y;

            areaTopRightCornerAABB = new Vector2(centerX + extendsX, centerY + extendsY);
            areaBottomLeftCornerAABB = new Vector2(centerX - extendsX, centerY - extendsY);
        }
        //might need to call this asyncronously
        
        public async Task<HookConnector> GetMostOverlappedHookStartCol(Vector2 characterPos)
        {
            SetMovingOverlappingArea(characterPos);
            // Physics2D.queriesStartInColliders = false;
            Collider2D[] overlappingCols =
                Physics2D.OverlapAreaAll(areaTopRightCornerAABB, areaBottomLeftCornerAABB, detectionLayer);
            if (overlappingCols.Length == 0)
                 return null;

            for (int i = 0; i < overlappingCols.Length; i++)
            {
                Debug.Log("Found start");
                HookConnector connector = overlappingCols[i].GetComponent<HookConnector>();
                if (connector != null)
                {
                    //Task.FromResult
                    return await Task.FromResult(connector);
                }
            }
            
            //Debug.Log(col.gameObject.name);
            return null;
        }


    }
}
