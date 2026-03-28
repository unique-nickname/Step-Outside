using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class PopupScript : MonoBehaviour
{

    public TMP_Text Title, Description, Cost;
    public GameObject Info, Confirm;

    public void SetParameters(string title, string description, int cost)
    {

        Title.text = title;
        Description.text = description;
        Cost.text = cost.ToString();

    }

    public void Toggle(int confirmation)
    {
        if (confirmation == 0) {
            Confirm.SetActive(false);
            Info.SetActive(true);
        } else {
            Confirm.SetActive(true);
            Info.SetActive(false);
        }
    }

}
