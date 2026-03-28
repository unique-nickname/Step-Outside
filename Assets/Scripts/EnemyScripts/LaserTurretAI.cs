using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurretAI : EnemyAI
{
    private Transform player;

    [Header("Settings")]
    [SerializeField] private LayerMask lineOfSightMask;
    [SerializeField] private LayerMask laserMask;

    [Header("Renderers")]
    [SerializeField] private LineRenderer laserRenderer;   // Pink actual laser
    [SerializeField] private LineRenderer guideRenderer;   // Thin yellow guide laser

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float timeAfterLock = 1f;

    [Header("Laser")]
    [SerializeField] private LayerMask damageMask;
    [SerializeField] private int bounces = 2;
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private float thickness = 0.2f;
    [SerializeField] private float maxDistance = 100f;

    [SerializeField] private List<LaserSegment> segments = new();

    private float attackTimer;
    private bool isAttacking;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (laserRenderer != null)
            laserRenderer.positionCount = 0;

        if (guideRenderer != null)
            guideRenderer.positionCount = 0;
    }

    void Update()
    {
        if (player == null || isAttacking)
            return;

        if (!IsPlayerInSight()) {
            attackTimer = 0f;
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown) {
            Vector2 lockDirection = (player.position - transform.position).normalized;
            StartCoroutine(WaitForLock(lockDirection));
            attackTimer = 0f;
        }
    }

    bool IsPlayerInSight()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Mathf.Infinity, lineOfSightMask);

        return hit.collider != null && hit.collider.transform == player;
    }

    IEnumerator WaitForLock(Vector2 dir)
    {
        isAttacking = true;

        // Show yellow warning beam during lock-on time
        DrawLaserPath(guideRenderer, dir, storeSegments: false);

        float timer = 0f;

        AudioManager.Instance.PlaySFX(14, 0.65f, 1f);

        bool firstPlayed = false;
        bool secondPlayed = false;
        bool thirdPlayed = false;

        while (timer < timeAfterLock) {
            // Optional: cancel if player is no longer visible
            if (!IsPlayerInSight()) {
                guideRenderer.positionCount = 0;
                isAttacking = false;
                yield break;
            }

            
            if (timer > 0.25f && !firstPlayed) {
                AudioManager.Instance.PlaySFX(14, 0.65f, 1f);
                firstPlayed = true;
            }
            if (timer > 0.5f && !secondPlayed) {
                AudioManager.Instance.PlaySFX(14, 0.65f, 1f);
                secondPlayed = true;
            }
            if (timer > 0.75f && !thirdPlayed) {
                AudioManager.Instance.PlaySFX(14, 0.65f, 1f);
                thirdPlayed = true;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Remove guide and fire actual laser
        guideRenderer.positionCount = 0;

        FireLaser(dir);
        AudioManager.Instance.PlaySFX(15, 0.65f, 1f);
        yield return StartCoroutine(LaserLifetime());

        isAttacking = false;
    }

    void FireLaser(Vector2 direction)
    {
        DrawLaserPath(laserRenderer, direction, storeSegments: true);
    }

    void DrawLaserPath(LineRenderer targetRenderer, Vector2 direction, bool storeSegments)
    {
        if (targetRenderer == null)
            return;

        if (storeSegments)
            segments.Clear();

        List<Vector3> points = new List<Vector3>();
        Vector2 start = transform.position;
        points.Add(start);

        for (int i = 0; i <= bounces; i++) {
            RaycastHit2D hit = Physics2D.Raycast(start, direction, maxDistance, laserMask);

            Vector2 end;
            if (hit.collider != null) {
                end = hit.point;
            } else {
                end = start + direction * maxDistance;
            }

            points.Add(end);

            if (storeSegments)
                segments.Add(new LaserSegment(start, end));

            Debug.DrawLine(start, end, storeSegments ? Color.magenta : Color.yellow, duration);

            if (hit.collider == null)
                break;

            direction = Vector2.Reflect(direction, hit.normal).normalized;
            start = hit.point + direction * 0.01f; // prevent rehitting same spot
        }

        targetRenderer.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
            targetRenderer.SetPosition(i, points[i]);
    }

    IEnumerator LaserLifetime()
    {
        float timer = 0f;

        while (timer < duration) {
            DealDamageAlongLaser();
            timer += Time.deltaTime;
            yield return null;
        }

        laserRenderer.positionCount = 0;
        segments.Clear();
    }

    void DealDamageAlongLaser()
    {
        foreach (var segment in segments) {
            Vector2 dir = (segment.end - segment.start).normalized;
            float length = Vector2.Distance(segment.start, segment.end);
            Vector2 center = (segment.start + segment.end) * 0.5f;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            Collider2D[] hits = Physics2D.OverlapBoxAll(center, new Vector2(length, thickness), angle, damageMask);

            foreach (Collider2D col in hits) {
                IDamageable damageable = col.GetComponent<IDamageable>();
                if (damageable != null) {
                    var hitPoint = (Vector2)col.transform.position;
                    var info = new DamageInfo(attackDamage, hitPoint, Vector2.zero, gameObject, DamageType.Laser);
                    damageable.TakeDamage(info);
                }
            }
        }
    }
}

public struct LaserSegment
{
    public Vector2 start;
    public Vector2 end;

    public LaserSegment(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
    }
}