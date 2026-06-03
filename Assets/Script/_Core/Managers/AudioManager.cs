using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource bgm;

    public GameObject soundOnIcon;
    public GameObject soundOffIcon;

    void Start()
    {
        // bgm wajib diisi — error jika kosong
        if (bgm == null)
            Debug.LogError("[AudioManager] AudioSource 'bgm' belum di-assign di Inspector!");

        // soundOnIcon & soundOffIcon opsional — hanya warning jika kosong
        if (soundOnIcon == null || soundOffIcon == null)
            Debug.LogWarning("[AudioManager] soundOnIcon / soundOffIcon belum di-assign. " +
                             "Tombol toggle sound tidak akan berubah tampilan, tapi tidak crash.");
    }

    public void ToggleSound()
    {
        // BUG FIX #9: Null-check agar tidak crash jika lupa assign di Inspector
        if (bgm == null) return;

        bool isMuted = bgm.mute;
        bgm.mute = !isMuted;

        if (soundOnIcon != null)  soundOnIcon.SetActive(isMuted);
        if (soundOffIcon != null) soundOffIcon.SetActive(!isMuted);
    }
}
