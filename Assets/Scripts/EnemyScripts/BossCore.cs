using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState
{
    Intro,
    Attack,
    Invincible,
    Death
}

[Serializable]
public class BossWave
{
    public List<GameObject> enemies = new();
    public List<Vector2> spawnPositions = new();
}

public class BossCore : MonoBehaviour, IDamageable
{
    private PlayerHealth playerHealth;

    [Header("Idle")]
    [SerializeField] private float sineRate = 1.0f;
    [SerializeField] private float sineAmp = 1.0f;

    private Vector2 pivotPos;

    [Header("Health")]
    [SerializeField] private int maxHealth = 6;
    public bool IsDamageLocked = false;

    private SpriteFlashEffect flasher;

    public int CurrentHealth;
    public int MaxHealth => maxHealth;

    public event Action<DamageInfo> OnDamaged;
    public static event Action OnBossDied;

    [Header("Movement")]
    [SerializeField] private Vector2[] targetPositions;
    [SerializeField] private Vector2 previousPosition;

    [Header("Attacks")]
    [SerializeField] private GameObject[] allAttacks;
    [SerializeField] private GameObject previousAttack;

    [SerializeField] private GameObject lastStandAttack;

    [Header("Enemies")]
    [SerializeField] private BossWave[] waves;
    [SerializeField] private int nextWave;

    [Header("Phases")]
    [SerializeField] private float phase2Percentage = 0.3f;
    [SerializeField] private int currentPhase = 1;

    [SerializeField] private Sprite crackedSprite;
    [SerializeField] private GameObject bossBreakEffect;

    [SerializeField] private GameObject forceFieldPrefab;

    [Header("State Machine")]
    [SerializeField] private BossState currentState = BossState.Intro;
    [SerializeField] private float timeBetweenAttacks = 10f;
    [SerializeField] private int attacksBeforeMove = 2;

    [SerializeField] private float timeUntilNextAttack;
    [SerializeField] private float attacksUntilNextMove;

    [Header("Effects / Other")]
    [SerializeField] private GameObject gatherEffect;
    [SerializeField] private GameObject shockwaveEffect;
    [SerializeField] private GameObject teleportIndicator;
    [SerializeField] private GameObject directionIndicator;
    [SerializeField] private int indicatorAmount;

    [SerializeField] private GameObject bossDeathEffect;
    [SerializeField] private GameObject BossBoundary;

    private bool stopFloating;

    void Awake()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();

        flasher = GetComponent<SpriteFlashEffect>();
        CurrentHealth = maxHealth;

        pivotPos = Vector3.zero;

        attacksUntilNextMove = attacksBeforeMove;

