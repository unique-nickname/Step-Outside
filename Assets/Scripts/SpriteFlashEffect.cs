using UnityEngine;
using System.Collections;

public class SpriteFlashEffect : MonoBehaviour
{
    private SpriteRenderer sr;
    private Material materialInstance;
    private Coroutine flashCoroutine;

    public float holdWhiteTime = 0.05f;
    public float fadeBackTime = 0.25f;
    public float fadePerm = 0.05f;

    private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) {
            sr = gameObject.GetComponentInChildren<SpriteRenderer>();
        }

        materialInstance = sr.material;
        materialInstance.SetFloat(FlashAmount, 0f);
    }

    public void FlashWhite()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    public void Clear()
    {
        materialInstance.SetFloat(FlashAmount, 0f);
        flashCoroutine = null;
    }

    public IEnumerator PermWhite()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        materialInstance.SetFloat(FlashAmount, 0f);

        float t = 0f;

        while (t < fadePerm) {
            t += Time.deltaTime;
            float amount = Mathf.Lerp(0f, 1f, t / fadePerm);
            materialInstance.SetFloat(FlashAmount, amount);
            yield return null;
        }

        materialInstance.SetFloat(FlashAmount, 1f);
        flashCoroutine = null;
    }

    public void PermWhiteNoTime()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        materialInstance.SetFloat(FlashAmount, 1f);
        flashCoroutine = null;
    }

    private IEnumerator FlashRoutine()
    {
        materialInstance.SetFloat(FlashAmount, 1f);

        if (holdWhiteTime > 0f)
            yield return new WaitForSeconds(holdWhiteTime);

        float t = 0f;

        while (t < fadeBackTime) {
            t += Time.deltaTime;
            float amount = Mathf.Lerp(1f, 0f, t / fadeBackTime);
            materialInstance.SetFloat(FlashAmount, amount);
            yield return null;
        }

        materialInstance.SetFloat(FlashAmount, 0f);
        flashCoroutine = null;
    }

    private void OnDestroy()
    {
        if (materialInstance != null) {
            Destroy(materialInstance);
        }
    }
}