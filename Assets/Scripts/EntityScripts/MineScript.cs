using UnityEngine;

public class MineScript : MonoBehaviour
{

    public int damage;
    public GameObject explosionPrefab;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<IDamageable>(out var target))
            return;

        if (transform.CompareTag("PlayerBullet") && collision.CompareTag("Player"))
            return;
        if (transform.CompareTag("EnemyBullet") && collision.CompareTag("Enemy"))
            return;

        Explode();
    }

    public void Explode()
    {
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosion.GetComponent<ExplosionScript>().damage = damage;
        explosion.tag = gameObject.tag;
        explosion.GetComponent<ExplosionScript>().Initialize();
        Destroy(gameObject);
    }

}
