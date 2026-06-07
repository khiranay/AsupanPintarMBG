using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource bgm;

    public GameObject soundOnIcon;
    public GameObject soundOffIcon;

    [Header("BGM Level")]
    public AudioClip bgmLevel; 
    // assign clip musik level di Inspector
    [Header("SFX")]
public AudioClip sfxBenar;
public AudioClip sfxSalah;



    void Start()
    {
        if (bgm == null)
            Debug.LogError("[AudioManager] AudioSource 'bgm' belum di-assign di Inspector!");

        if (soundOnIcon == null || soundOffIcon == null)
            Debug.LogWarning("[AudioManager] soundOnIcon / soundOffIcon belum di-assign.");
    }

    public void ToggleSound()
    {
        if (bgm == null) return;

        bool isMuted = bgm.mute;
        bgm.mute = !isMuted;

        if (soundOnIcon != null)  soundOnIcon.SetActive(isMuted);
        if (soundOffIcon != null) soundOffIcon.SetActive(!isMuted);
    }

    public void MainBGM()
    {
        if (bgm == null || bgmLevel == null) return;
        if (bgm.isPlaying) return;

        bgm.clip = bgmLevel;
        bgm.loop = true;
        bgm.Play();
    }

    public void StopBGM()
    {
        if (bgm != null) bgm.Stop();
    }
    public void PlaySFX(AudioClip clip)
{
    if (bgm == null || clip == null) return;
    bgm.PlayOneShot(clip);
}
}