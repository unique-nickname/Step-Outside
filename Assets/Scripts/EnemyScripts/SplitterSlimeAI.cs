using UnityEngine;

public class SplitterSlimeAI : EnemyAI
{

    [Header("References")]
    private Transform player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private EnemyHealth eH;
    [SerializeField] private GameObject babyPrefab;

    [Header("Settings")]
    [SerializeField] private LayerMask lineOfSightMask;
    [SerializeField] private float moveSpeed = 3f;

    private void OnEnable()
    {
        eH.OnSplit += Split;
    }

    private void OnDisable()
    {
        eH.OnSplit -= Split;
    }

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (!IsPlayerInSight()) {
            return;
        }

        rb.linearVelocity = Vector3.zero;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.position += direction * moveSpeed * Time.deltaTime;
    }

    void Split()
    {
        Instantiate(babyPrefab, transform.position, Quaternion.identity);
        Instantiate(babyPrefab, transform.position, Quaternion.identity);
    }

    bool IsPlayerInSight()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Mathf.Infinity, lineOfSightMask);

        return hit.collider != null && hit.collider.transform == player;
    }

}
