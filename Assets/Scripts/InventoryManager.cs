using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{

    [Header("References")]
    private GameObject player;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerShooting playerShooting;
    private PlayerMelee playerMelee;
    private ShopManager shopManager;

    public GameObject featherManager;

    public TMP_Text goldCounter;

    [Header("Items")]
    public int Gold;

    public List<BasicItem> allHealItems = new();
    public List<BasicItem> allPermBuffs = new();
    public List<BasicItem> allTempBuffs = new();
    public List<BasicItem> allAbilites = new();
    public List<BasicGun> allGuns = new();

    private int quickPillPurchases;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<PlayerHealth>();
        playerMovement = player.GetComponent<PlayerMovement>();
        playerShooting = player.GetComponent<PlayerShooting>();
        playerMelee = player.GetComponent<PlayerMelee>();

        shopManager = GameObject.FindGameObjectWithTag("ShopManager").GetComponent<ShopManager>();

        for (int i = 0; i < shopManager.allItems.Count; i++) {
            switch (shopManager.allItems[i].type) {
                case ItemType.Heal:
                    allHealItems.Add(shopManager.allItems[i]);
                    break;
                case ItemType.PermBuff:
                    allPermBuffs.Add(shopManager.allItems[i]);
                    break;
                case ItemType.TempBuff:
                    allTempBuffs.Add(shopManager.allItems[i]);
                    break;
                case ItemType.Ability:
                    allAbilites.Add(shopManager.allItems[i]);
                    break;
            }
        }
    }

    private void Initialize()
    {
        Gold = 10;
        goldCounter.text = ":10";
        
        quickPillPurchases = 0;

        List<GameObject> aliveBullets = GameObject.FindGameObjectsWithTag("PlayerBullet").ToList();
        aliveBullets.AddRange(GameObject.FindGameObjectsWithTag("EnemyBullet"));
        foreach (GameObject bullet in aliveBullets) {
            Destroy(bullet);
        }

        player.transform.position = Vector3.zero;
        player.SetActive(true);

        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>().target = player.transform;

        // Reset everything
        playerMovement.canMove = true;
        playerShooting.canShoot = true;
        playerMelee.canMelee = true;

        playerMovement.hasDash = false;
        playerMelee.canParry = false;

        playerShooting.guns[0] = allGuns[0];
        playerShooting.guns[1] = null;
        playerShooting.SwitchGun(0);

        playerHealth.SetMaxHealth(4);
        playerHealth.Initialize();

        shopManager.Initialize();
    }

    private void OnEnable()
    {
        EnemyHealth.OnDied += ChangeGoldAmount;
        MenuManager.GameStarted += Initialize;
    }

    private void OnDisable()
    {
        EnemyHealth.OnDied -= ChangeGoldAmount;
        MenuManager.GameStarted -= Initialize;
    }

    public void ChangeGoldAmount(int amount, bool subtract)
    {
        if (subtract) {
            Gold -= amount;
        } else { 
            Gold += Mathf.RoundToInt(amount * shopManager.GoldMultiplier); 
        }
        goldCounter.text = ":" + Gold;
    }

    public void BuyItem(int id, ItemType type)
    {
        AudioManager.Instance.PlaySFX(8, 0.75f, 1);

        switch (type) {

            case ItemType.Heal:
                UseHealItem(id); 
                break;
            case ItemType.PermBuff:
                ApplyPermBuff(id);
                break;
            case ItemType.TempBuff:
                break;
            case ItemType.Ability:
                ActivateAbility(id);
                break;
            case ItemType.Gun:
                AddGun(id);
                break;

        }
    }

    void UseHealItem(int id)
    {
        playerHealth.Heal(id);
        // Do other stuff here
    }

    void ActivateAbility(int id)
    {
        switch (id) {
            case 0: // Dash
                playerMovement.hasDash = true;
                shopManager.allItems.Remove(allAbilites[id]);
                featherManager.SetActive(true);
                break;
            case 1: // Parry
                playerMelee.canParry = true;
                shopManager.allItems.Remove(allAbilites[id]);
                break;
        }
    }

    void AddGun(int id)
    {
        if (playerShooting.guns[1] == null) {
            playerShooting.guns[1] = allGuns[id];
            playerShooting.SwitchGun(1);
        } 
        else {
            playerShooting.guns[playerShooting.weaponSelected] = allGuns[id];
            playerShooting.SwitchGun(playerShooting.weaponSelected);
        }
        // Add other stuff here
    }

    void ApplyPermBuff(int id)
    {
        switch (id) {
            case 0:
                playerHealth.SetMaxHealth(playerHealth.MaxHealth + 1);
                playerHealth.Heal(1);
                break;
            case 1:
                playerMelee.damage++;
                break;
            case 2:
                playerMelee.size += 0.2f;
                break;
            case 3:
                playerMelee.cooldown -= 0.1f;
                quickPillPurchases++;
                if (quickPillPurchases == 4) {
                    shopManager.allItems.Remove(allPermBuffs[id]);
                }
                break;
        }
    }

}
