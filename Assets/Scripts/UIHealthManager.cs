using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthManager : MonoBehaviour
{

    public PlayerHealth playerHealth;
    public GameObject heartPrefab;
    List<HeartScript> hearts = new List<HeartScript>();

    private void OnEnable()
    {
        playerHealth.OnHealthChanged += UpdateHealthUI;
    }

    private void OnDisable()
    {
        playerHealth.OnHealthChanged -= UpdateHealthUI;
    }

    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        ClearHearts();

        GetComponent<RectTransform>().sizeDelta = new Vector2(maxHealth * 45 + 5, 50);
        GetComponent<RectTransform>().anchoredPosition = new Vector2(maxHealth * 22.5f + 7.5f, -30);

        for (int i = 0; i < maxHealth; i++)
        {
            CreateEmptyHeart();
        }

        for (int i = 0; i < currentHealth; i++)
        {
            hearts[i].SetHeartImage(HeartState.Full);
        }
    }

    public void CreateEmptyHeart()
    {
        GameObject newHeart = Instantiate(heartPrefab, transform);
        newHeart.transform.SetParent(transform);

        HeartScript heartScript = newHeart.GetComponent<HeartScript>();
        heartScript.SetHeartImage(HeartState.Empty);
        hearts.Add(heartScript);
    }

    public void ClearHearts()
    {
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        hearts = new List<HeartScript>();
    }

}
