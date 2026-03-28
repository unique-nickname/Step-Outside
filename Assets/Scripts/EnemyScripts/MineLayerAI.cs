using UnityEngine;
using UnityEngine.U2D;

public class MineLayerAI : EnemyAI
{

    [Header("References")]
    private Transform player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject minePrefab;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float mineCooldown = 1f;

    private Vector3 targetPosition;
    private float attackTimer;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        targetPosition = new Vector3(Random.Range(-8.8f, 8.8f), Random.Range(-6.6f, 6.6f), 0f);
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f) {
            rb.linearVelocity = Vector3.zero;

            Vector2 direction = (targetPosition - transform.position).normalized;
            rb.position += direction * moveSpeed * Time.deltaTime;
        } else {
            targetPosition = new Vector3(Random.Range(-8.8f, 8.8f), Random.Range(-6.6f, 6.6f), 0f);
        }

        if (attackTimer < mineCooldown)
            attackTimer += Time.deltaTime;
        else {
            GameObject newMine = Instantiate(minePrefab, transform.position, Quaternion.identity);
            newMine.tag = "EnemyBullet";
            newMine.GetComponent<MineScript>().damage = damage;

            AudioManager.Instance.PlaySFX(0, 0.65f, 0.8f);

            attackTimer = 0f;
        }
            
    }

}
