using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button languageButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject languagePanel;
    [SerializeField] private string gameSceneName = "Game";

    private void Start()
    {
        if (languagePanel != null)
        {
            languagePanel.SetActive(false);
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
            SetupButtonAnimation(playButton);
        }
        if (languageButton != null)
        {
            languageButton.onClick.AddListener(OnLanguageButtonClicked);
            SetupButtonAnimation(languageButton);
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
            SetupButtonAnimation(quitButton);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM();
        }
    }

    private void OnPlayButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        SceneManager.LoadScene(gameSceneName);
    }

    private void OnLanguageButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        if (languagePanel != null)
        {
            languagePanel.SetActive(!languagePanel.activeSelf);
        }
        else
        {
            Debug.LogWarning("Language panel is not assigned in the inspector.");
        }
    }

    private void OnQuitButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

#if UNITY_EDITOR
        Debug.Log("Quit button clicked. Stopping play mode in Editor.");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetupButtonAnimation(Button button)
    {
        // Thêm hiệu ứng scale khi hover
        var eventTrigger = button.gameObject.AddComponent<EventTrigger>();
        var pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnter.callback.AddListener((data) =>
        {
            button.transform.DOScale(1.1f, 0.3f).SetEase(Ease.OutQuad);
        });
        eventTrigger.triggers.Add(pointerEnter);

        var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((data) =>
        {
            button.transform.DOScale(1f, 0.3f).SetEase(Ease.OutQuad);
        });
        eventTrigger.triggers.Add(pointerExit);

        var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((data) =>
        {
            button.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad);
        });
        eventTrigger.triggers.Add(pointerDown);

        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((data) =>
        {
            button.transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
        });
        eventTrigger.triggers.Add(pointerUp);
    }

    private void OnDestroy()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
        }
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }
    }
}