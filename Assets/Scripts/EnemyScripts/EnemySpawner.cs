using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

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

    public List<SpawnEntry> enemies = new();
}

public class EnemySpawner : MonoBehaviour
{

    public float timeElapsed;
    public int currentPhaseIndex { get; private set; }
    public bool gameStarted;
    public float timeMultiplierOut;

    public List <SpawnPhase> waves = new();

    private float timeUntilNextSpawn = 3f;
    public float timeMultiplier { get; private set; } = 1f;
    private Transform player;

    public TMP_Text timer;

    public static Action playerOutside;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; 
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
        GameObject[] aliveEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in aliveEnemies) {
            Destroy(enemy);
        }

        currentPhaseIndex = 0;
        timeElapsed = 0f;

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

        if (currentPhaseIndex + 1 <= waves.Count - 1)
        {
            if (timeElapsed >= waves[currentPhaseIndex + 1].startTime)
            {
                currentPhaseIndex++;
            }
        }

        if (timeElapsed >= timeUntilNextSpawn)
        {
            timeUntilNextSpawn = timeElapsed + waves[currentPhaseIndex].spawnsPerSecond;
            SpawnEnemy();
        }
    }

    void IsPlayerOutofBounds()
    {
        if (player.position.x > 10 || player.position.x < -10 || player.position.y > 8 || player.position.y < -8) {
            timeMultiplier = timeMultiplierOut;
            playerOutside?.Invoke();
        }
        else 
            timeMultiplier = 1f;
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
        foreach (var entry in waves[currentPhaseIndex].enemies)
        {
            totalWeight += entry.weight;
        }
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        for (int i = 0; i < waves[currentPhaseIndex].enemies.Count; i++)
        {
            cumulativeWeight += waves[currentPhaseIndex].enemies[i].weight;
            if (randomValue <= cumulativeWeight)
            {
                return i;
            }
        }
        return waves[currentPhaseIndex].enemies.Count - 1;
    }

}
