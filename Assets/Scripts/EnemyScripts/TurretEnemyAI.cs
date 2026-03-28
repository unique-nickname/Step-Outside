using UnityEngine;

public class TurretEnemyAI : EnemyAI
{
    [Header("References")]
    private Transform player;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform gun;
    [SerializeField] private SpriteRenderer gunSprite;


    [Header("Settings")]
    [SerializeField] private LayerMask lineOfSightMask;

    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float bulletSpeed = 10f;

    [SerializeField] private float rotationSpeed = 3f;

    private float attackTimer;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (!IsPlayerInSight())
            return;

        Quaternion targetRotation = CalculateTargetRotation();
        gun.rotation = Quaternion.Slerp(gun.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        float z = gun.eulerAngles.z;
        gunSprite.flipY = z > 90f && z < 270f;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCooldown) {
            AudioManager.Instance.PlaySFX(0, 0.65f, 0.8f);

            GameObject newBullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            newBullet.tag = "EnemyBullet";
            BasicBullet bulletScript = newBullet.GetComponent<BasicBullet>();
            bulletScript.damage = attackDamage;
            bulletScript.speed = bulletSpeed;
            attackTimer = 0f;
        }
    }

    bool IsPlayerInSight()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, float.PositiveInfinity, lineOfSightMask);
        if (hit.collider.transform == player)
            return true;
        else 
            return false;
    }

    private Quaternion CalculateTargetRotation()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        return targetRotation;
    }

}
