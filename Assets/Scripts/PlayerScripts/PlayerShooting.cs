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

    public bool canShoot = false;

    private List<GameObject> spawnedFire = new();

    private void Update()
    {
        if (!canShoot)
            return;

        mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        gun.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x) * Mathf.Rad2Deg);
        float z = gun.eulerAngles.z;
        gunSprite.flipY = z > 90f && z < 270f;

        if (Mouse.current.scroll.y.ReadValue() != 0f)
        {
            SwitchGun(weaponSelected + (int)Mouse.current.scroll.y.ReadValue());
        }

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
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

        if (timer > guns[weaponSelected].fireRate) {
            timer = guns[weaponSelected].fireRate;
        }
    }

    public void Shoot()
    {
        if (!canShoot)
            return;

        if (hasShot) 
            return;

        if (timer > 0) 
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
        }
    }

    void BasicShoot()
    {
        for (int i = 0; i < guns[weaponSelected].bulletAmount; i++) {

            float zOffset = Random.Range(-guns[weaponSelected].spread, guns[weaponSelected].spread);
            Quaternion offset = Quaternion.Euler(0, 0, zOffset);
            GameObject newBullet = Instantiate(guns[weaponSelected].bulletPrefab, firePoint.position, firePoint.rotation * offset);
            newBullet.tag = "PlayerBullet";
            BasicBullet bulletScript = newBullet.GetComponent<BasicBullet>();
            bulletScript.damage = guns[weaponSelected].damage;
            if (guns[weaponSelected].bulletAmount > 1) {
                bulletScript.speed = guns[weaponSelected].bulletSpeed * Random.Range(0.9f, 1.1f);
            } else {
                bulletScript.speed = guns[weaponSelected].bulletSpeed;
            }
        }

        timer = guns[weaponSelected].fireRate;
        if (!guns[weaponSelected].automatic)
            hasShot = true;
    }
    void ExplosiveShoot()
    {
        for (int i = 0; i < guns[weaponSelected].bulletAmount; i++) {

            float zOffset = Random.Range(-guns[weaponSelected].spread, guns[weaponSelected].spread);
            Quaternion offset = Quaternion.Euler(0, 0, zOffset);
            GameObject newBullet = Instantiate(guns[weaponSelected].bulletPrefab, firePoint.position, firePoint.rotation * offset);
            newBullet.tag = "PlayerBullet";
            GrenadeBullet bulletScript = newBullet.GetComponent<GrenadeBullet>();
            bulletScript.damage = guns[weaponSelected].damage;
            if (guns[weaponSelected].bulletAmount > 1) {
                bulletScript.speed = guns[weaponSelected].bulletSpeed * Random.Range(0.9f, 1.1f);
            } else {
                bulletScript.speed = guns[weaponSelected].bulletSpeed;
            }
        }

        timer = guns[weaponSelected].fireRate;
        if (!guns[weaponSelected].automatic)
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

        StartCoroutine(SpawnFire(positions, guns[weaponSelected].bulletSpeed, guns[weaponSelected].damage, guns[weaponSelected].bulletPrefab));

        timer = guns[weaponSelected].fireRate;
        if (!guns[weaponSelected].automatic)
            hasShot = true;
    }

    IEnumerator SpawnFire(Vector2[] positions, float duration, int damage, GameObject prefab)
    {
        foreach (Vector2 position in positions) {
            Vector2 offsetPosition = new Vector2(position.x, position.y + 0.25f);
            GameObject newFire = Instantiate(prefab, offsetPosition, Quaternion.identity);
            newFire.GetComponent<FireScript>().duration = duration;
            newFire.GetComponent<FireScript>().damage = damage;
            newFire.tag = "PlayerBullet";
            spawnedFire.Add(newFire);
            yield return new WaitForSeconds(0.05f);
        }
    }
}
