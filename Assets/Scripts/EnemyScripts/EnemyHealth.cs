using System;
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{

    [Header("Health")]
    [SerializeField] private int maxHealth = 6;
    [SerializeField] private int goldOnDeath = 1;
    [SerializeField] private int contactDamage = 1;

    private SpriteFlashEffect flasher;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;

    public event Action<DamageInfo> OnDamaged;
    public static event Action<int, bool> OnDied;

    public GameObject deathEffect;
    public GameObject gatherEffect;

    public bool IsDamageLocked { get; set; } = false;
    public bool isSplitter;
    public bool splitedEnemy;
    public event Action OnSplit;
       
    private void Awake()
    {
        flasher = GetComponent<SpriteFlashEffect>();
        CurrentHealth = maxHealth;

        if (!splitedEnemy)
            StartCoroutine(SpawnEnemy());
    }

    public void TakeDamage(DamageInfo info)
    {
        if (CurrentHealth <= 0) return;
        if (IsDamageLocked) return;

        AudioManager.Instance.PlaySFX(2, 0.75f, 0.8f);

        CurrentHealth -= info.amount;

        OnDamaged?.Invoke(info);

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnDied?.Invoke(goldOnDeath, false);
            Die();
        } else
            flasher.FlashWhite();
    }

    void Die()
    {
        if (isSplitter)
            OnSplit?.Invoke();

        StartCoroutine(DestroyGameobject());
    }

    IEnumerator SpawnEnemy()
    {
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<SpriteRenderer>() != null) {
                continue;
            }
            transform.GetChild(i).gameObject.SetActive(false);
        }
        GetComponent<Collider2D>().enabled = false;
        GetComponent<EnemyAI>().enabled = false;

        Instantiate(gatherEffect, transform.position, Quaternion.Euler(-90f, 0f, 0f));
        flasher.FlashWhite();

        float t = 0;
        Vector3 origScale = transform.localScale;

        while (t < 0.25f) {
            t += Time.deltaTime;
            float amount = Mathf.Lerp(0.3f, 1f, t / 0.25f);
            transform.localScale = origScale * amount;

            yield return null;
        }

        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(true);
        }

        GetComponent<Collider2D>().enabled = true;
        GetComponent<EnemyAI>().enabled = true;
    }

    IEnumerator DestroyGameobject()
    {

        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<SpriteRenderer>() != null) {
                continue;
            }
            transform.GetChild(i).gameObject.SetActive(false);
        }
        GetComponent<Collider2D>().enabled = false;
        GetComponent<EnemyAI>().enabled = false;

        AudioManager.Instance.PlaySFX(3, 0.3f, 1);
        StartCoroutine(flasher.PermWhite());

        float t = 0;
        Vector3 origScale = transform.localScale;

        while (t < 0.15f) {
            t += Time.deltaTime;
            float amount = Mathf.Lerp(1f, 0.3f, t / 0.15f);
            transform.localScale = origScale * amount;

            yield return null;
        }

        yield return new WaitForSeconds(0.15f);
        AudioManager.Instance.PlaySFX(4, 0.5f, 1);
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.TryGetComponent<IDamageable>(out var target))
            return;
        if (!collision.collider.CompareTag("Player"))
            return;

        var hitPoint = (Vector2)transform.position;
        var info = new DamageInfo(contactDamage, hitPoint, Vector2.zero, gameObject, DamageType.Contact);

        target.TakeDamage(info);
    }

}
