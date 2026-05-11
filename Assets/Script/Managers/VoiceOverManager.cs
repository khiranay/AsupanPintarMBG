using UnityEngine;

/// <summary>
/// (Opsional) Letakkan script ini di GameObject level/canvas.
/// Tugasnya: saat satu VoiceOver diputar, otomatis stop semua yang lain
/// supaya tidak tumpang tindih antar panel.
/// </summary>
public class VoiceOverManager : MonoBehaviour
{
    public static VoiceOverManager Instance { get; private set; }

    private VoiceOverToggle currentPlaying;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Dipanggil oleh VoiceOverToggle sebelum mulai main.
    /// </summary>
    public void RegisterPlay(VoiceOverToggle requester)
    {
        if (currentPlaying != null && currentPlaying != requester)
        {
            // Stop yang sedang main
            currentPlaying.StopExternal();
        }
        currentPlaying = requester;
    }

    public void UnregisterPlay(VoiceOverToggle requester)
    {
        if (currentPlaying == requester)
            currentPlaying = null;
    }
}