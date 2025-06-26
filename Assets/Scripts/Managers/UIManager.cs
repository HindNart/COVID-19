using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI antibodiesText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Slider playerHPSlider;
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private float pauseFadeDuration = 0.3f;

    [SerializeField] private PlayerController player;

    [SerializeField] private LocalizedString antibodiesString;
    [SerializeField] private LocalizedString waveString;
    [SerializeField] private LocalizedString upgradeCostCanUpgradeString;
    [SerializeField] private LocalizedString upgradeCostNotEnoughString;
    [SerializeField] private LocalizedString bossIncomingString;
    [SerializeField] private LocalizedString bossDefeatedString;
    [SerializeField] private LocalizedString waveStartingString;

    private LocalizeStringEvent antibodiesTextLocalize;
    private LocalizeStringEvent waveTextLocalize;
    private LocalizeStringEvent upgradeCostTextLocalize;
    private LocalizeStringEvent notificationTextLocalize;

    private CanvasGroup pauseMenuCanvasGroup;
    private bool isPaused;

    private void Start()
    {
        // Gán LocalizedString vào LocalizeStringEvent
        antibodiesTextLocalize = antibodiesText.GetComponent<LocalizeStringEvent>();
        waveTextLocalize = waveText.GetComponent<LocalizeStringEvent>();
        upgradeCostTextLocalize = upgradeCostText.GetComponent<LocalizeStringEvent>();
        notificationTextLocalize = notificationText.GetComponent<LocalizeStringEvent>();

        // Khởi tạo CanvasGroup cho pause menu
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvasGroup = pauseMenuCanvas.GetComponent<CanvasGroup>();
            if (pauseMenuCanvasGroup == null)
            {
                pauseMenuCanvasGroup = pauseMenuCanvas.AddComponent<CanvasGroup>();
            }
            pauseMenuCanvasGroup.alpha = 0f;
            pauseMenuCanvas.SetActive(false);
        }

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

        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
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
        antibodiesTextLocalize.StringReference = antibodiesString;
        antibodiesTextLocalize.StringReference.Arguments = new object[] { Mathf.Round(antibodies) };
        antibodiesTextLocalize.RefreshString();
        UpdateUpgradeCost();
    }

    private void UpdateWave(int wave)
    {
        waveTextLocalize.StringReference = waveString;
        waveTextLocalize.StringReference.Arguments = new object[] { wave };
        waveTextLocalize.RefreshString();
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
        notificationTextLocalize.StringReference = waveStartingString;
        notificationTextLocalize.RefreshString();
        StartCoroutine(DisplayNotification(2f));
    }

    private void UpdateUpgradeCost()
    {
        bool canUpgrade = player.CanUpgrade(GameManager.Instance.Antibodies);
        upgradeCostTextLocalize.StringReference = canUpgrade ? upgradeCostCanUpgradeString : upgradeCostNotEnoughString;
        upgradeCostTextLocalize.StringReference.Arguments = new object[] { player.GetUpgradeCost() };
        upgradeCostTextLocalize.RefreshString();
    }

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
        notificationTextLocalize.StringReference = bossIncomingString;
        notificationTextLocalize.RefreshString();
        StartCoroutine(DisplayNotification(3f));
    }

    public void ShowBossDefeated()
    {
        notificationTextLocalize.StringReference = bossDefeatedString;
        notificationTextLocalize.RefreshString();
        StartCoroutine(DisplayNotification(3f));
    }

    private IEnumerator DisplayNotification(float duration)
    {
        notificationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        notificationText.gameObject.SetActive(false);
    }

    private void PauseGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        if (pauseMenuCanvas == null) return;

        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuCanvas.SetActive(true);
        pauseMenuCanvasGroup.DOFade(1f, pauseFadeDuration).SetUpdate(true);
    }

    private void ResumeGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        if (pauseMenuCanvas == null) return;

        pauseMenuCanvasGroup.DOFade(0f, pauseFadeDuration).SetUpdate(true).OnComplete(() =>
        {
            pauseMenuCanvas.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
        });
    }

    private void ReturnToMainMenu()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        pauseMenuCanvasGroup.DOFade(0f, pauseFadeDuration).SetUpdate(true).OnComplete(() =>
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null) Destroy(GameManager.Instance.gameObject);
            if (WaveManager.Instance != null) Destroy(WaveManager.Instance.gameObject);
            if (AudioManager.Instance != null) Destroy(AudioManager.Instance.gameObject);
            SceneManager.LoadScene("Menu");
        });
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