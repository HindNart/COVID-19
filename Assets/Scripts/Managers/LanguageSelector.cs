using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using TMPro;
using System.Linq;

public class LanguageSelector : MonoBehaviour
{

    [SerializeField] private GameObject languagePanel;
    [SerializeField] private Button enBtn;
    [SerializeField] private Button viBtn;

    private void Start()
    {
        enBtn.onClick.AddListener(() => OnLanguageChanged("en"));
        viBtn.onClick.AddListener(() => OnLanguageChanged("vi"));
    }

    private void OnLanguageChanged(string langCode)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales
            .FirstOrDefault(locale => locale.Identifier.Code == langCode);

        if (languagePanel != null)
        {
            languagePanel.SetActive(false);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }
    }

    private void OnDestroy()
    {
        enBtn.onClick.RemoveListener(() => OnLanguageChanged("en"));
        viBtn.onClick.RemoveListener(() => OnLanguageChanged("vi"));
    }
}