using System;
using UnityEngine;
using System.Collections;

public class SlimeForcefield : MonoBehaviour, IDamageable
{

    public SpriteRenderer sr;
    public PlayerHealth player;

    public void TakeDamage(DamageInfo info)
    {
        if (info.source.CompareTag("PlayerBullet"))
            return;

        AudioManager.Instance.PlaySFX(9, 0.9f, 1);

        player.StartInvulnerability();

        GetComponent<Collider2D>().enabled = false; // Disable the collider to prevent further damage
        StartCoroutine(DeathEffect());
    }

    IEnumerator DeathEffect()
    {
            float duration = 0.5f;
            float elapsed = 0f;
            Vector3 initialScale = transform.localScale;
            Color initialColor = sr.color;
    
            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
    
                transform.localScale = Vector3.Lerp(initialScale, initialScale * 1.5f, t);
                sr.color = Color.Lerp(initialColor, new Color(initialColor.r, initialColor.g, initialColor.b, 0), t);
    
                yield return null;
            }
    
            Destroy(gameObject);
    }
}
