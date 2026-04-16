using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System;
using System.Collections;

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

    private List<BasicItem> allHealItems = new();
    private List<BasicItem> allPermBuffs = new();
    private List<BasicItem> allTempBuffs = new();
    private List<BasicItem> allAbilites = new();
    public List<BasicGun> allGuns = new();

    public BasicItem postDashPurchase;

    private int quickPillPurchases;
    private int corePurchases;

    public static event Action<int, int> GunsUpdated;

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
        playerShooting.canUseShockwave = false;

        playerShooting.guns[0] = allGuns[0];
        playerShooting.guns[1] = null;
        playerShooting.SwitchGun(0);

        playerMovement.moveSpeed = 5f;

        playerMelee.damage = 2;
        playerMelee.size = 1f;
        playerMelee.cooldown = 0.5f;

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
                ApplyTempBuff(id);
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
                shopManager.allItems.Add(postDashPurchase);
                break;
            case 1: // Parry
                playerMelee.canParry = true;
                shopManager.allItems.Remove(allAbilites[id]);
                break;
            case 2: // Shockwave
                playerShooting.canUseShockwave = true;
                shopManager.allItems.Remove(allAbilites[id]);
                break;
        }
    }

    void AddGun(int id)
    {
        if (playerShooting.guns[1] == null) {
            playerShooting.guns[1] = allGuns[id];
            playerShooting.SwitchGun(1);
            GunsUpdated?.Invoke(1, id);
        } 
        else {
            playerShooting.guns[playerShooting.weaponSelected] = allGuns[id];
            playerShooting.SwitchGun(playerShooting.weaponSelected);
            GunsUpdated?.Invoke(playerShooting.weaponSelected, id);
        }
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
            case 4:
                playerShooting.switchUnlocked = true;
                shopManager.allItems.Remove(allPermBuffs[id]);
                break;
            case 5:
                playerMovement.moveSpeed += 0.5f;
                break;
            case 6:
                playerMovement.rechargeTime -= 0.1f;
                if (playerMovement.rechargeTime == 0.1f) {
                    shopManager.allItems.Remove(allPermBuffs[id]);
                }
                break;
            case 7:
                corePurchases++;
                if (corePurchases == 3) {
                    playerShooting.damageMultiplier += 0.5f;
                    shopManager.allItems.Remove(allPermBuffs[id]);
                }
                break;
        }
    }

    void ApplyTempBuff(int id) 
    {
        switch (id) {

            case 0:
                StartCoroutine(AleApply());
                break;

            case 1:
                playerHealth.CreateForcefield();
                break;

        }
    }

    IEnumerator AleApply()
    {
        playerHealth.damageMultiplier = 2f;
        playerMelee.damageMultiplier = 2f;
        playerShooting.damageMultiplier += 1f;

        yield return new WaitForSeconds(30f);

        playerHealth.damageMultiplier = 1f;
        playerMelee.damageMultiplier = 1f;
        playerShooting.damageMultiplier -= 1f;
    }
}
