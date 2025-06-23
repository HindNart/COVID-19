using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public readonly int maxPlayerHP = 100;
    public int PlayerHP { get; private set; } = 100;
    public float Antibodies { get; private set; } = 0;
    public int CurrentWave => WaveManager.Instance.CurrentWave;

    public UnityEvent<float> OnAntibodiesChanged = new UnityEvent<float>();
    public UnityEvent<int> OnPlayerHPChanged = new UnityEvent<int>();
    public UnityEvent OnGameOver = new UnityEvent();

    [SerializeField] private PlayerController player;
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

    public void AddAntibodies(float amount)
    {
        Antibodies += amount;
        OnAntibodiesChanged?.Invoke(Antibodies);
    }

    public void TakePlayerDamage(int damage)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("VirusAtk");
        }

        if (player.HasShield())
        {
            return;
        }

        PlayerHP = Mathf.Max(0, PlayerHP - damage);
        OnPlayerHPChanged?.Invoke(PlayerHP);
        if (PlayerHP <= 0)
        {
            StartCoroutine(WaitForGameOver());
        }
    }

    private IEnumerator WaitForGameOver()
    {
        yield return new WaitForSeconds(0.5f);
        OnGameOver?.Invoke();
    }

    public void HealPlayer(int amount)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Heal");
        }

        PlayerHP = Mathf.Min(100, PlayerHP + amount);
        OnPlayerHPChanged?.Invoke(PlayerHP);
    }

    public void TriggerExplosion()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Explosion");
        }

        VirusBase[] viruses = FindObjectsOfType<VirusBase>();
        foreach (var virus in viruses)
        {
            virus.Die();
        }
    }

    public void SpawnPowerUp(Vector2 position)
    {
        if (powerUpPrefabs.Length > 0)
        {
            int index = Random.Range(0, powerUpPrefabs.Length);
            GameObject powerUp = FindObjectOfType<ObjectPool>().Get(powerUpPrefabs[index]);
            powerUp.transform.position = position;
        }
    }
}




// tạo thêm list boss
// màn boss cập nhật lại