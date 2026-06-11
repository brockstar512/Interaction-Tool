using UnityEngine;
using System.Threading.Tasks;

namespace IT.Overlap
{
    using IT.Core.Utilities;
    using IT.Items.GrapplingHook;

    public class GrappleSocketOverlap : MonoBehaviour
    {
        [SerializeField] Vector2 areaTopRightCornerAABB, areaBottomLeftCornerAABB = Vector2.zero;
        [SerializeField] protected LayerMask detectionLayer;
        private SpriteRenderer _sr;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            AddDetectionLayers();
        }

        private void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << LayerIndex.SocketUnused;
        }
        

        private void SetMovingOverlappingArea(Vector2 characterPos)
        {
            if (_sr is null)
                return;
            this.transform.position = characterPos;
            float centerX = _sr.bounds.center.x;
            float centerY = _sr.bounds.center.y;
            float extendsX = _sr.bounds.extents.x;
            float extendsY = _sr.bounds.extents.y;

            areaTopRightCornerAABB = new Vector2(centerX + extendsX, centerY + extendsY);
            areaBottomLeftCornerAABB = new Vector2(centerX - extendsX, centerY - extendsY);
        }
        
        public async Task<GrappleSocket> GetMostOverlappedHookStartCol(Vector2 characterPos)
        {
            SetMovingOverlappingArea(characterPos);
            Collider2D[] overlappingCols =
                Physics2D.OverlapAreaAll(areaTopRightCornerAABB, areaBottomLeftCornerAABB, detectionLayer);
            

            if (overlappingCols.Length == 0)
                 return null;

            for (int i = 0; i < overlappingCols.Length; i++)
            {
                // Debug.Log($"iterating {overlappingCols[i].gameObject.name}");
                GrappleSocket connector = overlappingCols[i].GetComponent<GrappleSocket>();
                if (connector != null)
                {
                    //Task.FromResult
                    return await Task.FromResult(connector);
                }
            }
            
            //Debug.Log(col.gameObject.name);
            return null;
        }
        
        public GrappleSocket GetMostOverlappedHookEndCol(Vector2 characterPos)
        {
            // Debug.Log($"Movement {characterPos}");
            SetMovingOverlappingArea(characterPos);
            // Physics2D.queriesStartInColliders = false;
            Collider2D[] overlappingCols =
                Physics2D.OverlapAreaAll(areaTopRightCornerAABB, areaBottomLeftCornerAABB, detectionLayer);
            if (overlappingCols.Length == 0)
                return null;

            for (int i = 0; i < overlappingCols.Length; i++)
            {
                GrappleSocket connector = overlappingCols[i].GetComponent<GrappleSocket>();
                if (connector != null)
                {
                    return connector;
                }
            }
            
            return null;
        }
    }
}
