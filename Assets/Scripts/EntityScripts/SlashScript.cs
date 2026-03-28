using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashScript : MonoBehaviour
{
    public int damage;
    public LayerMask damageMask;
    public float duration;
    public Vector2 size;
    public float angle;
    public bool isParry;

    private List<Collider2D> alreadyHit = new();
    private float timer;

    private void Update()
    {
        Collider2D[] affectedEntities = Physics2D.OverlapBoxAll(transform.position, size, angle);
        foreach (Collider2D c in affectedEntities) {
            if (isParry && c.TryGetComponent<BasicBullet>(out var bullet) && !alreadyHit.Contains(c)) {
                if (!bullet.isParryable)
                    continue;
                
                c.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                bullet.damage = Mathf.RoundToInt(damage * 1.5f);
                c.gameObject.tag = "PlayerBullet";
                if (c.transform.parent != null) {
                    c.transform.parent = null;
                }
                AudioManager.Instance.PlaySFX(12, 0.85f, 1);
                alreadyHit.Add(c);
                continue;
            }

            if (c.TryGetComponent<MineScript>(out var mine)) {
                Destroy(c.gameObject);
                AudioManager.Instance.PlaySFX(12, 0.85f, 1);
                continue;
            }

            if (!c.TryGetComponent<IDamageable>(out var target))
                continue;
            if (alreadyHit.Contains(c)) 
                continue;
            if (transform.CompareTag("EnemyBullet") && c.CompareTag("Enemy"))
                continue;
            if (transform.CompareTag("PlayerBullet") && c.CompareTag("Player"))
                continue;

            var hitPoint = (Vector2)transform.position;
            var info = new DamageInfo(damage, hitPoint, Vector2.zero, gameObject, DamageType.Explosion);

            target.TakeDamage(info);
            alreadyHit.Add(c);
        }

        timer += Time.deltaTime;
        if (timer > duration) {
            Destroy(gameObject);
        }
    }

}
