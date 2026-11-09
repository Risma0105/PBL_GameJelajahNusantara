
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public struct SoundItem
    {
        public string namaSuara;
        public AudioClip clipAudio;
    }

    [Header("Daftar SFX")]
    public List<SoundItem> daftarSFX = new List<SoundItem>();

    [Header("Daftar BGM")]
    public List<SoundItem> daftarBGM = new List<SoundItem>();

    [Header("Pengaturan Fade")]
    public float fadeDuration = 2f;

    private AudioSource sfxSource;
    private AudioSource bgmSource;
    private AudioSource loopSource;

    private Dictionary<AudioClip, float> posisiBGM =
        new Dictionary<AudioClip, float>();

    private string sceneSekarang;
    private Coroutine fadeCoroutine;

    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    // [SFX] Referensi Rigidbody Player
private Rigidbody2D playerRb;

    void Awake()
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

        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length > 0)
            sfxSource = sources[0];
        else
            sfxSource = gameObject.AddComponent<AudioSource>();

        if (sources.Length > 1)
            bgmSource = sources[1];
        else
            bgmSource = gameObject.AddComponent<AudioSource>();

        if (sources.Length > 2)
            loopSource = sources[2];
        else
            loopSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        bgmSource.playOnAwake = false;
        loopSource.playOnAwake = false;

        sfxSource.volume = sfxVolume;
        loopSource.volume = sfxVolume;

        bgmSource.volume = bgmVolume;

        SceneManager.sceneLoaded += GantiScene;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= GantiScene;
            Instance = null;
        }
    }

    // Mendeteksi perpindahan scene
    void GantiScene(Scene scene, LoadSceneMode mode)
    {
        string namaSceneBaru = scene.name;

        if (sceneSekarang == namaSceneBaru)
            return;

        SoundItem sound = daftarBGM.Find(
            s => s.namaSuara == namaSceneBaru
        );

        if (sound.clipAudio == null)
        {
            Debug.LogWarning(
                "BGM scene '" + namaSceneBaru + "' tidak ditemukan!"
            );
            return;
        }

        if (bgmSource.clip == sound.clipAudio && bgmSource.isPlaying)
        {
            sceneSekarang = namaSceneBaru;
            return;
        }

        if (bgmSource.clip != null)
        {
            posisiBGM[bgmSource.clip] = bgmSource.time;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        bgmSource.Stop();

        PlayBGM(namaSceneBaru);
    }

    // Memutar BGM berdasarkan nama scene
    public void PlayBGM(string nama)
    {
        SoundItem sound = daftarBGM.Find(
            s => s.namaSuara == nama
        );

        if (sound.clipAudio == null)
        {
            Debug.LogWarning("BGM '" + nama + "' tidak ditemukan!");
            return;
        }

        sceneSekarang = nama;

        bgmSource.clip = sound.clipAudio;
        bgmSource.loop = true;

        if (posisiBGM.ContainsKey(sound.clipAudio))
            bgmSource.time = posisiBGM[sound.clipAudio];
        else
            bgmSource.time = 0;

        bgmSource.volume = 0;
        bgmSource.Play();

        fadeCoroutine = StartCoroutine(FadeIn());
    }

    // Efek fade in BGM
    IEnumerator FadeIn()
    {
        float waktu = 0;

        if (fadeDuration <= 0)
        {
            bgmSource.volume = bgmVolume;
            fadeCoroutine = null;
            yield break;
        }

        while (waktu < fadeDuration)
        {
            waktu += Time.deltaTime;

            bgmSource.volume = Mathf.Lerp(
                0,
                bgmVolume,
                waktu / fadeDuration
            );

            yield return null;
        }

        bgmSource.volume = bgmVolume;
        fadeCoroutine = null;
    }

    // SFX sekali putar
    public void PlaySFX(string nama)
    {
        SoundItem sound = daftarSFX.Find(
            s => s.namaSuara == nama
        );

        if (sound.clipAudio != null)
        {
            sfxSource.PlayOneShot(sound.clipAudio);
        }
        else
        {
            Debug.LogWarning("SFX '" + nama + "' tidak ditemukan!");
        }
    }

    // SFX berulang (looping)
    public void PlayLoopSFX(string nama)
    {
        SoundItem sound = daftarSFX.Find(
            s => s.namaSuara == nama
        );

        if (sound.clipAudio == null)
        {
            Debug.LogWarning("SFX '" + nama + "' tidak ditemukan!");
            return;
        }

        if (loopSource.isPlaying &&
            loopSource.clip == sound.clipAudio)
            return;

        loopSource.Stop();

        loopSource.clip = sound.clipAudio;
        loopSource.loop = true;
        loopSource.volume = sfxVolume;
        loopSource.Play();
    }

    // Menghentikan SFX looping
    public void StopLoopSFX()
    {
        if (loopSource != null)
            loopSource.Stop();
    }

    // Mengatur volume BGM
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (bgmSource != null && fadeCoroutine == null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    // Mengatur volume seluruh SFX
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        if (sfxSource != null)
            sfxSource.volume = sfxVolume;

        if (loopSource != null)
            loopSource.volume = sfxVolume;
    }
    // [SFX] Deteksi gerakan Player
void Update()
{
    if (playerRb == null)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            playerRb = player.GetComponent<Rigidbody2D>();
    }

    if (playerRb != null &&
        playerRb.linearVelocity.sqrMagnitude > 0.01f)
    {
        PlayLoopSFX("Footstep");
    }
    else
    {
        StopLoopSFX();
    }
}
}
