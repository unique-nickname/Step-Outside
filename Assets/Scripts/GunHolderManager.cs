using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UIGun
{
    public Sprite gunSprite;
    public Vector2 offset;
    public Vector2 size;
}

public class GunHolderManager : MonoBehaviour
{
    public Sprite selectedGunHolder, unselectedGunHolder;
    public Image[] gunHolders;
    public Image[] gunSprites;
    public List<UIGun> guns = new();

    private void OnEnable()
    {
        MenuManager.GameStarted += Initialize;
        PlayerShooting.switchedWeapon += SwitchWeapons;
        InventoryManager.GunsUpdated += UpdateGunHolder;
    }

    private void OnDisable()
    {
        MenuManager.GameStarted -= Initialize;
        PlayerShooting.switchedWeapon -= SwitchWeapons;
        InventoryManager.GunsUpdated -= UpdateGunHolder;
    }

    void Initialize()
    {
        UpdateGunHolder(0, 0);
        gunSprites[1].color = new Color(0f, 0f, 0f, 0f);
    }

    void SwitchWeapons(int weaponSelected)
    {
        gunHolders[0].sprite = unselectedGunHolder;
        gunHolders[1].sprite = unselectedGunHolder;
        gunHolders[weaponSelected].sprite = selectedGunHolder;
    }

    void UpdateGunHolder(int gunHolder, int gunId)
    {
        if (gunSprites[1].color == new Color(0f, 0f, 0f, 0f)) {
            gunSprites[1].color = new Color(1f, 1f, 1f, 1f);
        }

        gunHolders[0].sprite = unselectedGunHolder;
        gunHolders[1].sprite = unselectedGunHolder;
        gunHolders[gunHolder].sprite = selectedGunHolder;

        gunSprites[gunHolder].sprite = guns[gunId].gunSprite;
        gunSprites[gunHolder].rectTransform.anchoredPosition = guns[gunId].offset;
        gunSprites[gunHolder].rectTransform.localScale = guns[gunId].size;
    }
}
