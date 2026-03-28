using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    
    public List<BasicItem> allItems = new();
    private List<BasicItem> staticAllItems = new();

    public GameObject itemPrefab;
    public float rotationTime;
    public int startingSlotAmount;

    public float GoldMultiplierMax = 3;
    public float GoldMultiplier { get; private set; }
    
    public TMP_Text goldMultiplier;

    [SerializeField] private List<GameObject> slots = new();
    private List<BasicItem> rolledSlots = new();
    private float timer;
    private EnemySpawner spawner;
    private PlayerShooting playerShooting;

    private float timeMultiplier;

    private float totalWeight = 0f;

    private Vector2[] slotPositions = { new Vector2(-0.75f, 0f), 
                                        new Vector2(-0.75f, 1.5f), 
                                        new Vector2(-0.75f, -1.5f),
                                        new Vector2(0.75f, 0f),
                                        new Vector2(0.75f, 1.5f),
                                        new Vector2(0.75f, -1.5f) };

    private void Awake()
    {
        spawner = GameObject.FindGameObjectWithTag("Box").GetComponent<EnemySpawner>();
        playerShooting = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerShooting>();

        staticAllItems = allItems;
    }

    public void Initialize()
    {
        for (int i = 0; i < slots.Count; i++) {
            if (slots[i] != null) {
                Destroy(slots[i]);
            }
        }

        slots.Clear();
        rolledSlots.Clear();
        for (int i = 0; i < startingSlotAmount; i++) {
            slots.Add(null);
            rolledSlots.Add(null);
        }

        allItems = staticAllItems;
        GoldMultiplier = 1f;
        goldMultiplier.text = "1.0x";

        RotateItems();
    }

    private void Update()
    {
        if (!spawner.gameStarted) return;

        timeMultiplier = (spawner.timeMultiplier == 1) ? 1f : 0.75f;

        timer += Time.deltaTime * timeMultiplier;
        if (timer > rotationTime){
            timer = 0;
            RotateItems();        
        }

        if (spawner.timeMultiplier != 1) {
            GoldMultiplier += 0.2f * Time.deltaTime;
            GoldMultiplier = Mathf.Clamp(GoldMultiplier, 1f, GoldMultiplierMax);
        } else {
            GoldMultiplier -= 0.15f * Time.deltaTime;
            GoldMultiplier = Mathf.Clamp(GoldMultiplier, 1f, GoldMultiplierMax);
        }
        goldMultiplier.text = GoldMultiplier.ToString("F1") + "x";
    }

    void RotateItems()
    {
        Debug.Log("Rotating Items!");
        for (int i = 0; i < slots.Count; i++) {
            BasicItem rolledItem = RollItem();

            while (true) {
                bool shouldReroll = false;

                if (rolledItem.type == ItemType.Heal || rolledItem.type == ItemType.TempBuff) {
                    shouldReroll = false;
                } else if (rolledItem.type == ItemType.Gun) {
                    if (playerShooting.guns[1] != null) {
                        shouldReroll =
                        rolledSlots.Contains(rolledItem) ||
                        playerShooting.guns.Any(x => x.id == rolledItem.id);
                    } else {
                        shouldReroll =
                        rolledSlots.Contains(rolledItem) ||
                        playerShooting.guns[0].id == rolledItem.id;
                    }
                } else {
                    shouldReroll = rolledSlots.Contains(rolledItem);
                }

                if (!shouldReroll)
                    break;

                rolledItem = RollItem();
            }

            ShopItem itemScript = Instantiate(itemPrefab, transform).GetComponent<ShopItem>();

            itemScript.transform.localPosition = slotPositions[i];
            itemScript.itemName = rolledItem.name;
            itemScript.description = rolledItem.description;
            itemScript.price = rolledItem.price;
            itemScript.type = rolledItem.type;
            itemScript.id = rolledItem.id;
            itemScript.SetSprite(rolledItem.sprite);

            if (slots[i] != null) {
                Destroy(slots[i]);
            }
            slots[i] = itemScript.gameObject;
            rolledSlots[i] = rolledItem;
        }
    }

    BasicItem RollItem()
    {
        totalWeight = 0f;
        foreach (var entry in allItems) {
            totalWeight += entry.weight;
        }
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        for (int i = 0; i < allItems.Count; i++) {
            cumulativeWeight += allItems[i].weight;
            if (randomValue <= cumulativeWeight) {
                return allItems[i];
            }
        }
        return allItems[allItems.Count - 1];
    }

}
