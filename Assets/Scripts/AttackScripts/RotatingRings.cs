using System.Collections;
using UnityEngine;

public class RotatingRings : BossAttack
{

    public GameObject projectilePrefab;
    public GameObject unparryablePrefab;
    public GameObject ringPrefab;
    
    public float timeBetweenRings;
    public int bulletsPerRing;
    public float ringOffsetRange;

    public bool loop;

    private float currentOffset;
    private float timer;
    private bool isOtherRing;

    void Start()
    {
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

            GameObject newRing = Instantiate(ringPrefab, transform.position, Quaternion.identity);
            if (isOtherRing) {
                newRing.GetComponent<RingScript>().rotationSpeed = -newRing.GetComponent<RingScript>().rotationSpeed;
            }

            float step = 360f / bulletsPerRing;

            for (int i = 0; i < bulletsPerRing; i++) {
                float angle = currentOffset + i * step;
                Quaternion rot = Quaternion.Euler(0f, 0f, angle);
                BasicBullet bulletScript;
                if (Level == 2)
                    bulletScript = Instantiate(unparryablePrefab, transform.position, rot, newRing.transform).GetComponent<BasicBullet>();
                else
                    bulletScript = Instantiate(projectilePrefab, transform.position, rot, newRing.transform).GetComponent<BasicBullet>();
                bulletScript.damage = damage;
                bulletScript.speed = bulletSpeed;
            }
            currentOffset += Random.Range(-ringOffsetRange, ringOffsetRange);
            isOtherRing = !isOtherRing;

            yield return new WaitForSeconds(timeBetweenRings);
        }
            
        if (loop) {
            timer = 0f;
            StartCoroutine(Attack());
        } else
            Destroy(gameObject);
    }

}
