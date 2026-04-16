using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform gun;
    [SerializeField] private SpriteRenderer gunSprite;

    [Header("Weapons")]
    public List<BasicGun> guns = new();
    public int weaponSelected;
    public bool hasShot;

    public LayerMask laserMask;

    private float timer;
    private Vector2 mousePos;

    public bool switchUnlocked;
    public float damageMultiplier;

    public float cantShootTimer;
    public bool canShoot = false;
    public bool isInBox = false;

    public static event Action<int> switchedWeapon;

    private List<GameObject> spawnedFire = new();

    [Header("Shockwave")]
    [SerializeField] private float shockwaveCooldown;
    [SerializeField] private GameObject shockwavePrefab;

    public float shockTimer;
    public bool canUseShockwave;

    private void Update()
    {
        if (!canShoot)
            return;

        mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        gun.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x) * Mathf.Rad2Deg);
        float z = gun.eulerAngles.z;
        gunSprite.flipY = z > 90f && z < 270f;

        if (Mouse.current.scroll.y.ReadValue() != 0f) {
            SwitchGun(weaponSelected + (int)Mouse.current.scroll.y.ReadValue());
        }

        if (timer > 0) {
            timer -= Time.deltaTime;
        }
        if (cantShootTimer > 0) {
            cantShootTimer -= Time.deltaTime;
        }
        if (shockTimer > 0)
            shockTimer -= Time.deltaTime;
    }

    public void SwitchGun(int i)
    {
        weaponSelected = i;
        weaponSelected = (weaponSelected % 2 + 2) % 2; // Wraparound clamp
        if (guns[weaponSelected] == null) {
            weaponSelected = 0;
        }
        firePoint.localPosition = new Vector2(guns[weaponSelected].firePointPosition, 0);
        gunSprite.sprite = guns[weaponSelected].gunSprite;

        switchedWeapon?.Invoke(weaponSelected);

        if (timer > guns[weaponSelected].fireRate) {
            timer = guns[weaponSelected].fireRate;
        }
    }

    public void Shoot()
    {
        if (!canShoot)
            return;
        if (isInBox)
            return;
        if (hasShot) 
            return;
        if (timer > 0) 
            return;
        if (cantShootTimer > 0)
            return;

        AudioManager.Instance.PlaySFX(0, 0.9f, 1);

        switch (guns[weaponSelected].bulletType)
        {
            case BulletType.Basic:
                BasicShoot();
                break;
            case BulletType.Explosive:
                ExplosiveShoot();
                break;
            case BulletType.Raycast:
                LaserShoot();
                break;
            case BulletType.Returning:
                ReturningShoot();
                break;
        }
    }

    void BasicShoot()
    {
        for (int i = 0; i < guns[weaponSelected].bulletAmount; i++) {

            float zOffset = UnityEngine.Random.Range(-guns[weaponSelected].spread, guns[weaponSelected].spread);
            Quaternion offset = Quaternion.Euler(0, 0, zOffset);
            GameObject newBullet = Instantiate(guns[weaponSelected].bulletPrefab, firePoint.position, firePoint.rotation * offset);
            newBullet.tag = "PlayerBullet";
            BasicBullet bulletScript = newBullet.GetComponent<BasicBullet>();
            bulletScript.damage = Mathf.CeilToInt(guns[weaponSelected].damage * damageMultiplier);
            if (guns[weaponSelected].bulletAmount > 1) {
                bulletScript.speed = guns[weaponSelected].bulletSpeed * UnityEngine.Random.Range(0.9f, 1.1f);
            } else {
                bulletScript.speed = guns[weaponSelected].bulletSpeed;
            }
        }

        timer = guns[weaponSelected].fireRate;
        if (!guns[weaponSelected].automatic && !switchUnlocked)
            hasShot = true;
    }
    void ExplosiveShoot()
    {
        for (int i = 0; i < guns[weaponSelected].bulletAmount; i++) {

            float zOffset = UnityEngine.Random.Range(-guns[weaponSelected].spread, guns[weaponSelected].spread);
            Quaternion offset = Quaternion.Euler(0, 0, zOffset);
            GameObject newBullet = Instantiate(guns[weaponSelected].bulletPrefab, firePoint.position, firePoint.rotation * offset);
            newBullet.tag = "PlayerBullet";
            GrenadeBullet bulletScript = newBullet.GetComponent<GrenadeBullet>();
            bulletScript.damage = Mathf.CeilToInt(guns[weaponSelected].damage * damageMultiplier);
            if (guns[weaponSelected].bulletAmount > 1) {
                bulletScript.speed = guns[weaponSelected].bulletSpeed * UnityEngine.Random.Range(0.9f, 1.1f);
            } else {
                bulletScript.speed = guns[weaponSelected].bulletSpeed;
            }
        }

        timer = guns[weaponSelected].fireRate;
        if (!guns[weaponSelected].automatic && !switchUnlocked)
            hasShot = true;
    }
    void LaserShoot()
    {
        if (spawnedFire.Count > 0) {
            foreach (GameObject fire in spawnedFire) 
                Destroy(fire);
            spawnedFire.Clear();
        }

        Vector2 direction = (mousePos - (Vector2)firePoint.position).normalized; 
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, 50f, laserMask);

        Vector2 a = firePoint.position;
        Vector2 b = hit.point;

        float spacing = guns[weaponSelected].bulletSpeed;
        float distance = Vector2.Distance(a, b);
        int count = guns[weaponSelected].bulletAmount;

        Vector2[] positions = new Vector2[count];

        for (int i = 0; i < count; i++) {
            positions[i] = a + direction * (i * spacing);
        }

        StartCoroutine(SpawnFire(positions, guns[weaponSelected].bulletSpeed, Mathf.CeilToInt(guns[weaponSelected].damage * damageMultiplier), guns[weaponSelected].bulletPrefab, distance, a));

        timer = guns[weaponSelected].fireRate;
        if (!guns[weaponSelected].automatic)
            hasShot = true;
    }

    IEnumerator SpawnFire(Vector2[] positions, float duration, int damage, GameObject prefab, float distance, Vector2 currentFirePoint)
    {
        foreach (Vector2 position in positions) {
            Vector2 offsetPosition = new Vector2(position.x, position.y + 0.25f);
            if (Vector2.Distance(currentFirePoint, offsetPosition) > distance) {
                continue;
            }
            GameObject newFire = Instantiate(prefab, offsetPosition, Quaternion.identity);
            newFire.GetComponent<FireScript>().duration = duration;
            newFire.GetComponent<FireScript>().damage = damage;
            newFire.tag = "PlayerBullet";
            spawnedFire.Add(newFire);
            yield return new WaitForSeconds(0.05f);
        }
    }

    void ReturningShoot()
    {
        GameObject newBullet = Instantiate(guns[weaponSelected].bulletPrefab, firePoint.position, firePoint.rotation);
        BoxCutterProjectile bulletScript = newBullet.GetComponent<BoxCutterProjectile>();
        bulletScript.damage = Mathf.CeilToInt(guns[weaponSelected].damage * damageMultiplier);
        bulletScript.speed = guns[weaponSelected].bulletSpeed;
        bulletScript.owner = transform;

        timer = guns[weaponSelected].fireRate;
        if (!guns[weaponSelected].automatic && !switchUnlocked)
            hasShot = true;
    }

    public void UseShockwave()
    {
        if (!canUseShockwave)
            return;
        if (shockTimer > 0)
            return;

        Instantiate(shockwavePrefab, transform.position, Quaternion.Euler(-90f, 0f, 0f));

        shockTimer = shockwaveCooldown;
    }
}
