using UnityEngine;
using UnityEngine.UI;

public class HeartScript : MonoBehaviour
{
    public Sprite fullHeart, emptyHeart;
    Image heartImage;

    private void Awake()
    {
        heartImage = GetComponent<Image>();
    }

    public void SetHeartImage(HeartState state)
        {
            switch (state)
            {
                case HeartState.Full:
                    heartImage.sprite = fullHeart;
                    break;
                case HeartState.Empty:
                    heartImage.sprite = emptyHeart;
                    break;
            }
    }
}

public enum HeartState
{
    Full = 1,
    Empty = 0
}