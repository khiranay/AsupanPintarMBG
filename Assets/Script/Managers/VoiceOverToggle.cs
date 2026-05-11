using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach script ini ke GameObject tombol Sound di setiap panel Materi.
///
/// CARA SETUP DI INSPECTOR:
/// 1. Pilih tombol sound (speaker icon) di Hierarchy
/// 2. Add Component → VoiceOverToggle
/// 3. Voice Over Clip  → drag AudioClip (.mp3/.wav) voice over panel ini
/// 4. Icon Sound On    → drag Sprite icon speaker aktif
/// 5. Icon Sound Off   → drag Sprite icon speaker dicoret / mute
/// 6. Button Icon      → drag komponen Image pada tombol
/// 7. (Opsional) Sound Button → drag Button jika tidak di-object yang sama
/// </summary>
public class VoiceOverToggle : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("AudioClip voice over untuk panel ini")]
    public AudioClip voiceOverClip;

    [Header("Icon Sprites")]
    [Tooltip("Icon saat siap diputar (speaker aktif)")]
    public Sprite iconSoundOn;

    [Tooltip("Icon saat sedang playing / klik lagi untuk mute (speaker dicoret)")]
    public Sprite iconSoundOff;

    [Header("References")]
    [Tooltip("Image komponen pada tombol untuk ganti icon")]
    public Image buttonIcon;

    [Tooltip("Button component (otomatis dicari jika kosong)")]
    public Button soundButton;

    // ─────────────────────────────────────────────────────────────────────
    private AudioSource audioSource;
    private bool isPlaying = false;

    // ── Unity Lifecycle ───────────────────────────────────────────────────

    void Awake()
    {
        // AudioSource — tambahkan otomatis kalau belum ada
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Button
        if (soundButton == null)
            soundButton = GetComponent<Button>();
        if (soundButton != null)
            soundButton.onClick.AddListener(OnSoundButtonClicked);

        // Icon Image — cari di children kalau belum di-assign
        if (buttonIcon == null)
            buttonIcon = GetComponentInChildren<Image>();
    }

    void OnEnable()
    {
        // Reset ke idle setiap kali panel ditampilkan
        ForceStop();
    }

    void OnDisable()
    {
        // Hentikan audio saat panel disembunyikan / ganti panel
        ForceStop();
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>Dipanggil VoiceOverManager untuk stop dari luar.</summary>
    public void StopExternal() => ForceStop();

    // ── Button Handler ────────────────────────────────────────────────────

    public void OnSoundButtonClicked()
    {
        if (isPlaying)
            ForceStop();
        else
            Play();
    }

    // ── Internal ──────────────────────────────────────────────────────────

    private void Play()
    {
        if (voiceOverClip == null)
        {
            Debug.LogWarning($"[VoiceOverToggle] Tidak ada AudioClip pada '{gameObject.name}'.");
            return;
        }

        // Beritahu manager supaya stop voice over lain yang sedang main
        VoiceOverManager.Instance?.RegisterPlay(this);

        audioSource.clip = voiceOverClip;
        audioSource.Play();
        isPlaying = true;

        // Ganti icon → mute (tekan lagi untuk stop)
        SetIcon(iconSoundOff);

        // Auto-reset setelah audio selesai
        CancelInvoke(nameof(OnAudioFinished));
        Invoke(nameof(OnAudioFinished), voiceOverClip.length);
    }

    private void ForceStop()
    {
        CancelInvoke(nameof(OnAudioFinished));
        audioSource?.Stop();
        isPlaying = false;
        SetIcon(iconSoundOn);
        VoiceOverManager.Instance?.UnregisterPlay(this);
    }

    private void OnAudioFinished()
    {
        // Audio habis natural → kembali ke icon sound on
        isPlaying = false;
        SetIcon(iconSoundOn);
        VoiceOverManager.Instance?.UnregisterPlay(this);
    }

    private void SetIcon(Sprite sprite)
    {
        if (buttonIcon != null && sprite != null)
            buttonIcon.sprite = sprite;
    }
}