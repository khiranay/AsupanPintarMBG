using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script reusable untuk tombol-tombol di PopupScore semua level game.
/// Attach ke GameObject PopupScore, lalu hubungkan tombol ke method ini.
///
/// - Ulangi  : reload scene Game (mulai ulang level ini)
/// - Home    : ke RouteMap
/// - Next    : simpan bintang 3 → ke Materi level berikutnya
///             (jika sudah level terakhir → ke RouteMap)
/// </summary>
public class PopupScoreButtons : MonoBehaviour
{
    [Header("Jumlah total level dalam game (default 7)")]
    public int totalLevel = 7;

    /// <summary>
    /// Tombol ULANGI — mulai ulang level game yang sama.
    /// </summary>
    public void OnUlangi()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Tombol HOME — kembali ke peta level (RouteMap).
    /// </summary>
    public void OnHome()
    {
        Time.timeScale = 1f;
        LevelFlowManager.GoToRouteMap();
    }

    /// <summary>
    /// Tombol NEXT — simpan progress, lanjut ke Materi level berikutnya.
    /// Jika sudah level terakhir, kembali ke RouteMap.
    /// </summary>
    public void OnNext()
    {
        Time.timeScale = 1f;
        LevelFlowManager.GoToNextLevelMateri(totalLevel);
    }
}
