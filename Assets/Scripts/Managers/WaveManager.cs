using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [SerializeField] private VirusFactory virusFactory;
    [SerializeField] private ObjectPool objectPool;
    [SerializeField] private float waveInterval = 15f;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private int baseVirusCount = 25;
    [SerializeField] private float difficultyIncrease = 0.5f;

    public int CurrentWave { get; private set; } = 0;
    public UnityEvent<int> OnWaveStarted = new UnityEvent<int>();
    // public UnityEvent<int> OnWaveEnded = new UnityEvent<int>();
    public UnityEvent<float> OnWaveTimeUpdated = new UnityEvent<float>();
    public UnityEvent OnBossSpawned = new UnityEvent();

    private bool isWaveActive;
    private Queue<Vector2> spawnQueue = new Queue<Vector2>();
    private float nextWaveTime;
    private float nextSpawnTime;
    private float timeLeft;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        nextWaveTime = Time.time + waveInterval;
        timeLeft = waveInterval;
        OnWaveTimeUpdated.Invoke(timeLeft);
    }

    private void Update()
    {
        if (isWaveActive)
        {
            timeLeft = Mathf.Max(0, nextWaveTime - Time.time);
            OnWaveTimeUpdated.Invoke(timeLeft);
        }
        else if (!isWaveActive && timeLeft > 0)
        {
            timeLeft = Mathf.Max(0, nextWaveTime - Time.time);
            OnWaveTimeUpdated.Invoke(timeLeft);
        }

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
        timeLeft = waveInterval;
        OnWaveStarted.Invoke(CurrentWave);
        OnWaveTimeUpdated.Invoke(timeLeft);

        int virusCount = CurrentWave % 10 == 0 ? 1 : Mathf.RoundToInt(baseVirusCount + CurrentWave * 2);
        float hpMultiplier = 1f + CurrentWave * difficultyIncrease;
        float speedMultiplier = 1f + CurrentWave * difficultyIncrease * 0.5f;

        spawnQueue.Clear();
        for (int i = 0; i < virusCount; i++)
        {
            spawnQueue.Enqueue(GetSpawnPosition(CurrentWave));
        }

        if (CurrentWave % 10 == 0)
        {
            OnBossSpawned.Invoke();
        }

        if (virusFactory != null)
        {
            virusFactory.SetWaveParameters(hpMultiplier, speedMultiplier);
        }
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
        timeLeft = waveInterval;
        // OnWaveEnded.Invoke(CurrentWave);
        OnWaveTimeUpdated.Invoke(timeLeft);
    }

    private Vector2 GetSpawnPosition(int wave)
    {
        float rand = Random.value;
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        if (wave % 10 == 0) // Wave boss
        {
            return new Vector2(screenBounds.x, 0);
        }
        else // Wave cơ bản
        {
            if (rand < 0.2f) return new Vector2(Random.Range(-screenBounds.x, screenBounds.x), screenBounds.y);
            else if (rand < 0.4f) return new Vector2(Random.Range(-screenBounds.x, screenBounds.x), -screenBounds.y);
            else if (rand < 0.7f) return new Vector2(-screenBounds.x, Random.Range(-screenBounds.y, screenBounds.y));
            else return new Vector2(screenBounds.x, Random.Range(-screenBounds.y, screenBounds.y));
        }
    }

    private bool AnyVirusActive()
    {
        return FindObjectsOfType<VirusBase>().Length > 0;
    }
}