using UnityEngine;
using UnityEngine.AdaptivePerformance.Provider;
using UnityEngine.UI;

public class UIFeatherManager : MonoBehaviour
{

    public Sprite fullFeather, emptyFeather;
    public Image[] featherContainers;

    private void OnEnable()
    {
        PlayerMovement.chargesChanged += updateUI;
    }

    private void OnDisable()
    {
        PlayerMovement.chargesChanged -= updateUI;
    }

    void updateUI(int charges)
    {
        foreach (Image feather in featherContainers) {
            feather.sprite = emptyFeather;
        }
        for (int i = 0; i < charges; i++) {
            featherContainers[i].sprite = fullFeather;
        }
    }

}
