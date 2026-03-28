using UnityEngine;

public class FireScript : MonoBehaviour
{
    public int damage;
    public float duration;
    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > duration)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<IDamageable>(out var target))
            return;

        var hitPoint = Vector2.zero;
        var info = new DamageInfo();

        if (transform.CompareTag("PlayerBullet") && collision.CompareTag("Player")) 
            return;

        if (transform.CompareTag("EnemyBullet") && collision.CompareTag("Enemy"))
            return;

        hitPoint = (Vector2)transform.position;
        info = new DamageInfo(damage, hitPoint, Vector2.zero, gameObject, DamageType.Fire);

        target.TakeDamage(info);
    }
}
