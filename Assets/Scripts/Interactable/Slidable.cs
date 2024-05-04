using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Slidable : InteractableBase
{
    Vector3 GetWidth { get { return GetComponent<SpriteRenderer>().bounds.size; } }



    public override void Interact(PlayerStateMachineManager player)
    {
        Move(player.LookDirection);
    }

    void Move(Vector2 direction)
    {
        //Vector2 direction = actor
        Physics2D.queriesStartInColliders = false;
        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, direction * 100);
        if (hit.collider != null)
        {
            Debug.Log(Mathf.Abs(transform.position.x - hit.collider.transform.position.x));
            if (Mathf.Abs(transform.position.x - hit.collider.transform.position.x) <= GetWidth.x && Mathf.Abs(direction.x) == 1)
            {
                return;
            }
            if (Mathf.Abs(transform.position.y - hit.collider.transform.position.y) <= GetWidth.y && Mathf.Abs(direction.y) == 1)
            {
                return;
            }
            Vector2 width = new Vector2(hit.collider.GetComponent<SpriteRenderer>().bounds.size.x / 2, hit.collider.GetComponent<SpriteRenderer>().bounds.size.y / 2);
            Vector2 sideOfDestination = direction * -1;
            width *= sideOfDestination;
            Vector3 currentLocation = this.transform.position;
            Vector2 destination = Vector2.zero;
            float distance = 0;

            if (direction == Vector2.down || direction == Vector2.up)
            {
                float distanceMargin = hit.collider.transform.position.y;
                float MyWidthWithSidePos = (GetWidth.y / 2) * (direction.y * -1);
                destination = new Vector2(currentLocation.x, distanceMargin + width.y + MyWidthWithSidePos);
                distance = Mathf.Abs(transform.position.y - hit.collider.transform.position.y);
            }
            if (direction == Vector2.right || direction == Vector2.left)
            {
                float distanceMargin = hit.collider.transform.position.x;
                float MyWidthWithSidePos = (GetWidth.x / 2) * (direction.x * -1);
                destination = new Vector2(distanceMargin + width.x + MyWidthWithSidePos, currentLocation.y);
                distance = Mathf.Abs(transform.position.x - hit.collider.transform.position.x);

            }

            float time = MeasureTime(distance);
            transform.DOMove(destination, time);
        }

    }

    float MeasureTime(float distance)
    {
        const float speed = 15;
        return (distance / speed);
    }

    public override void Release(PlayerStateMachineManager player)
    {
        
    }
}
