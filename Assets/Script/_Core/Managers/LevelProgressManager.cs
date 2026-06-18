using UnityEngine;

public class LevelProgressManager : MonoBehaviour
{
    private const int TOTAL_LEVELS = 7;

    // Panggil ini dari scene Materi saat selesai
    public static void CompleteMateri(int levelIndex)
    {
        int current = PlayerPrefs.GetInt("Level_" + levelIndex + "_Stars", 0);
        if (current < 1)
        {
            PlayerPrefs.SetInt("Level_" + levelIndex + "_Stars", 1);
            PlayerPrefs.Save();
        }
    }

    // Panggil ini dari scene Kuis saat selesai
    public static void CompleteKuis(int levelIndex)
    {
        int current = PlayerPrefs.GetInt("Level_" + levelIndex + "_Stars", 0);
        if (current < 2)
        {
            PlayerPrefs.SetInt("Level_" + levelIndex + "_Stars", 2);
            PlayerPrefs.Save();
        }
    }

    // Panggil ini dari scene Mini Game saat selesai
    public static void CompleteMiniGame(int levelIndex)
    {
        PlayerPrefs.SetInt("Level_" + levelIndex + "_Stars", 3);
        PlayerPrefs.Save();
    }

    public static int GetStars(int levelIndex)
    {
        return PlayerPrefs.GetInt("Level_" + levelIndex + "_Stars", 0);
    }

    /// <summary>
    /// Periksa apakah semua level sudah selesai (minimal 1 bintang)
    /// </summary>
    public static bool AreAllLevelsCompleted()
    {
        for (int i = 1; i <= TOTAL_LEVELS; i++)
        {
            int stars = GetStars(i);
            if (stars < 1)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Set flag semua level selesai (dipanggil dari berbagai tempat)
    /// </summary>
    public static void SetAllLevelsCompleted()
    {
        PlayerPrefs.SetInt("AllLevelsCompleted", 1);
        PlayerPrefs.Save();
        Debug.Log("[LevelProgressManager] Flag reward diset manual!");
    }

    /// <summary>
    /// Cek dan set flag semua level selesai jika semua level sudah complete
    /// </summary>
    public static void CheckAndSetAllLevelsCompleted()
    {
        if (AreAllLevelsCompleted())
        {
            PlayerPrefs.SetInt("AllLevelsCompleted", 1);
            PlayerPrefs.Save();
            Debug.Log("[LevelProgressManager] Semua level selesai! Flag reward diset.");
        }
        else
        {
            Debug.Log("[LevelProgressManager] Belum semua level selesai.");
        }
    }

    /// <summary>
    /// Periksa apakah flag semua level selesai aktif
    /// </summary>
    public static bool IsAllLevelsCompletedFlagSet()
    {
        return PlayerPrefs.GetInt("AllLevelsCompleted", 0) == 1;
    }

    /// <summary>
    /// Hapus flag (dipanggil setelah popup reward ditampilkan)
    /// </summary>
    public static void ClearAllLevelsCompletedFlag()
    {
        PlayerPrefs.SetInt("AllLevelsCompleted", 0);
        PlayerPrefs.Save();
        Debug.Log("[LevelProgressManager] Flag reward dihapus!");
    }
}