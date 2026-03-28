using System.Collections;
using UnityEngine;

public class DasherEnemyAI : EnemyAI
{

    [Header("References")]
    private Transform player;
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private LayerMask lineOfSightMask;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float chaseRotationSpeed = 5f;
    [SerializeField] private float targetAttackDistance = 3f;
    [SerializeField] private float maxDistanceToChase = 4f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRotationSpeed = 3f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashSpeed = 10f;

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
        else if (currentState == "Dashing")
            Dash();
    }

    void CheckDistanceAndSight()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(player.position.y - transform.position.y, player.position.x - transform.position.x) * Mathf.Rad2Deg;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, float.PositiveInfinity, lineOfSightMask);
        if (hit.collider.transform != player) {
            currentState = "Idle";
            return;
        }

        if (currentState == "Dashing")
            return;

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
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, chaseRotationSpeed * Time.deltaTime);
    }

    void AttackPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, attackRotationSpeed * Time.deltaTime);

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCooldown) {
            attackTimer = 0;
            currentState = "Dashing";
            AudioManager.Instance.PlaySFX(1, 0.6f, 0.6f);
        }
    }

    void Dash()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer <= dashDuration) {
            rb.position += (Vector2)transform.right * dashSpeed * Time.deltaTime;
        } else {
            attackTimer = 0f;
            currentState = "Idle";
        }

    }

}
