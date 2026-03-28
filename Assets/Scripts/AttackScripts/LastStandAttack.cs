using System.Collections;
using UnityEngine;

public class LastStandAttack : BossAttack
{

    [Header("General")]
    public GameObject projectilePrefab;
    public GameObject unparryablePrefab;

    [Header("Walls")]
    public float timeBetweenWalls;
    public int wallBulletAmount;
    public float wallsSpeed;

    public int amountOfWalls;

    [Header("Rotating Rings")]
    public GameObject ringPrefab;

    public float timeBetweenRings;
    public int bulletsPerRing;
    public float ringOffsetRange;

    public int amountOfRings;

    private float step;
    private float currentOffset;
    private bool isOtherRing;

    [Header("Spiral")]
    public float timeBetweenBullets;
    public float anglePerBullet;
    public float offsetAngle;

    public float spiralDuration;

    private float currentAngle;

    [Header("Burst")]
    public int bulletAmount;
    public float spread;
    public float bulletSpeedRange;

    public float timeBetweenBursts;

    private float timer;

    void Start()
    {
        currentAngle = Random.Range(0f, 360f);
        StartCoroutine(Attack());
    }

    private void Update()
    {
        timer += Time.deltaTime;
    }

    IEnumerator Attack()
    {
        // Walls
        for (int i = 0; i < amountOfWalls; i++) {

            AudioManager.Instance.PlaySFX(0, 0.65f, 0.8f);

            Vector2 a = new Vector2(9.4f, 7f);
            Vector2 b = new Vector2(9.4f, 0f);

            float distance = Vector2.Distance(a, b);
            float spacing = distance / wallBulletAmount;

            Vector2[] positions = new Vector2[wallBulletAmount];

            for (int j = 0; j < wallBulletAmount; j++) {
                positions[j] = a + Vector2.down * (j * spacing);
            }

            foreach (Vector2 position in positions) {
                GameObject newBullet = Instantiate(projectilePrefab, position, Quaternion.identity);
                newBullet.GetComponent<BasicBullet>().damage = damage;
                newBullet.GetComponent<BasicBullet>().speed = bulletSpeed;
                newBullet.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            }

            a = new Vector2(-9.4f, -7f);
            b = new Vector2(-9.4f, 0f);

            distance = Vector2.Distance(a, b);
            spacing = distance / wallBulletAmount;

            positions = new Vector2[wallBulletAmount + 1];

            for (int j = 0; j < wallBulletAmount + 1; j++) {
                positions[j] = a + Vector2.up * (j * spacing);
            }

            foreach (Vector2 position in positions) {
                GameObject newBullet = Instantiate(projectilePrefab, position, Quaternion.identity);
                newBullet.GetComponent<BasicBullet>().damage = damage;
                newBullet.GetComponent<BasicBullet>().speed = bulletSpeed;
            }

            yield return new WaitForSeconds(timeBetweenWalls);

        }

        yield return new WaitForSeconds(1.5f);

        // Rotating Rings
        for (int i = 0;i < amountOfRings; i++) {
            AudioManager.Instance.PlaySFX(0, 0.65f, 0.8f);
            GameObject newRing = Instantiate(ringPrefab, transform.position, Quaternion.identity);
            if (isOtherRing) {
                newRing.GetComponent<RingScript>().rotationSpeed = -newRing.GetComponent<RingScript>().rotationSpeed;
            }

            step = 360f / bulletsPerRing;

            for (int j = 0; j < bulletsPerRing; j++) {
                float angle = currentOffset + j * step;
                Quaternion rot = Quaternion.Euler(0f, 0f, angle);
                BasicBullet bulletScript;
                bulletScript = Instantiate(unparryablePrefab, transform.position, rot, newRing.transform).GetComponent<BasicBullet>();
                bulletScript.damage = damage;
                bulletScript.speed = bulletSpeed;
            }
            currentOffset += Random.Range(-ringOffsetRange, ringOffsetRange);
            isOtherRing = !isOtherRing;

            yield return new WaitForSeconds(timeBetweenRings);
        }

        // Spiral
        timer = 0;

        while (timer < spiralDuration) {
            AudioManager.Instance.PlaySFX(0, 0.65f, 0.8f);
            Quaternion rot = Quaternion.Euler(0f, 0f, currentAngle);
            Instantiate(projectilePrefab, transform.position, rot);
            rot = Quaternion.Euler(0f, 0f, currentAngle + 90f);
            Instantiate(projectilePrefab, transform.position, rot);
            rot = Quaternion.Euler(0f, 0f, currentAngle + 180f);
            Instantiate(projectilePrefab, transform.position, rot);
            rot = Quaternion.Euler(0f, 0f, currentAngle + 270f);
            Instantiate(projectilePrefab, transform.position, rot);

            currentAngle += anglePerBullet;

            if (currentAngle >= 360f) {
                currentAngle -= 360f;
                transform.rotation = Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z + Random.Range(-offsetAngle, offsetAngle));
            }

            yield return new WaitForSeconds(timeBetweenBullets);
        }

