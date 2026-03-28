using System.Collections;
using UnityEngine;

public class SansWalls : BossAttack
{
    public GameObject projectilePrefab;
    public GameObject unparryPrefab;
    
    public GameObject warningPrefab;
    public float appearSpeed;

    public float timeBetweenBullets;
    public int bulletAmount;
    public float sineRate = 1.0f;
    public float sineAmp = 1.0f;

    public float sineSpeed;

    public bool loop;
    private float timer;

    void Start()
    {
        sineSpeed = bulletSpeed;
        StartCoroutine(Attack());
    }

    void Update()
    {
        timer += Time.deltaTime;

        sineSpeed = bulletSpeed + Mathf.Sin(Time.time * sineRate) * sineAmp;
    }

    public IEnumerator Attack()
    {
        GameObject warning = Instantiate(warningPrefab, Vector2.zero, Quaternion.identity);
        SpriteRenderer[] warningSprites = warning.GetComponentsInChildren<SpriteRenderer>();
        float alpha = 0f;

        while (alpha < 1f) {
            alpha = Mathf.MoveTowards(alpha, 1f, appearSpeed * 0.1f);

            foreach (SpriteRenderer warningSprite in warningSprites) {
                Color c = warningSprite.color;
                warningSprite.color = new Color(c.r, c.g, c.b, alpha);
            }

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);

        Destroy(warning);

        while (timer < duration) {
            AudioManager.Instance.PlaySFX(0, 0.65f, 0.8f);

            if ( Level == 1) {
                // Top Part
                Vector2 a = new Vector2(9.4f, 7f);
                Vector2 b = new Vector2(9.4f, 0f);

                float distance = Vector2.Distance(a, b);
                float spacing = distance / bulletAmount;

                Vector2[] positions = new Vector2[bulletAmount];

                for (int i = 0; i < bulletAmount; i++) {
                    positions[i] = a + Vector2.down * (i * spacing);
                }

                foreach (Vector2 position in positions) {
                    GameObject newBullet = Instantiate(projectilePrefab, position, Quaternion.identity);
                    newBullet.GetComponent<BasicBullet>().damage = damage;
                    newBullet.GetComponent<BasicBullet>().speed = bulletSpeed;
                    newBullet.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                }

                // Botton Part
                a = new Vector2(-9.4f, -7f);
                b = new Vector2(-9.4f, 0f);

                distance = Vector2.Distance(a, b);
                spacing = distance / bulletAmount;

                positions = new Vector2[bulletAmount + 1];

                for (int i = 0; i < bulletAmount + 1; i++) {
                    positions[i] = a + Vector2.up * (i * spacing);
                }

                foreach (Vector2 position in positions) {
                    GameObject newBullet = Instantiate(projectilePrefab, position, Quaternion.identity);
                    newBullet.GetComponent<BasicBullet>().damage = damage;
                    newBullet.GetComponent<BasicBullet>().speed = bulletSpeed;
                }
            } 
            else if (Level == 2) {
                // Top Part
                Vector2 a = new Vector2(9.4f, 7f);
                Vector2 b = new Vector2(9.4f, 0f);

                float distance = Vector2.Distance(a, b);
                float spacing = distance / bulletAmount;

                Vector2[] positions = new Vector2[bulletAmount];

                for (int i = 0; i < bulletAmount; i++) {
                    positions[i] = a + Vector2.down * (i * spacing);
                }

                foreach (Vector2 position in positions) {
                    GameObject newBullet = Instantiate(unparryPrefab, position, Quaternion.identity);
                    newBullet.GetComponent<BasicBullet>().damage = damage;
                    newBullet.GetComponent<BasicBullet>().speed = sineSpeed;
                    newBullet.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                }

                // Botton Part
                a = new Vector2(-9.4f, -7f);
                b = new Vector2(-9.4f, 0f);

                distance = Vector2.Distance(a, b);
                spacing = distance / bulletAmount;

                positions = new Vector2[bulletAmount + 1];

                for (int i = 0; i < bulletAmount + 1; i++) {
                    positions[i] = a + Vector2.up * (i * spacing);
                }

                foreach (Vector2 position in positions) {
                    GameObject newBullet = Instantiate(unparryPrefab, position, Quaternion.identity);
                    newBullet.GetComponent<BasicBullet>().damage = damage;
                    newBullet.GetComponent<BasicBullet>().speed = sineSpeed;
                }
            }

            yield return new WaitForSeconds(timeBetweenBullets);
        }

        if (loop) {
            timer = 0f;
            StartCoroutine(Attack());
        } else
            Destroy(gameObject);
    }
}
