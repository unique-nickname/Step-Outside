using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerShockwave : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private CircleCollider2D shockwaveCollider;
    [SerializeField] private int damage;
    [SerializeField] private float sizeIncrease;
    [SerializeField] private float duration;

    void Start()
    {
        StartCoroutine(DestroyAfterTime());
    }

    private void Update()
    {
        shockwaveCollider.radius += sizeIncrease * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Shockwave hit: " + other.name);
        if (!other.TryGetComponent<IDamageable>(out var target))
            return;
        if (other.CompareTag("PlayerBullet") || other.CompareTag("Player"))
            return;

        if (other.CompareTag("EnemyBullet")) {
            if (other.TryGetComponent<BasicBullet>(out var bullet)) {
                bullet.Break();
            }
        }

        if (other.CompareTag("Enemy")) {
            var hitPoint = (Vector2)transform.position;
            var info = new DamageInfo(damage, hitPoint, Vector2.zero, gameObject, DamageType.Explosion);

            target.TakeDamage(info);
        }
    }

    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
