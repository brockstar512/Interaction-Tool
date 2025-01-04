using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlapCircleCollider : MonoBehaviour, IBellSound
{

    private SpriteRenderer circleSprite; // Reference to the SpriteRenderer of the circle
    public LayerMask detectionLayer; // Layer mask to filter detected objects
    private void Awake()
    {
        circleSprite = GetComponent<SpriteRenderer>();
    }

    public IBellSound Init()
    {
        return this;
    }

    public void Stop()
    {
        Destroy(this.gameObject);
    }


    void Update()
       {
           if (circleSprite == null)
           {
               Debug.LogError("Circle SpriteRenderer is not assigned!");
               return;
           }

           // Calculate the circle's position and radius
           Vector2 circlePosition = circleSprite.transform.position;
           float circleRadius = circleSprite.bounds.extents.x; // Assuming the sprite is uniformly scaled

           // Detect all colliders overlapping the circle
           Collider2D[] colliders = Physics2D.OverlapCircleAll(circlePosition, circleRadius, detectionLayer);

           // Loop through the detected colliders
           foreach (Collider2D collider in colliders)
           {
               Debug.Log("Detected: " + collider.gameObject.name);
           }

           // Optional: Visualize the detection area in the Scene view
           //DrawDetectionCircle(circlePosition, circleRadius);
       }

       private void DrawDetectionCircle(Vector2 position, float radius)
       {
           Gizmos.color = Color.green;
           Gizmos.DrawWireSphere(position, radius);
       }

       private void OnDrawGizmos()
       {
           if (circleSprite != null)
           {
               Vector2 position = circleSprite.transform.position;
               float radius = circleSprite.bounds.extents.x;
               DrawDetectionCircle(position, radius);
           }
       }
     
}
