using TMPro;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    public float firstHintStartTime;
    public float secondHintStartTime;

    public float firstHintEndTime;
    public float secondHintEndTime;
    public float thirdHintTime;

    public string firstHint;
    public string secondHint;
    public string thirdHint;

    public Transform canvas;
    public GameObject hintPrefab;
    public GameObject currentHint;

    public float timer;
    public float timeTillThirdHintDestroyed;
    public bool gameStarted;

    private bool firstHintDone, secondHintDone, thirdHintDone;

    private void OnEnable()
    {
        MenuManager.GameStarted += GameStarted;
        MenuManager.ResumeGame += PauseAndResume;
        PlayerHealth.OnDied += PlayerDied;
        MenuManager.Paused += PauseAndResume;
        EnemySpawner.playerOutside += ShowThirdHint;
    }

    private void OnDisable()
    {
        MenuManager.GameStarted -= GameStarted;
        MenuManager.ResumeGame -= PauseAndResume;
        PlayerHealth.OnDied -= PlayerDied;
        MenuManager.Paused -= PauseAndResume;
        EnemySpawner.playerOutside -= ShowThirdHint;
    }

    void Update()
    {
        if (!gameStarted)
            return;

        timer += Time.deltaTime;
        if (thirdHintDone && timer > timeTillThirdHintDestroyed) {
            Destroy(currentHint);
        }
        if (thirdHintDone)
            return;

        if (timer > firstHintStartTime && !firstHintDone) {
            currentHint = Instantiate(hintPrefab, canvas);
            currentHint.GetComponentInChildren<TMP_Text>().text = firstHint;
            firstHintDone = true;
        }
        if (timer > firstHintEndTime && timer < secondHintStartTime) {
            Destroy(currentHint);
        }

        if (timer > secondHintStartTime && !secondHintDone) {
            currentHint = Instantiate(hintPrefab, canvas);
            currentHint.GetComponentInChildren<TMP_Text>().text = secondHint;
            secondHintDone = true;
        }
        if (timer > secondHintEndTime && !thirdHintDone) {
            Destroy(currentHint);
        }

    }

    void GameStarted()
    {
        gameStarted = true;
    }

    void PlayerDied()
    {
        gameStarted = false;
        Destroy(currentHint);
    }

    void PauseAndResume()
    {
        gameStarted = !gameStarted;
        if (currentHint != null) {
            currentHint.SetActive(gameStarted);
        }
    }

    void ShowThirdHint()
    {
        if (thirdHintDone)
            return;

        Destroy(currentHint);
        currentHint = Instantiate(hintPrefab, canvas);
        currentHint.GetComponentInChildren<TMP_Text>().text = thirdHint;
        thirdHintDone = true;
        timeTillThirdHintDestroyed = timer + thirdHintTime;
    }

}
