using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI antibodiesText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    // [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Slider playerHPSlider;

    [SerializeField] private PlayerController player;

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
        WaveManager.Instance.OnWaveEnded.AddListener(ShowWaveTime);
        WaveManager.Instance.OnWaveIntervalEnded.AddListener(HideWaveTime);
        WaveManager.Instance.OnWaveTimeUpdated.AddListener(UpdateWaveTime);
        WaveManager.Instance.OnBossSpawned.AddListener(ShowBossWarning);
        BossVirus.onBossDefeated.AddListener(ShowBossDefeated);

        // upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        restartButton.onClick.AddListener(RestartGame);
    }

    private void UpdateHP(int hp)
    {
        float hpRatio = (float)hp / GameManager.Instance.maxPlayerHP;
        playerHPSlider.DOValue(hpRatio, 0.5f).SetEase(Ease.OutQuad);

        if (hpRatio <= 0.3f)
        {
            playerHPSlider.fillRect.GetComponent<Image>().DOColor(Color.yellow, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            playerHPSlider.fillRect.GetComponent<Image>().DOKill();
            playerHPSlider.fillRect.GetComponent<Image>().DOColor(new Color32(0, 223, 255, 255), 0.5f);
        }
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
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    private void ShowWaveTime()
    {
        timeText.gameObject.SetActive(true);
        timeText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    private void HideWaveTime()
    {
        timeText.gameObject.SetActive(false);
        timeText.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
        StartCoroutine(DisplayNotification("Wave Starting!", 2f));
    }

    private void UpdateUpgradeCost()
    {
        bool canUpgrade = player.CanUpgrade(GameManager.Instance.Antibodies);
        upgradeCostText.text = $"Upgrade ({(canUpgrade ? "Click Earth" : "Not Enough")}): {player.GetUpgradeCost()}";
        // upgradeButton.interactable = canUpgrade;
    }

    // private void OnUpgradeButtonClicked()
    // {
    //     player.TryUpgrade(GameManager.Instance.Antibodies);
    // }

    private void ShowGameOver()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("GameOver");
        }

        gameOverPanel.SetActive(true);
        gameOverPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerHPChanged.RemoveListener(UpdateHP);
            GameManager.Instance.OnAntibodiesChanged.RemoveListener(UpdateAntibodies);
            GameManager.Instance.OnGameOver.RemoveListener(ShowGameOver);
        }
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStarted.RemoveListener(UpdateWave);
            WaveManager.Instance.OnWaveEnded.RemoveListener(ShowWaveTime);
            WaveManager.Instance.OnWaveIntervalEnded.RemoveListener(HideWaveTime);
            WaveManager.Instance.OnWaveTimeUpdated.RemoveListener(UpdateWaveTime);
            WaveManager.Instance.OnBossSpawned.RemoveListener(ShowBossWarning);
        }
        BossVirus.onBossDefeated.RemoveListener(ShowBossDefeated);
    }
}