using System.Collections;
using UnityEngine;

public class AimedBurst : BossAttack
{
    public GameObject projectilePrefab;
    public GameObject unparryPrefab;

    private Transform player;

    public float timeBetweenBullets;
    public int bulletAmount;
    public float spread;
    public float bulletSpeedRange;
    public float level2AngleOffset;
    public float level2BulletSpeed;

    public bool loop;
    private float timer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(Attack());
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    public IEnumerator Attack()
    {
        while (timer < duration) {
            AudioManager.Instance.PlaySFX(0, 0.65f, 0.8f);

            if (Level == 1) {
                for (int i = 0; i < bulletAmount; i++) {
                    float zOffset = Random.Range(-spread, spread);
                    Quaternion offset = Quaternion.Euler(0, 0, zOffset);
                    GameObject newBullet = Instantiate(projectilePrefab, transform.position, CalculateTargetRotation() * offset);
                    BasicBullet bulletScript = newBullet.GetComponent<BasicBullet>();
                    bulletScript.damage = damage;
                    bulletScript.speed = bulletSpeed * Random.Range(1 - bulletSpeedRange, 1 + bulletSpeedRange);
                }
                yield return new WaitForSeconds(timeBetweenBullets / 3);
                for (int i = 0; i < bulletAmount; i++) {
                    float zOffset = Random.Range(-spread, spread);
                    Quaternion offset = Quaternion.Euler(0, 0, zOffset);
                    GameObject newBullet = Instantiate(unparryPrefab, transform.position, CalculateTargetRotation() * offset);
                    BasicBullet bulletScript = newBullet.GetComponent<BasicBullet>();
                    bulletScript.damage = damage;
                    bulletScript.speed = bulletSpeed * Random.Range(1 - bulletSpeedRange, 1 + bulletSpeedRange) * 2.5f;
                }
            } 
            else if (Level == 2) {
                for (int i = 0; i < bulletAmount; i++) {
                    float zOffset = Random.Range(-spread, spread);
                    Quaternion offset = Quaternion.Euler(0, 0, zOffset + level2AngleOffset);
                    GameObject newBullet = Instantiate(projectilePrefab, transform.position, CalculateTargetRotation() * offset);
                    BasicBullet bulletScript = newBullet.GetComponent<BasicBullet>();
                    bulletScript.damage = damage;
                    bulletScript.speed = level2BulletSpeed * Random.Range(1 - bulletSpeedRange, 1 + bulletSpeedRange);
                }
                for (int i = 0; i < bulletAmount; i++) {
                    float zOffset = Random.Range(-spread, spread);
                    Quaternion offset = Quaternion.Euler(0, 0, zOffset - level2AngleOffset);
                    GameObject newBullet = Instantiate(projectilePrefab, transform.position, CalculateTargetRotation() * offset);
                    BasicBullet bulletScript = newBullet.GetComponent<BasicBullet>();
                    bulletScript.damage = damage;
                    bulletScript.speed = level2BulletSpeed * Random.Range(1 - bulletSpeedRange, 1 + bulletSpeedRange);
                }
                yield return new WaitForSeconds(timeBetweenBullets / 3);
                for (int i = 0; i < bulletAmount; i++) {
                    float zOffset = Random.Range(-spread, spread);
                    Quaternion offset = Quaternion.Euler(0, 0, zOffset);
                    GameObject newBullet = Instantiate(unparryPrefab, transform.position, CalculateTargetRotation() * offset);
                    BasicBullet bulletScript = newBullet.GetComponent<BasicBullet>();
                    bulletScript.damage = damage;
                    bulletScript.speed = bulletSpeed * Random.Range(1 - bulletSpeedRange, 1 + bulletSpeedRange) * 2.5f;
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

    private Quaternion CalculateTargetRotation()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        return targetRotation;
    }

}
