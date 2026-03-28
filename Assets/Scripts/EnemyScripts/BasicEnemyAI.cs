using UnityEngine;

public class BasicEnemyAI : EnemyAI
{

    [Header("References")]
    private Transform player;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform gun;
    [SerializeField] private SpriteRenderer gunSprite;

    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private LayerMask lineOfSightMask;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float chaseRotationSpeed = 5f;
    [SerializeField] private float targetAttackDistance = 3f;
    [SerializeField] private float maxDistanceToChase = 4f;
    [SerializeField] private float attackRotationSpeed = 3f;

    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float bulletSpeed = 10f;

    private string currentState = "Chase";
    private float attackTimer;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        rb.linearVelocity = Vector3.zero;

        CheckDistanceAndSight();

        if (currentState == "Chase")
            ChasePlayer();
        else if (currentState == "Attack")
            AttackPlayer();
    }

    void CheckDistanceAndSight()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(player.position.y - transform.position.y, player.position.x - transform.position.x) * Mathf.Rad2Deg;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, float.PositiveInfinity, lineOfSightMask);
        if (hit.collider.transform != player)
        {
            currentState = "Idle";
            return;
        }   

        if (Vector2.Distance(transform.position, player.position) > targetAttackDistance && currentState == "Idle")
            currentState = "Chase";

        if (Vector2.Distance(transform.position, player.position) <= targetAttackDistance)
            currentState = "Attack";
        else if (Vector2.Distance(transform.position, player.position) > maxDistanceToChase)
            currentState = "Chase";
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.position += direction * moveSpeed * Time.deltaTime;
        Quaternion targetRotation = CalculateTargetRotation();
        gun.rotation = Quaternion.Slerp(gun.rotation, targetRotation, chaseRotationSpeed * Time.deltaTime);
        float z = gun.eulerAngles.z;
        gunSprite.flipY = z > 90f && z < 270f;
    }

    void AttackPlayer()
    {
        Quaternion targetRotation = CalculateTargetRotation();
        gun.rotation = Quaternion.Slerp(gun.rotation, targetRotation, attackRotationSpeed * Time.deltaTime);
        float z = gun.eulerAngles.z;
        gunSprite.flipY = z > 90f && z < 270f;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCooldown)
        {
            AudioManager.Instance.PlaySFX(0, 0.65f, 0.8f);
            GameObject newBullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            newBullet.tag = "EnemyBullet";
            BasicBullet bulletScript = newBullet.GetComponent<BasicBullet>();
            bulletScript.damage = attackDamage;
            bulletScript.speed = bulletSpeed;
            attackTimer = 0f;
        }
    }

    private Quaternion CalculateTargetRotation()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        return targetRotation;
    }

}
