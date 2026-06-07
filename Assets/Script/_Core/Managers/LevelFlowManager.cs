using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static manager untuk alur: Materi → Kuis → Game → RouteMap.
/// Tidak perlu di-attach ke GameObject manapun (static utility).
/// Bisa dipanggil dari scene Kuis maupun Game.
/// Semua perpindahan scene dilakukan via SceneLoader (async)
/// agar tidak ada freeze saat loading.
/// </summary>
public static class LevelFlowManager
{
    // ─── Nama scene (sesuaikan dengan nama scene di Build Settings) ───
    public const string RouteMapScene   = "RouteMap";
    public const string KuisScene       = "Kuis";
    public const string GameScene       = "Game";
    public const string MateriScene     = "Materi";   // scene yg berisi semua MateriLevel

    // PlayerPrefs key
    public const string KeyCurrentLevel = "CurrentLevel";

    // ─── Dipanggil dari tombol Next di scene Kuis ─────────────────────
    /// <summary>
    /// Kuis selesai → pindah ke Game untuk level yang sama.
    /// </summary>
    public static void OnKuisSelesai()
    {
        SceneLoader.LoadScene(GameScene);
    }

    // ─── Dipanggil dari tombol Next / selesai di scene Game ──────────
    /// <summary>
    /// Game selesai → simpan bintang 3 → kembali ke RouteMap.
    /// Ini adalah satu-satunya tempat yang bertanggung jawab menyimpan
    /// bintang 3. Tiap game manager TIDAK perlu memanggil CompleteMiniGame
    /// sendiri karena OnGameSelesai sudah menanganinya di sini.
    /// </summary>
    public static void OnGameSelesai()
    {
        int level = PlayerPrefs.GetInt(KeyCurrentLevel, 1);

        // ROOT CAUSE FIX: Simpan bintang 3 di sini.
        // Sebelumnya CompleteMiniGame hanya dipanggil di dalam masing-masing
        // game manager (GameOver), tapi jika tombol Selesai di-wire langsung
        // ke OnGameSelesai tanpa melewati game manager, bintang tidak pernah
        // tersimpan dan tetap stuck di 2 (dari Kuis).
        LevelProgressManager.CompleteMiniGame(level);

        // Unlock level berikutnya di RouteMap
        int highestUnlocked = PlayerPrefs.GetInt("HighestUnlocked", 1);
        if (level >= highestUnlocked)
        {
            PlayerPrefs.SetInt("HighestUnlocked", level + 1);
            PlayerPrefs.Save();
        }
        if (level == 7)
        {
        PlayerPrefs.SetInt("ShowSelesaiPopup", 1);
        PlayerPrefs.Save();
        }   
        GoToRouteMap();
    }

    // ─── Dipanggil tombol Next di popup Game → Materi level berikutnya ─
    /// <summary>
    /// Game selesai → simpan bintang 3 → unlock next → ke Materi level+1.
    /// </summary>
    public static void GoToNextLevelMateri(int totalLevel = 7)
    {
        int level = PlayerPrefs.GetInt(KeyCurrentLevel, 1);

        // Simpan bintang 3 & unlock level berikutnya
        LevelProgressManager.CompleteMiniGame(level);
        int highestUnlocked = PlayerPrefs.GetInt("HighestUnlocked", 1);
        if (level >= highestUnlocked)
        {
            PlayerPrefs.SetInt("HighestUnlocked", level + 1);
            PlayerPrefs.Save();
        }

        if (level >= totalLevel)
        {
            // Sudah level terakhir → kembali ke RouteMap
            GoToRouteMap();
            return;
        }

        // Set level berikutnya lalu muat Materi
        PlayerPrefs.SetInt(KeyCurrentLevel, level + 1);
        PlayerPrefs.Save();
        SceneLoader.LoadScene(MateriScene);
    }

    // ─── Navigasi umum ────────────────────────────────────────────────
    public static void GoToRouteMap()
    {
        SceneLoader.LoadScene(RouteMapScene);
    }

    // ─── Helper: baca level saat ini ─────────────────────────────────
    public static int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt(KeyCurrentLevel, 1);
    }
}
