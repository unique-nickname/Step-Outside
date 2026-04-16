using UnityEngine;

public class BasicBullet : MonoBehaviour
{

    public float speed = 10f;
    public int damage;
    public bool isParryable;
    [SerializeField] private DamageType damageType;
    [SerializeField] private GameObject breakParticle;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Box"))
        {
            Instantiate(breakParticle, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        if (collision.CompareTag("ForceField") && transform.CompareTag("PlayerBullet")) {
            Instantiate(breakParticle, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        if (!collision.TryGetComponent<IDamageable>(out var target))
            return;

        if (transform.CompareTag("PlayerBullet") && collision.CompareTag("Player"))
            return;
        if (transform.CompareTag("EnemyBullet") && collision.CompareTag("Enemy"))
            return;

        var hitPoint = (Vector2)transform.position;
        var info = new DamageInfo(damage, hitPoint, Vector2.zero, gameObject, damageType);

        target.TakeDamage(info);

        Break();
    }

    public void Break()
    {
        Instantiate(breakParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

}
