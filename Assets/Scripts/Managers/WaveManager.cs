using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private VirusFactory virusFactory;
    [SerializeField] private ObjectPool objectPool;
    [SerializeField] private float waveInterval = 30f;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int baseVirusCount = 5;
    [SerializeField] private float difficultyIncrease = 0.1f;

    public int CurrentWave { get; private set; } = 0;
    public UnityEvent<int> OnWaveStarted = new UnityEvent<int>();
    public UnityEvent<int> OnWaveEnded = new UnityEvent<int>();

    private bool isWaveActive;
    private Queue<Vector2> spawnQueue = new Queue<Vector2>();
    private float nextWaveTime;
    private float nextSpawnTime;

    private void Start()
    {
        nextWaveTime = Time.time + waveInterval;
    }

    private void Update()
    {
        if (!isWaveActive && Time.time >= nextWaveTime)
        {
            StartWave();
        }

        if (isWaveActive && Time.time >= nextSpawnTime && spawnQueue.Count > 0)
        {
            SpawnVirus();
            nextSpawnTime = Time.time + spawnInterval;
        }

        if (isWaveActive && spawnQueue.Count == 0 && !AnyVirusActive())
        {
            EndWave();
        }
    }

    private void StartWave()
    {
        CurrentWave++;
        isWaveActive = true;
        OnWaveStarted.Invoke(CurrentWave);

        int virusCount = Mathf.RoundToInt(baseVirusCount + CurrentWave * 2);
        float hpMultiplier = 1f + CurrentWave * difficultyIncrease;
        float speedMultiplier = 1f + CurrentWave * difficultyIncrease * 0.5f;

        spawnQueue.Clear();
        for (int i = 0; i < virusCount; i++)
        {
            spawnQueue.Enqueue(GetSpawnPosition(CurrentWave));
        }

        virusFactory.SetWaveParameters(hpMultiplier, speedMultiplier);
    }

    private void SpawnVirus()
    {
        if (spawnQueue.Count > 0)
        {
            Vector2 spawnPos = spawnQueue.Dequeue();
            virusFactory.SpawnVirus(CurrentWave, spawnPos);
        }
    }

    private void EndWave()
    {
        isWaveActive = false;
        nextWaveTime = Time.time + waveInterval;
        OnWaveEnded.Invoke(CurrentWave);
    }

    private Vector2 GetSpawnPosition(int wave)
    {
        float rand = Random.value;
        if (wave % 10 == 0) // Wave boss
        {
            if (rand < 0.25f) return new Vector2(Random.Range(-8f, 8f), 6f);
            else if (rand < 0.5f) return new Vector2(Random.Range(-8f, 8f), -6f);
            else if (rand < 0.75f) return new Vector2(-8f, Random.Range(-6f, 6f));
            else return new Vector2(8f, Random.Range(-6f, 6f));
        }
        else if (wave > 5) // Wave trung cấp
        {
            float x = Random.Range(-8f, 8f);
            return Random.value < 0.5f ? new Vector2(x, 6f) : new Vector2(x, -6f);
        }
        else // Wave cơ bản
        {
            if (rand < 0.25f) return new Vector2(Random.Range(-8f, 8f), 6f);
            else if (rand < 0.5f) return new Vector2(Random.Range(-8f, 8f), -6f);
            else if (rand < 0.75f) return new Vector2(-8f, Random.Range(-6f, 6f));
            else return new Vector2(8f, Random.Range(-6f, 6f));
        }
    }

    private bool AnyVirusActive()
    {
        return FindObjectsOfType<VirusBase>().Length > 0;
    }
}