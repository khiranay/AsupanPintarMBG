using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach script ini ke setiap Button yang ingin punya sound effect.
/// 
/// CARA SETUP:
/// 1. Pilih GameObject Button di Hierarchy
/// 2. Add Component → ButtonSFX
/// 3. Isi Click Sound → drag AudioClip SFX klik
/// 4. (Opsional) Isi Audio Source → jika ingin pakai AudioSource tertentu
///    Jika kosong, script otomatis buat AudioSource sendiri
/// </summary>
public class ButtonSFX : MonoBehaviour
{
    [Header("Sound")]
    [Tooltip("AudioClip yang diputar saat tombol diklik")]
    public AudioClip clickSound;

    [Tooltip("Volume suara (0 = mute, 1 = full)")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Reference (Opsional)")]
    [Tooltip("Kosongkan untuk auto-create AudioSource")]
    public AudioSource audioSource;

    void Awake()
    {
        // Auto-create AudioSource jika belum diassign
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Daftarkan ke event onClick Button
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(PlayClickSound);
        else
            Debug.LogWarning($"[ButtonSFX] Tidak ada komponen Button pada '{gameObject.name}'");
    }

    public void PlayClickSound()
    {
        if (clickSound == null)
        {
            Debug.LogWarning($"[ButtonSFX] Click Sound belum diisi pada '{gameObject.name}'");
            return;
        }

        audioSource.PlayOneShot(clickSound, volume);
    }
}