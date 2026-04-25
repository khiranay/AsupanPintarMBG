using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach ke GameObject KuisManager di Scene Kuis.
/// Kuis 1 soal per level: Benar = 100, Salah = 0.
/// Popup hasil memiliki 3 tombol: Next ke Game, Ulangi, Back to Home.
/// </summary>
public class KuisNavigator : MonoBehaviour
{
    [Header("Panel Kuis per Level (index 0 = Level 1, dst.)")]
    public GameObject[] kuisPanels;

    [Header("Popup yang muncul setelah kuis selesai")]
    public GameObject popupHasil;

    [Header("Overlay (background gelap di belakang popup)")]
    public GameObject overlay;

    [Header("Text nilai di popup (assign Text atau TMP_Text)")]
    public Text nilaiText;                  // Jika pakai Text biasa
    // public TMP_Text nilaiText;           // Uncomment jika pakai TextMeshPro, dan comment baris atas

    [Header("Nama Scene")]
    public string gameSceneName      = "SceneGame";
    public string routeMapSceneName  = "RouteMap";

    // Simpan nilai sementara
    private int nilaiSaatIni = 0;
    private int currentLevel = 1;

    void Start()
    {
        if (overlay != null)    overlay.SetActive(false);
        if (popupHasil != null) popupHasil.SetActive(false);

        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        ShowKuisForLevel(currentLevel);
    }

    private void ShowKuisForLevel(int level)
    {
        int index = level - 1;
        for (int i = 0; i < kuisPanels.Length; i++)
        {
            if (kuisPanels[i] != null)
                kuisPanels[i].SetActive(i == index);
        }
    }

    // ── Dipanggil tombol jawaban ──────────────────────────────────

    /// <summary>
    /// Hubungkan ke tombol jawaban BENAR.
    /// </summary>
    public void JawabBenar()
    {
        nilaiSaatIni = 100;
        TampilkanPopup();
    }

    /// <summary>
    /// Hubungkan ke tombol jawaban SALAH.
    /// </summary>
    public void JawabSalah()
    {
        nilaiSaatIni = 0;
        TampilkanPopup();
    }

    // ── Internal ──────────────────────────────────────────────────

    private void TampilkanPopup()
    {
        // Sembunyikan semua panel kuis
        foreach (var panel in kuisPanels)
            if (panel != null) panel.SetActive(false);

        // Simpan nilai ke PlayerPrefs
        PlayerPrefs.SetInt("NilaiKuis_Level" + currentLevel, nilaiSaatIni);
        PlayerPrefs.Save();

        // Update text nilai di popup
        if (nilaiText != null)
            nilaiText.text = "Nilai: " + nilaiSaatIni;

        // Tampilkan overlay + popup
        if (overlay != null)    overlay.SetActive(true);
        if (popupHasil != null) popupHasil.SetActive(true);
    }

    // ── Tombol di dalam Popup ─────────────────────────────────────

    /// <summary>
    /// Tombol "Next" di popup → pindah ke Scene Game.
    /// </summary>
    public void OnPopupNextToGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Tombol "Ulangi" di popup → sembunyikan popup, tampilkan kuis lagi dari awal.
    /// </summary>
    public void OnPopupUlangi()
    {
        // Tutup popup & overlay
        if (popupHasil != null) popupHasil.SetActive(false);
        if (overlay != null)    overlay.SetActive(false);

        // Reset nilai
        nilaiSaatIni = 0;

        // Tampilkan panel kuis lagi
        ShowKuisForLevel(currentLevel);
    }

    /// <summary>
    /// Tombol "Back to Home" di popup → kembali ke RouteMap.
    /// </summary>
    public void OnPopupBackToHome()
    {
        SceneManager.LoadScene(routeMapSceneName);
    }

    // ── Tombol Back di panel kuis (sebelum menjawab) ──────────────

    /// <summary>
    /// Tombol Back di panel kuis → kembali ke RouteMap.
    /// </summary>
    public void OnBackPressed()
    {
        SceneManager.LoadScene(routeMapSceneName);
    }
}