using UnityEngine;

public class GrenadeBullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage;
    public GameObject explosionPrefab;
    private float speedScaling = 1f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        speedScaling -= Time.deltaTime;
        if (speedScaling >= 0f) {
            rb.linearVelocity = transform.right * speed * speedScaling;
        } else {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Box")) {
            Explode();
        }

        if (!collision.TryGetComponent<IDamageable>(out var target))
            return;

        if (transform.CompareTag("PlayerBullet") && collision.CompareTag("Player"))
            return;
        if (transform.CompareTag("EnemyBullet") && collision.CompareTag("Enemy"))
            return;

        Explode();
    }

    void Explode()
    {
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosion.GetComponent<ExplosionScript>().damage = damage;
        explosion.tag = gameObject.tag;
        explosion.GetComponent<ExplosionScript>().Initialize();
        Destroy(gameObject);
    }

}
