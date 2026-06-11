using UnityEngine;

namespace IT.Core.Utilities
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class YDepthSort : MonoBehaviour
    {
        // Positive offset moves the anchor point up, negative moves it down.
        // Set this to roughly half the sprite's world-height to anchor at the feet.
        [SerializeField] private float yOffset = 0f;

        private SpriteRenderer _renderer;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        void LateUpdate()
        {
            _renderer.sortingOrder = Mathf.RoundToInt(-(transform.position.y + yOffset) * 100);
        }
    }
}
