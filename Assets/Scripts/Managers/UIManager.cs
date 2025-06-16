using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI antibodiesText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;

    // [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    [SerializeField] private PlayerController player;

    // private void Awake()
    // {
    //     if (Instance == null)
    //     {
    //         Instance = this;
    //         DontDestroyOnLoad(gameObject);
    //     }
    //     else
    //     {
    //         Destroy(gameObject);
    //     }
    // }

    private void Start()
    {
        // Khởi tạo UI
        UpdateHP(GameManager.Instance.PlayerHP);
        UpdateAntibodies(GameManager.Instance.Antibodies);
        UpdateWave(WaveManager.Instance.CurrentWave);
        UpdateWaveTime(WaveManager.Instance.CurrentWave > 0 ? 0f : 15f);
        UpdateUpgradeCost();
        gameOverPanel.SetActive(false);
        notificationText.gameObject.SetActive(false);

        // Đăng ký sự kiện
        GameManager.Instance.OnPlayerHPChanged.AddListener(UpdateHP);
        GameManager.Instance.OnAntibodiesChanged.AddListener(UpdateAntibodies);
        GameManager.Instance.OnGameOver.AddListener(ShowGameOver);
        WaveManager.Instance.OnWaveStarted.AddListener(UpdateWave);
        WaveManager.Instance.OnWaveTimeUpdated.AddListener(UpdateWaveTime);
        WaveManager.Instance.OnBossSpawned.AddListener(ShowBossWarning);
        BossVirus.onBossDefeated.AddListener(ShowBossDefeated);
        // upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        restartButton.onClick.AddListener(RestartGame);
    }

    private void UpdateHP(int hp)
    {
        hpText.text = $"HP: {hp}";
    }

    private void UpdateAntibodies(float antibodies)
    {
        antibodiesText.text = $"Antibodies: {Mathf.Round(antibodies)}";
        UpdateUpgradeCost();
    }

    private void UpdateWave(int wave)
    {
        waveText.text = $"Wave: {wave}";
    }

    private void UpdateWaveTime(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timeText.text = $"Time Left: {minutes:00}:{seconds:00}";
    }

    private void UpdateUpgradeCost()
    {
        bool canUpgrade = player.CanUpgrade(GameManager.Instance.Antibodies);
        upgradeCostText.text = $"Upgrade ({(canUpgrade ? "Available" : "Not Enough")}): {player.GetUpgradeCost()}";
        // upgradeButton.interactable = canUpgrade;
    }

    // private void OnUpgradeButtonClicked()
    // {
    //     player.TryUpgrade(GameManager.Instance.Antibodies);
    // }

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void ShowBossWarning()
    {
        StartCoroutine(DisplayNotification("Boss Incoming!", 3f));
    }

    public void ShowBossDefeated()
    {
        StartCoroutine(DisplayNotification("Boss Defeated!", 3f));
    }

    private IEnumerator DisplayNotification(string message, float duration)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        notificationText.gameObject.SetActive(false);
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }
        if (WaveManager.Instance != null)
        {
            Destroy(WaveManager.Instance.gameObject);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh rò rỉ bộ nhớ
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerHPChanged.RemoveListener(UpdateHP);
            GameManager.Instance.OnAntibodiesChanged.RemoveListener(UpdateAntibodies);
            GameManager.Instance.OnGameOver.RemoveListener(ShowGameOver);
        }
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStarted.RemoveListener(UpdateWave);
            WaveManager.Instance.OnWaveTimeUpdated.RemoveListener(UpdateWaveTime);
            WaveManager.Instance.OnBossSpawned.RemoveListener(ShowBossWarning);
        }
    }
}