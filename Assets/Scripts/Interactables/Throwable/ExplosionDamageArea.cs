using UnityEngine;

namespace IT.Interactables.Throwable
{
    using IT.Core;

    public class ExplosionDamageArea : MonoBehaviour
    {
        [SerializeField] float _splashRange = 5f;   // world units; was a hardcoded local
        [SerializeField] float _splashDamage = 5f;  // damage at the blast center (full falloff)

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"Damaging something");
            if (_splashDamage > 0)
            {
                var hitColliders = Physics2D.OverlapCircleAll(transform.position, _splashRange);

                foreach (var hitCollider in hitColliders)
                {
                    // GetComponentInParent, not GetComponent: the player's hurtbox is the
                    // HealthBox child trigger, but Health (IDamageable) is on the root.
                    // Same-frame double-hit (root collider + HealthBox child) is deduped by
                    // Health's i-frames — the second ApplyDamage lands inside the window.
                    var damageable = hitCollider.GetComponentInParent<IDamageable>();

                    if (damageable != null)
                    {
                        var closestPoint = hitCollider.ClosestPoint(transform.position);
                        float distance = Vector3.Distance(closestPoint, transform.position);

                        // Falloff over DISTANCE vs RANGE: 1.0 at the center, 0.0 at the edge.
                        // (Was InverseLerp(SplashDamage, …) — the damage value was wrongly used
                        //  as the range bound; it only worked because both happened to be 5.)
                        var damagePercent = Mathf.InverseLerp(_splashRange, 0, distance);
                        damageable.ApplyDamage(
                            Mathf.Max(1, Mathf.RoundToInt(_splashDamage * damagePercent)),
                            transform.position);
                    }
                }
            }
        }
    }
}
