using UnityEngine;

namespace IT.Player.Movement
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ItemAnchorPoint : MonoBehaviour
    {
       public SpriteRenderer getSpriteRenderer => _sr;
       public Sprite getSprite => _sr.sprite;
       private SpriteRenderer _sr;

       private void Awake()
       {
          _sr = GetComponent<SpriteRenderer>();
          _sr.sprite = null;
       }
    }
}
