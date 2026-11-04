using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public struct SoundItem
    {
        public string namaSuara;    
        public AudioClip clipAudio; 
    }

    [Header("Daftar SFX (Suara Pendek / Efek)")]
    public List<SoundItem> daftarSFX = new List<SoundItem>();

    [Header("Daftar BGM (Musik Latar Belakang)")]
    public List<SoundItem> daftarBGM = new List<SoundItem>();

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Biar musik BGM tidak mati saat pindah scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Otomatis buat 2 AudioSource terpisah untuk SFX dan BGM
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length < 2)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            sfxSource = sources[0];
            bgmSource = sources[1];
        }
    }

    // Fungsi untuk memutar SFX
    public void PlaySFX(string nama)
    {
        SoundItem sound = daftarSFX.Find(s => s.namaSuara == nama);
        if (sound.clipAudio != null)
        {
            sfxSource.PlayOneShot(sound.clipAudio);
        }
        else
        {
            Debug.LogWarning("SFX '" + nama + "' tidak ditemukan!");
        }
    }

    // Fungsi untuk memutar BGM (Otomatis Looping)
    public void PlayBGM(string nama)
    {
        SoundItem sound = daftarBGM.Find(s => s.namaSuara == nama);
        if (sound.clipAudio != null)
        {
            // Kalau BGM yang sama lagi diputar, biarkan jalan terus
            if (bgmSource.clip == sound.clipAudio && bgmSource.isPlaying) return;

            bgmSource.clip = sound.clipAudio;
            bgmSource.loop = true; // Musik latar wajib looping
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning("BGM '" + nama + "' tidak ditemukan!");
        }
    }
}