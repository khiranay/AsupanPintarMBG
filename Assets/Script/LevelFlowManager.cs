using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static manager untuk alur: Materi → Kuis → Game → RouteMap.
/// Tidak perlu di-attach ke GameObject manapun (static utility).
/// Bisa dipanggil dari scene Kuis maupun Game.
/// </summary>
public static class LevelFlowManager
{
    // ─── Nama scene (sesuaikan dengan nama scene di Build Settings) ───
    public const string RouteMapScene   = "RouteMap";
    public const string KuisScene       = "SceneKuis";
    public const string GameScene       = "SceneGame";
    public const string MateriScene     = "SceneMateri";   // scene yg berisi semua MateriLevel

    // PlayerPrefs key
    public const string KeyCurrentLevel = "CurrentLevel";

    // ─── Dipanggil dari tombol Next di scene Kuis ─────────────────────
    /// <summary>
    /// Kuis selesai → pindah ke Game untuk level yang sama.
    /// </summary>
    public static void OnKuisSelesai()
    {
        SceneManager.LoadScene(GameScene);
    }

    // ─── Dipanggil dari tombol Next / selesai di scene Game ──────────
    /// <summary>
    /// Game selesai → kembali ke RouteMap.
    /// </summary>
    public static void OnGameSelesai()
    {
        // Tandai level ini sudah selesai (opsional, untuk unlock di RouteMap)
        int level = PlayerPrefs.GetInt(KeyCurrentLevel, 1);
        int highestUnlocked = PlayerPrefs.GetInt("HighestUnlocked", 1);
        if (level >= highestUnlocked)
        {
            PlayerPrefs.SetInt("HighestUnlocked", level + 1);
            PlayerPrefs.Save();
        }

        GoToRouteMap();
    }

    // ─── Navigasi umum ────────────────────────────────────────────────
    public static void GoToRouteMap()
    {
        SceneManager.LoadScene(RouteMapScene);
    }

    // ─── Helper: baca level saat ini ─────────────────────────────────
    public static int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt(KeyCurrentLevel, 1);
    }
}