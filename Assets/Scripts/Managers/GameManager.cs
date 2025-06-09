using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int PlayerHP { get; private set; } = 100;
    public int Antibodies { get; private set; } = 0;
    public int CurrentWave => waveManager.CurrentWave;

    public UnityEvent<int> OnAntibodiesChanged = new UnityEvent<int>();
    public UnityEvent<int> OnPlayerHPChanged = new UnityEvent<int>();
    public UnityEvent OnGameOver = new UnityEvent();

    [SerializeField] private PlayerController player;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private GameObject[] powerUpPrefabs;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.TryUpgrade(Antibodies);
        }
    }

    public void AddAntibodies(int amount)
    {
        Antibodies += amount;
        OnAntibodiesChanged.Invoke(Antibodies);

        if (Random.value < 0.05f) // 5% drop power-up
        {
            SpawnPowerUp((Vector2)player.transform.position + Random.insideUnitCircle * 3f);
        }
    }

    public void TakePlayerDamage(int damage)
    {
        if (player.HasShield())
        {
            player.ConsumeShieldHit();
            return;
        }

        PlayerHP = Mathf.Max(0, PlayerHP - damage);
        OnPlayerHPChanged.Invoke(PlayerHP);
        if (PlayerHP <= 0)
        {
            OnGameOver.Invoke();
        }
    }

    public void HealPlayer(int amount)
    {
        PlayerHP = Mathf.Min(100, PlayerHP + amount);
        OnPlayerHPChanged.Invoke(PlayerHP);
    }

    public void TriggerExplosion()
    {
        VirusBase[] viruses = FindObjectsOfType<VirusBase>();
        foreach (var virus in viruses)
        {
            virus.Die();
        }
    }

    private void SpawnPowerUp(Vector2 position)
    {
        if (powerUpPrefabs.Length > 0)
        {
            int index = Random.Range(0, powerUpPrefabs.Length);
            GameObject powerUp = FindObjectOfType<ObjectPool>().Get(powerUpPrefabs[index]);
            powerUp.transform.position = position;
        }
    }
}