using UnityEngine;

public class PlayerMelee : MonoBehaviour
{

    [Header("Settings")]
    public float size;
    [SerializeField] private float hitboxDuration;
    public float cooldown;
    public int damage;

    [SerializeField] private float slashOffset;

    [Header("References")]
    [SerializeField] private GameObject slashPrefab;
    [SerializeField] private Transform gun;

    public bool canParry;
    public bool canMelee;
    private float timer;
    private bool flipY;

    private void Update()
    {
        if (timer > 0)
            timer -= Time.deltaTime;
    }

    public void UseMelee()
    {
        if (timer > 0)
            return;
        if (!canMelee)
            return;

        AudioManager.Instance.PlaySFX(Random.Range(5, 7), 0.8f, 1);

        GameObject newSlash = Instantiate(slashPrefab, gun);
        newSlash.transform.localPosition = new Vector2(slashOffset, 0);
        newSlash.transform.localScale = Vector3.one * size;
        newSlash.tag = "PlayerBullet";
        SlashScript script = newSlash.GetComponent<SlashScript>();
        script.damage = damage;
        script.size = new Vector2(0.75f * size, 1.8f * size); // Hardcoded values for now
        script.duration = hitboxDuration;
        script.angle = gun.eulerAngles.z;

        flipY = !flipY;
        newSlash.GetComponent<SpriteRenderer>().flipY = flipY;

        if (canParry)
            script.isParry = true;

        timer = cooldown;
    }

}
