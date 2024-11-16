using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionDamage : MonoBehaviour,IDamage
{
    //instantiate this when explosion happens 
    private void OnCollisionEnter(Collision other)
    {
        float SplashRange = 5;//this sprite bounds
        float SplashDamage = 5f;
        if (SplashDamage > 0)
        {
            var hitColliders = Physics2D.OverlapCircleAll(transform.position, SplashRange);

            foreach (var hitCollider in hitColliders)
            {
                var enemy = hitCollider.GetComponent<IHurt>();

                if (enemy!=null)
                {
                    var closestPoint = hitCollider.ClosestPoint(transform.position);
                    float distance = Vector3.Distance(closestPoint, transform.position);

                    var damagePercent = Mathf.InverseLerp(SplashDamage, 0, distance);
                    enemy.ApplyDamage(this);
                }
            }
                
        }
            
    }
}