        StartCoroutine(BossEntrance());
    }

    void Update()
    {
        switch (currentState) {
            case BossState.Attack:
                if (Time.time >= timeUntilNextAttack) {
                    timeUntilNextAttack = Time.time + timeBetweenAttacks;
                    attacksUntilNextMove--;
                    StartCoroutine(StartAttack());
                }
                break;

            case BossState.Invincible:
                GameObject[] aliveEnemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (GameObject enemy in aliveEnemies) {
                    if (enemy == gameObject)
                        continue;
                    return;
                }

                if (nextWave == waves.Length) {
                    currentState = BossState.Attack;
                    Destroy(transform.GetChild(0).gameObject);
                    IsDamageLocked = false;
                    playerHealth.Heal(3);
                } else {
                    for (int i = 0; i < waves[nextWave].enemies.Count; i++) {
                        Instantiate(waves[nextWave].enemies[i], waves[nextWave].spawnPositions[i], Quaternion.identity);
                    }
                    nextWave++;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        if (currentState != BossState.Intro && !stopFloating) {
            float newY = pivotPos.y + Mathf.Sin(Time.time * sineRate) * sineAmp;
            transform.position = new Vector2(pivotPos.x, newY);
        }
    }

    public void TakeDamage(DamageInfo info)
    {
        if (CurrentHealth <= 0) return;
        if (IsDamageLocked) return;

        AudioManager.Instance.PlaySFX(2, 0.75f, 0.8f);

        CurrentHealth -= info.amount;

        OnDamaged?.Invoke(info);

        flasher.FlashWhite();
        PhaseCheck();
    }

    void PhaseCheck()
    {
        if ((float)CurrentHealth / (float)maxHealth <= phase2Percentage && currentPhase == 1) {
            currentPhase = 2;
            Instantiate(bossBreakEffect, pivotPos, Quaternion.Euler(-90f, 0f, 0f));
            GetComponent<SpriteRenderer>().sprite = crackedSprite;
            currentState = BossState.Invincible;
            IsDamageLocked = true;

            GameObject currentAttack = GameObject.FindGameObjectWithTag("BossAttack");
            Destroy(currentAttack);

            Instantiate(forceFieldPrefab, transform.position, Quaternion.identity, transform);
            playerHealth.Heal(3);

            StartCoroutine(Teleport(true));
        }

        if (CurrentHealth <= 0) {
            CurrentHealth = 0;
            currentPhase = 3;
            currentState = BossState.Death;
            IsDamageLocked = true;
            playerHealth.Heal(3);

            GameObject currentAttack = GameObject.FindGameObjectWithTag("BossAttack");
            Destroy(currentAttack);

            StartCoroutine(Teleport(true));
            StartCoroutine(DeathSequence());
        }
    }

    IEnumerator BossEntrance()
    {
        yield return new WaitForEndOfFrame();

        Instantiate(BossBoundary, Vector3.zero, Quaternion.identity);

        playerHealth.Heal(999);

        transform.position = Vector3.zero;

        GetComponent<Collider2D>().enabled = false;

        flasher.PermWhiteNoTime();
        Instantiate(gatherEffect, pivotPos, Quaternion.Euler(-90f, 0f, 0f));
        
        float t = 0;
        Vector3 origScale = transform.localScale;

        while (t < 1.65f) {
            t += Time.deltaTime;
            float amount = Mathf.Lerp(0.15f, 1f, t / 1.65f);
            transform.localScale = origScale * amount;

            yield return null;
        }

        flasher.FlashWhite();

        yield return new WaitForSeconds(0.4f);

        AudioManager.Instance.PlaySFX(4, 0.8f, 1);
        Instantiate(shockwaveEffect, pivotPos, Quaternion.Euler(-90f, 0f, 0f));

        GameObject[] aliveEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in aliveEnemies) {
            if (enemy == gameObject)
                continue;

            enemy.GetComponent<EnemyHealth>().TakeDamage(new DamageInfo(999, Vector2.zero, Vector2.zero, gameObject, DamageType.Contact));
        }

        yield return new WaitForSeconds(0.75f);

        GetComponent<Collider2D>().enabled = true;
        
        currentState = BossState.Attack;
        timeUntilNextAttack = Time.time + 5f;
    }

    IEnumerator Teleport(bool center)
    {
        previousPosition = pivotPos;
        Vector2 chosenPosition;
        if (center)
            chosenPosition = Vector2.zero;
        else
            chosenPosition = targetPositions[UnityEngine.Random.Range(0, targetPositions.Length)];

        Instantiate(teleportIndicator, chosenPosition, Quaternion.Euler(-90f, 0f, 0f));
        yield return new WaitForSeconds(0.85f);

        Vector2 a = previousPosition;
        Vector2 b = chosenPosition;

        float distance = Vector2.Distance(a, b);
        float spacing = distance / indicatorAmount;
        Vector2 direction = (chosenPosition - previousPosition).normalized;

        Vector2[] positions = new Vector2[indicatorAmount];

        for (int i = 0; i < indicatorAmount; i++) {
            positions[i] = a + direction * (i * spacing);
        }

        foreach (Vector2 position in positions) {
            Instantiate(directionIndicator, position, Quaternion.Euler(-90f, 0f, 0f));
        }

        pivotPos = chosenPosition;
    }
    IEnumerator StartAttack()
    {
        GameObject chosenAttack = allAttacks[UnityEngine.Random.Range(0, allAttacks.Length)];
        while (chosenAttack == previousAttack) {
            chosenAttack = allAttacks[UnityEngine.Random.Range(0, allAttacks.Length)];
        }
        previousAttack = chosenAttack;

        if (chosenAttack.name != "SansWalls") {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            for (int i = 0; i < 3; i++) {
                sr.color = new Color(1f, 0.75f, 0.75f);
                yield return new WaitForSeconds(0.05f);
                sr.color = new Color(1f, 0.5f, 0.5f);
                yield return new WaitForSeconds(0.05f);
                sr.color = new Color(1f, 0.75f, 0.75f);
                yield return new WaitForSeconds(0.05f);
                sr.color = Color.white;
                yield return new WaitForSeconds(0.05f);
            }
        }

        GameObject newAttack = Instantiate(chosenAttack, transform.position, Quaternion.identity);
        newAttack.GetComponent<BossAttack>().Level = currentPhase;
        
        yield return new WaitForSeconds(newAttack.GetComponent<BossAttack>().duration);

        if (attacksUntilNextMove == 0) {
            StartCoroutine(Teleport(false));
            attacksUntilNextMove = attacksBeforeMove;
        }
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(1f);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.75f, 0.75f);
        yield return new WaitForSeconds(0.05f);
        sr.color = new Color(1f, 0.5f, 0.5f);
        yield return new WaitForSeconds(0.05f);
        sr.color = new Color(1f, 0.4f, 0.4f);

        Instantiate(lastStandAttack, pivotPos, Quaternion.identity);

        yield return new WaitForSeconds(lastStandAttack.GetComponent<LastStandAttack>().duration);

        stopFloating = true;
        transform.position = Vector3.zero;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerMovement>().canMove = false;
        player.GetComponent<PlayerShooting>().canShoot = false;
        player.GetComponent<PlayerMelee>().canMelee = false;

        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>().target = transform;

        Destroy(GameObject.FindGameObjectWithTag("BossBoundary"));

        AudioManager.Instance.PlaySFX(4, 0.8f, 1);
        Instantiate(shockwaveEffect, pivotPos, Quaternion.Euler(-90f, 0f, 0f));

        yield return new WaitForSeconds(0.75f);

        StartCoroutine(flasher.PermWhite());

        Instantiate(gatherEffect, pivotPos, Quaternion.Euler(-90f, 0f, 0f));

        yield return new WaitForSeconds(2f);

        float t = 0;
        Vector3 origScale = transform.localScale;

        while (t < 0.25f) {
            t += Time.deltaTime;
            float amount = Mathf.Lerp(1f, 0.1f, t / 0.25f);
            transform.localScale = origScale * amount;

            yield return null;
        }

        yield return new WaitForSeconds(0.35f);

        GameObject effect = Instantiate(bossDeathEffect, Vector2.zero, Quaternion.identity);
        yield return new WaitForSeconds(0.75f);
        OnBossDied?.Invoke();
        yield return new WaitForSeconds(2f);
        Destroy(effect);
    }
}
