using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class SpawnEntry
{
    public GameObject prefab;
    [Min(0f)] public float weight = 1f;
}

[Serializable]
public class SpawnPhase
{
    [Min(0f)] public float startTime;
    [Min(0f)] public float spawnsPerSecond = 1f;

    public bool unlockSlot = false;

    public List<SpawnEntry> enemies = new();
}

public class EnemySpawner : MonoBehaviour
{

    public float timeElapsed;
    public int currentPhaseIndex { get; private set; }
    public bool gameStarted;
    public float timeMultiplierOut;

    public Volume v;
    private ColorAdjustments colorAdjustments;
    public float inSat = 30f;
    public float outSat = -100f;

    public List<SpawnPhase> waves = new();

    private float timeUntilNextSpawn = 3f;
    public float timeMultiplier { get; private set; } = 1f;
    private Transform player;
    private PlayerShooting playerShooting;

    public TMP_Text timer;

    public SpriteRenderer[] enemyIndicators;

    public static Action playerOutside;
    public static Action UnlockSlot;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerShooting = player.GetComponent<PlayerShooting>();

        v.profile.TryGet(out colorAdjustments);
    }

    private void OnEnable()
    {
        MenuManager.GameStarted += Initialize;
    }

    private void OnDisable()
    {
        MenuManager.GameStarted -= Initialize;
    }

    public void Initialize()
    {
        Destroy(GameObject.FindGameObjectWithTag("BossAttack"));

        GameObject[] aliveEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in aliveEnemies) {
            Destroy(enemy);
        }

        StopAllCoroutines();
        colorAdjustments.saturation.value = inSat;

        currentPhaseIndex = 0;
        timeElapsed = 0f;
        UpdateSign();

        timer.text = "0:00";

        timeUntilNextSpawn = waves[0].spawnsPerSecond;

        gameStarted = true;
    }

    void Update()
    {
        IsPlayerOutofBounds();

        if (gameStarted)
            timeElapsed += Time.deltaTime * timeMultiplier;

        int minutes = (int)(timeElapsed / 60);
        int seconds = (int)timeElapsed - minutes * 60;
        string secs = (seconds > 9) ? seconds.ToString() : "0" + seconds;
        timer.text = minutes + ":" + secs;

        if (currentPhaseIndex + 1 <= waves.Count - 1) {
            if (timeElapsed >= waves[currentPhaseIndex + 1].startTime) {
                currentPhaseIndex++;
                UpdateSign();
                if (waves[currentPhaseIndex].unlockSlot) {
                    UnlockSlot?.Invoke();
                }
            }
        }

        if (timeElapsed >= timeUntilNextSpawn) {
            timeUntilNextSpawn = timeElapsed + waves[currentPhaseIndex].spawnsPerSecond;
            SpawnEnemy();
        }
    }

    void IsPlayerOutofBounds()
    {
        if (player.position.x > 10 || player.position.x < -10 || player.position.y > 8 || player.position.y < -8) {
            timeMultiplier = timeMultiplierOut;
        } else {
            timeMultiplier = 1f;
        }

    }

    void SpawnEnemy()
    {
        int r = RollEnemyIndex();
        Vector2 ScreenPosition = new Vector2(UnityEngine.Random.Range(-8.8f, 8.8f), UnityEngine.Random.Range(-6.6f, 6.6f));
        Instantiate(waves[currentPhaseIndex].enemies[r].prefab, ScreenPosition, Quaternion.identity);
    }

    int RollEnemyIndex()
    {
        float totalWeight = 0f;
        foreach (var entry in waves[currentPhaseIndex].enemies) {
            totalWeight += entry.weight;
        }
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        for (int i = 0; i < waves[currentPhaseIndex].enemies.Count; i++) {
            cumulativeWeight += waves[currentPhaseIndex].enemies[i].weight;
            if (randomValue <= cumulativeWeight) {
                return i;
            }
        }
        return waves[currentPhaseIndex].enemies.Count - 1;
    }

    void UpdateSign()
    {
        foreach (var indicator in enemyIndicators) {
            indicator.color = Color.black;
            if (indicator.transform.childCount > 0) {
                indicator.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.black;
            }
        }

        foreach (var entry in waves[currentPhaseIndex].enemies) {
            switch (entry.prefab.name) {
                case "BasicEnemy":
                    enemyIndicators[0].color = Color.white;
                    break;
                case "DasherEnemy":
                    enemyIndicators[1].color = Color.white;
                    break;
                case "TurretEnemy":
                    enemyIndicators[2].color = Color.white;
                    enemyIndicators[2].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
                    break;
                case "Mine Layer":
                    enemyIndicators[3].color = Color.white;
                    break;
                case "Splitter Slime Mother":
                    enemyIndicators[4].color = Color.white;
                    break;
                case "LaserTurretEnemy":
                    enemyIndicators[5].color = Color.white;
                    break;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player")) {
            playerShooting.isInBox = true;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag("Player"))
            return;

        StopAllCoroutines();
        if (timeMultiplier == timeMultiplierOut) {
            StartCoroutine(SwitchSaturation(outSat));
            playerOutside?.Invoke();
        } else {
            StartCoroutine(SwitchSaturation(inSat));
            Debug.Log("Player left box");
            playerShooting.isInBox = false;
            playerShooting.cantShootTimer = 0.35f;
        }
           
    }

    IEnumerator SwitchSaturation(float sat)
    {
        float startSat = colorAdjustments.saturation.value;
        float elapsed = 0f;
        float duration = 0.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            colorAdjustments.saturation.value = Mathf.Lerp(startSat, sat, elapsed / duration);
            yield return null;
        }

        colorAdjustments.saturation.value = sat;
    }
}
