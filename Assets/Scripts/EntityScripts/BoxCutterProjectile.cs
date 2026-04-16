using UnityEngine;

public class BoxCutterProjectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage;
    public float duration = 1f;
    public float rotationSpeed;
    public Transform sprite;
    public Transform owner;

    public AnimationCurve curve;

    private Rigidbody2D rb;

    private float timer = 0f;
    private float spinAngle = 0f;
    private bool returning = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / duration);

        float currentSpeed = Mathf.Lerp(-speed, speed, curve.Evaluate(t));

        returning = t > 0.285f;

        if (returning && owner != null) {
            Vector2 dir = (owner.position - transform.position).normalized;
            rb.linearVelocity = dir * Mathf.Abs(currentSpeed);
        } else {
            rb.linearVelocity = transform.right * currentSpeed;
        }

        spinAngle += rotationSpeed * Time.deltaTime;
        sprite.localRotation = Quaternion.Euler(0f, 0f, spinAngle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Box") ||
           (collision.CompareTag("ForceField") && CompareTag("PlayerBullet"))) {
            timer = duration;
            returning = true;
            return;
        }

        if (!collision.TryGetComponent<IDamageable>(out var target))
            return;

        if (CompareTag("PlayerBullet") && collision.CompareTag("Player")) {
            if (returning) {
                Destroy(gameObject);
            }
            return;
        }

        if (CompareTag("EnemyBullet") && collision.CompareTag("Enemy"))
            return;

        var point = (Vector2)transform.position;
        var damageInfo = new DamageInfo(damage, point, Vector2.zero, gameObject, DamageType.Bullet);

        target.TakeDamage(damageInfo);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (CompareTag("PlayerBullet") && collision.CompareTag("Player")) {
            if (returning) {
                Destroy(gameObject);
            }
        }
    }
}