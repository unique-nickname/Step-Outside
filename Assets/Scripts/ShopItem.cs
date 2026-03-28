using UnityEngine;
using UnityEngine.InputSystem;

public class ShopItem : MonoBehaviour
{

    public string itemName;
    public string description;
    public int id;
    public int price;
    public ItemType type;

    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private GameObject popupPrefab;

    private bool isPlayerInRange;
    private int confirmation;

    private GameObject popup;

    private PlayerController input;
    private InventoryManager inventory;

    private void Awake()
    {
        input = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        inventory = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
    }

    public void SetSprite(Sprite sprite)
    {
        sr.sprite = sprite;
    }

    private void OnEnable()
    {
        input.interacted += Buy;
    }

    private void OnDisable()
    {
        input.interacted -= Buy;
    }

    void Buy()
    {
        if (!isPlayerInRange) return;

        confirmation++;
        if (confirmation == 1) {
            popup.GetComponentInChildren<PopupScript>().Toggle(confirmation);
            AudioManager.Instance.PlaySFX(9, 0.85f, 1);
        } else if (confirmation == 2) {
            if (inventory.Gold >= price) {
                inventory.BuyItem(id, type);
                inventory.ChangeGoldAmount(price, true);
                Destroy(popup);
                Destroy(gameObject);
            } else {
                confirmation = 0;
                popup.GetComponentInChildren<PopupScript>().Toggle(confirmation);
                AudioManager.Instance.PlaySFX(10, 0.85f, 1);
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) {
            SpawnPopupBox();
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) {
            Destroy(popup);
            isPlayerInRange = false;
            confirmation = 0;
        }
    }

    void SpawnPopupBox()
    {
        popup = Instantiate(popupPrefab, transform);
        popup.transform.position = transform.position;
        popup.transform.SetParent(GameObject.FindGameObjectWithTag("WorldCanvas").transform);

        popup.GetComponentInChildren<PopupScript>().SetParameters(itemName, description, price);
    }

}