        yield return new WaitForSeconds(0.5f);

        // Criss Cross Burst
        AudioManager.Instance.PlaySFX(0, 0.65f, 0.8f);
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < bulletAmount; j++) {
                float currentRotation = i * 90f;
                float zOffset = Random.Range(-spread, spread);
                Quaternion rotation = Quaternion.Euler(0, 0, currentRotation + zOffset);
                GameObject newBullet = Instantiate(unparryablePrefab, transform.position, rotation);
                BasicBullet bulletScript = newBullet.GetComponent<BasicBullet>();
                bulletScript.damage = damage;
                bulletScript.speed = bulletSpeed * Random.Range(1 - bulletSpeedRange, 1 + bulletSpeedRange) * 2f;
            }
        }

        yield return new WaitForSeconds(timeBetweenBursts);
        
        AudioManager.Instance.PlaySFX(0, 0.65f, 0.8f);
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < bulletAmount; j++) {
                float currentRotation = i * 90f + 45f;
                float zOffset = Random.Range(-spread, spread);
                Quaternion rotation = Quaternion.Euler(0, 0, currentRotation + zOffset);
                GameObject newBullet = Instantiate(unparryablePrefab, transform.position, rotation);
                BasicBullet bulletScript = newBullet.GetComponent<BasicBullet>();
                bulletScript.damage = damage;
                bulletScript.speed = bulletSpeed * Random.Range(1 - bulletSpeedRange, 1 + bulletSpeedRange) * 2f;
            }
        }

        yield return new WaitForSeconds(timeBetweenBursts);

        AudioManager.Instance.PlaySFX(0, 0.65f, 0.8f);
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < bulletAmount; j++) {
                float currentRotation = i * 90f;
                float zOffset = Random.Range(-spread, spread);
                Quaternion rotation = Quaternion.Euler(0, 0, currentRotation + zOffset);
                GameObject newBullet = Instantiate(unparryablePrefab, transform.position, rotation);
                BasicBullet bulletScript = newBullet.GetComponent<BasicBullet>();
                bulletScript.damage = damage;
                bulletScript.speed = bulletSpeed * Random.Range(1 - bulletSpeedRange, 1 + bulletSpeedRange) * 2f;
            }
        }

        yield return new WaitForSeconds(timeBetweenBursts);

        AudioManager.Instance.PlaySFX(0, 0.65f, 0.8f);
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < bulletAmount; j++) {
                float currentRotation = i * 90f + 45f;
                float zOffset = Random.Range(-spread, spread);
                Quaternion rotation = Quaternion.Euler(0, 0, currentRotation + zOffset);
                GameObject newBullet = Instantiate(unparryablePrefab, transform.position, rotation);
                BasicBullet bulletScript = newBullet.GetComponent<BasicBullet>();
                bulletScript.damage = damage;
                bulletScript.speed = bulletSpeed * Random.Range(1 - bulletSpeedRange, 1 + bulletSpeedRange) * 2f;
            }
        }

        yield return new WaitForSeconds(0.25f);

        // Final Ring
        step = 360f / bulletsPerRing * 2;

        AudioManager.Instance.PlaySFX(0, 0.65f, 0.8f);
        for (int j = 0; j < bulletsPerRing * 2; j++) {
            float angle = currentOffset + j * step;
            Quaternion rot = Quaternion.Euler(0f, 0f, angle);
            BasicBullet bulletScript;
            bulletScript = Instantiate(unparryablePrefab, transform.position, rot).GetComponent<BasicBullet>();
            bulletScript.damage = damage;
            bulletScript.speed = bulletSpeed;
        }
    }

}
