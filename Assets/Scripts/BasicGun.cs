using UnityEngine;

public enum BulletType
{
    Basic,
    Explosive,
    Raycast,
    Returning
}

[CreateAssetMenu(fileName = "BasicGun", menuName = "Scriptable Objects/BasicGun")]
public class BasicGun : ScriptableObject
{
    public int damage;
    public float fireRate;
    public float bulletSpeed;
    public bool automatic;

    public int bulletAmount;
    public float spread;

    public int id;

    public GameObject bulletPrefab;
    public BulletType bulletType;
    public Sprite gunSprite;
    public float firePointPosition;
}
