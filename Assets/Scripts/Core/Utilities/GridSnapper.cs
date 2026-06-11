using UnityEditor;
using UnityEngine;


namespace IT.Core.Utilities
{
    [ExecuteInEditMode]
    public class GridSnapper : MonoBehaviour
    {
        public int tileSize = 1;
        public Vector3 tileOffset = Vector3.zero;


        void Update()
        {
            if(!EditorApplication.isPlaying)
            {
                Vector3 currentPosition = transform.position;

                float snappedX = Mathf.Round(currentPosition.x / tileSize) * tileSize + tileOffset.x;
                //float snappedZ = Mathf.Round(currentPosition.z / tileSize) * tileSize + tileOffset.z;
                //float snappedY = tileOffset.y; // Preserve the original y-coordinate
                float snappedY = Mathf.Round(currentPosition.y / tileSize) * tileSize + tileOffset.y;
                float snappedZ = tileOffset.z; // Preserve the original y-coordinate

                Vector3 snappedPosition = new Vector3(snappedX, snappedY, snappedZ);
                transform.position = snappedPosition;
            }
        }
    }
}
