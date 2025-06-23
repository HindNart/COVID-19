using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public struct AudioClipEntry
    {
        public string key;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClipEntry[] sfxClips;
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private float bgmVolume = 0.5f;
    [SerializeField] private float sfxVolume = 0.8f;
    [SerializeField] private int maxSfxSources = 10;

    private Dictionary<string, AudioClipEntry> sfxDictionary;
    private List<AudioSource> sfxSources;
    private int currentSfxSourceIndex;

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
            return;
        }

        // Khởi tạo dictionary cho SFX
        sfxDictionary = new Dictionary<string, AudioClipEntry>();
        foreach (var entry in sfxClips)
        {
            if (!sfxDictionary.ContainsKey(entry.key))
            {
                sfxDictionary.Add(entry.key, entry);
            }
        }

        // Khởi tạo danh sách AudioSource cho SFX
        sfxSources = new List<AudioSource>();
        for (int i = 0; i < maxSfxSources; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxSources.Add(source);
        }

        // Đảm bảo bgmSource
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
        }
    }

    public void PlayBGM()
    {
        if (bgmClip == null || bgmSource.clip == bgmClip) return;

        bgmSource.clip = bgmClip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(string key)
    {
        if (sfxDictionary.ContainsKey(key))
        {
            AudioClipEntry entry = sfxDictionary[key];
            AudioSource source = GetAvailableSFXSource();
            source.PlayOneShot(entry.clip, entry.volume * sfxVolume);
        }
        else
        {
            Debug.LogWarning($"SFX key '{key}' not found.");
        }
    }

    private AudioSource GetAvailableSFXSource()
    {
        for (int i = 0; i < sfxSources.Count; i++)
        {
            int index = (currentSfxSourceIndex + i) % sfxSources.Count;
            if (!sfxSources[index].isPlaying)
            {
                currentSfxSourceIndex = (index + 1) % sfxSources.Count;
                return sfxSources[index];
            }
        }

        // Nếu tất cả source đang phát, trả về source tiếp theo (ghi đè)
        currentSfxSourceIndex = (currentSfxSourceIndex + 1) % sfxSources.Count;
        return sfxSources[currentSfxSourceIndex];
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}