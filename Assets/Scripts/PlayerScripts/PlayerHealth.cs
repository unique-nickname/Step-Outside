using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{

    [Header("Health")]
    [SerializeField] private int maxHealth = 4;
    [SerializeField] private float invulnSeconds = 0.75f;

    public float damageMultiplier = 1f;

    public GameObject forceFieldPrefab;

    private SpriteFlashEffect flasher;
    public GameObject deathEffect;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public void SetMaxHealth(int h) => maxHealth = h;

    public event Action<int, int> OnHealthChanged;
    public event Action<DamageInfo> OnDamaged;
    public static event Action OnDied;

    private float invulnUntil;

    public void Initialize()
    {
        flasher = GetComponent<SpriteFlashEffect>();
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        GetComponent<SpriteRenderer>().enabled = true;
        transform.localScale = new Vector2(0.85f, 0.85f);
        flasher.Clear();
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<SpriteRenderer>() != null) {
                continue;
            }
            transform.GetChild(i).gameObject.SetActive(true);
        }
        GetComponent<Collider2D>().enabled = true;
        GetComponent<PlayerMelee>().enabled = true;
        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<PlayerShooting>().enabled = true;
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
    public void TakeDamage(DamageInfo info)
    {
        if (CurrentHealth <= 0) return;
        if (Time.time < invulnUntil) return;

        AudioManager.Instance.PlaySFX(2, 0.9f, 1);

        CurrentHealth -= Mathf.CeilToInt(info.amount * damageMultiplier);
        invulnUntil = Time.time + invulnSeconds;

        OnDamaged?.Invoke(info);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            StartCoroutine(DeathSequence());
        } else {
            flasher.FlashWhite();
            StartCoroutine(InvulnerabilityFlash());
        }
            
    }

    IEnumerator InvulnerabilityFlash()
    {
        yield return new WaitForSeconds(0.15f);
        var sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
        yield return new WaitForSeconds(invulnSeconds / 3 - 0.05f);
        sr.enabled = true;
        yield return new WaitForSeconds(invulnSeconds / 3 - 0.05f);
        sr.enabled = false;
        yield return new WaitForSeconds(invulnSeconds / 3 - 0.05f);
        sr.enabled = true;
    }

    IEnumerator DeathSequence()
    {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<SpriteRenderer>() != null) {
                continue;
            }
            transform.GetChild(i).gameObject.SetActive(false);
        }
        GetComponent<Collider2D>().enabled = false;
        GetComponent<PlayerMelee>().enabled = false;
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerShooting>().enabled = false;

        AudioManager.Instance.PlaySFX(3, 0.3f, 1);
        StartCoroutine(flasher.PermWhite());
        yield return new WaitForSeconds(flasher.fadePerm);

        AudioManager.Instance.PlaySFX(13, 0.9f, 1);
        Instantiate(deathEffect, transform.position, Quaternion.identity);

        float t = 0;
        Vector3 origScale = transform.localScale;

        while (t < 0.3f) {
            t += Time.deltaTime;
            float amount = Mathf.Lerp(1f, 0.3f, t / 0.3f);
            transform.localScale = origScale * amount;

            yield return null;
        }

        GetComponent<SpriteRenderer>().enabled = false;

        yield return new WaitForSeconds(2.5f);
        OnDied?.Invoke();
        AudioManager.Instance.StopMusic();
        gameObject.SetActive(false);
    }

    public void CreateForcefield()
    {
        var forcefield = Instantiate(forceFieldPrefab, transform.position, Quaternion.identity, transform);
        forcefield.GetComponent<SlimeForcefield>().player = this;
    }

    public void StartInvulnerability()
    {
        invulnUntil = Time.time + invulnSeconds;
        StartCoroutine(InvulnerabilityFlash());
    }
}
