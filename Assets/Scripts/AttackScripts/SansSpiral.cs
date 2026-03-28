using System.Collections;
using UnityEngine;

public class SansSpiral : BossAttack
{

    public GameObject projectilePrefab;

    public float timeBetweenBullets;
    public float anglePerBullet;
    public float offsetAngle;

    public bool loop;

    private float currentAngle;
    private float timer;

    void Start()
    {
        currentAngle = Random.Range(0f, 360f);
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

            Quaternion rot = Quaternion.Euler(0f, 0f, currentAngle);
            Instantiate(projectilePrefab, transform.position, rot);
            rot = Quaternion.Euler(0f, 0f, currentAngle + 180f);
            Instantiate(projectilePrefab, transform.position, rot);
            if (Level == 2) {
                rot = Quaternion.Euler(0f, 0f, currentAngle + 90f);
                Instantiate(projectilePrefab, transform.position, rot);
                rot = Quaternion.Euler(0f, 0f, currentAngle + 270f);
                Instantiate(projectilePrefab, transform.position, rot);
            } 

            currentAngle += anglePerBullet;

            if (currentAngle >= 360f) {
                currentAngle -= 360f;
                transform.rotation = Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z + Random.Range(-offsetAngle, offsetAngle));
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
