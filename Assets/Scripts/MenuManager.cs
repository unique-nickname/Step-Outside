using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject pausedSettingsPanel;

    public float appearTime;

    public AudioClip backgroundMusic;

    private GameObject currentPanel;
    private PlayerShooting playerShooting;

    public List<GameObject> gameUI = new();
    public static event Action GameStarted;
    public static event Action ResumeGame;
    public static event Action Paused;

    private void Start()
    {
        playerShooting = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerShooting>();
        ShowMainMenu();
    }
    private void OnEnable()
    {
        PlayerHealth.OnDied += InitializeDeath;
        PlayerController.paused += Pause;
        BossCore.OnBossDied += ShowWin;
    }

    private void OnDisable()
    {
        PlayerHealth.OnDied -= InitializeDeath;
        PlayerController.paused -= Pause;
        BossCore.OnBossDied -= ShowWin;
    }

    public void HideAll()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        pausedSettingsPanel.SetActive(false);

        currentPanel = null;
    }

    public void ShowMainMenu()
    {
        AudioManager.Instance.StopMusic();
        HideAll();
        mainMenuPanel.SetActive(true);
        currentPanel = mainMenuPanel;
    }

    public void ShowOptions()
    {
        HideAll();
        optionsPanel.SetActive(true);
        currentPanel = optionsPanel;
    }

    public void ShowGameOver()
    {
        HideAll();
        gameOverPanel.SetActive(true);
        currentPanel = gameOverPanel;
    }

    public void ShowPausedSettings()
    {
        HideAll();
        pausedSettingsPanel.SetActive(true);
        currentPanel = pausedSettingsPanel;
    }

    public void CloseCurrentMenu()
    {
        if (currentPanel != null) {
            currentPanel.SetActive(false);
            currentPanel = null;
        }
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        Time.timeScale = 1;
        CloseCurrentMenu();
        foreach (var ui in gameUI) {
            if (ui.gameObject.CompareTag("Feather Bar")) {
                ui.SetActive(false);
                continue;
            }
            ui.SetActive(true);
        }
        GameStarted?.Invoke();
        AudioManager.Instance.PlayMusic(backgroundMusic, 0.45f);
    }

    private void InitializeDeath()
    {
        Time.timeScale = 0;
        ShowGameOver();
    }

    public void Pause()
    {
        if (currentPanel == mainMenuPanel || currentPanel == optionsPanel || currentPanel == gameOverPanel || currentPanel == winPanel)
            return;
        Paused?.Invoke();
        if (currentPanel == pausePanel) {
            CloseCurrentMenu();
            playerShooting.canShoot = true;
            Time.timeScale = 1f;
            return;
        }
        HideAll();
        pausePanel.SetActive(true);
        currentPanel = pausePanel;

        playerShooting.canShoot = false;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        CloseCurrentMenu();
        playerShooting.canShoot = true;
        Time.timeScale = 1f;
        ResumeGame?.Invoke();
    }

    private void ShowWin()
    {
        StartCoroutine(ShowWinCoroutine());
    }

    private IEnumerator ShowWinCoroutine()
    {
        HideAll();
        winPanel.SetActive(true);
        currentPanel = winPanel;

        Image[] sprites = winPanel.GetComponentsInChildren<Image>();
        float alpha = 0f;
        float fadedAlpha = 0f;

        float t = 0f;

        while (t < appearTime) {
            t += Time.deltaTime;
            alpha = Mathf.Lerp(alpha, 1f, t / appearTime);
            fadedAlpha = Mathf.Lerp(fadedAlpha, 0.53f, t / appearTime);
            foreach (Image sprite in sprites) {
                Color c = sprite.color;
                if (sprite.gameObject.name == "Felled") {
                    sprite.color = new Color(c.r, c.g, c.b, fadedAlpha);
                    sprite.GetComponentInChildren<TMP_Text>().color = new Color(1f, 1f, 0f, alpha);
                    continue;
                }
                sprite.GetComponentInChildren<TMP_Text>().color = new Color(0f, 0f, 0f, alpha);
            }

            yield return null;
        }
    }
}