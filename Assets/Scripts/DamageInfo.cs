using UnityEngine;

public struct DamageInfo
{
    public int amount;
    public Vector2 hitPoint;
    public Vector2 hitNormal;
    public GameObject source;
    public DamageType type;

    public DamageInfo(int amount, Vector2 hitPoint, Vector2 hitNormal, GameObject source, DamageType type)
    {
        this.amount = amount;
        this.hitPoint = hitPoint;
        this.hitNormal = hitNormal;
        this.source = source;
        this.type = type;
    }
}

public enum DamageType
{
    Bullet,
    Contact,
    Explosion,
    Laser,
    Fire
}
