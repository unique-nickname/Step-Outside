using System.Collections;
using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
    public float radius;
    public int damage;
    public LayerMask damageMask;
    // Particle references?

    public void Initialize()
    {
        AudioManager.Instance.PlaySFX(11, 0.85f, 1);
        Collider2D[] affectedEntities = Physics2D.OverlapCircleAll(transform.position, radius, damageMask);
        StartCoroutine(Destroy());
        foreach (Collider2D c in affectedEntities) {
            if (!c.TryGetComponent<IDamageable>(out var target))
                continue;

            var hitPoint = Vector2.zero;
            var info = new DamageInfo();

            if (transform.CompareTag("EnemyBullet") && c.CompareTag("Enemy")) {
                hitPoint = (Vector2)transform.position;
                info = new DamageInfo(5, hitPoint, Vector2.zero, gameObject, DamageType.Explosion);

                target.TakeDamage(info);
                continue;
            }

            if (transform.CompareTag("PlayerBullet") && c.CompareTag("Player")) {
                hitPoint = (Vector2)transform.position;
                info = new DamageInfo(1, hitPoint, Vector2.zero, gameObject, DamageType.Explosion);

                target.TakeDamage(info);
                continue;
            }

            hitPoint = (Vector2)transform.position;
            info = new DamageInfo(damage, hitPoint, Vector2.zero, gameObject, DamageType.Explosion);

            target.TakeDamage(info);
        }
    }

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(0.58f);
        Destroy(gameObject);
    }

}
