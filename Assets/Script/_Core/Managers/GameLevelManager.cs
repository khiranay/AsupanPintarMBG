using UnityEngine;

public class GameLevelManager : MonoBehaviour
{
    [Header("Panel Game per Level (index 0 = Level 1, dst.)")]
    public GameObject[] gameLevelPanels;

    [Header("Popup Perintah per Level (index 0 = Level 1, dst.)")]
    public GameObject[] popupPerLevel;

    /// <summary>
    /// Game Manager per level — drag komponen game manager (WhackAMoleManager,
    /// DragDropManager, XRayGameManager, SnakeGameManager, dst.) ke slot yang sesuai.
    /// Index 0 = Level 1, index 1 = Level 2, dst.
    /// Semua game manager harus mengimplementasi interface IGameManager.
    /// </summary>
    [Header("Game Manager per Level (implementasi IGameManager)")]
    public MonoBehaviour[] gameManagers;

    private int currentLevel;

    void Start()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        foreach (var panel in gameLevelPanels)
        {
            if (panel != null) panel.SetActive(false);
        }

        TampilkanPopupPerintah();
    }

    void TampilkanPopupPerintah()
    {
        for (int i = 0; i < popupPerLevel.Length; i++)
        {
            if (popupPerLevel[i] != null)
                popupPerLevel[i].SetActive(false);
        }

        int index = currentLevel - 1;

        if (index >= 0 && index < gameLevelPanels.Length && gameLevelPanels[index] != null)
            gameLevelPanels[index].SetActive(true);

        if (index >= 0 && index < popupPerLevel.Length && popupPerLevel[index] != null)
            popupPerLevel[index].SetActive(true);
    }

    /// <summary>
    /// Assign ke tombol "Mulai / X" di popup perintah setiap level.
    /// Menutup popup lalu memanggil MulaiGame() pada game manager level ini.
    /// </summary>
    public void OnTombolTutupPopup()
    {
        Debug.Log($"[GameLevelManager] OnTombolTutupPopup dipanggil. currentLevel={currentLevel}");

        // Tutup semua popup perintah
        foreach (var popup in popupPerLevel)
        {
            if (popup != null) popup.SetActive(false);
        }

        int index = currentLevel - 1;

        Debug.Log($"[GameLevelManager] gameManagers.Length={gameManagers.Length}, index={index}");

        // Panggil MulaiGame() lewat interface IGameManager
        if (index >= 0 && index < gameManagers.Length && gameManagers[index] != null)
        {
            Debug.Log($"[GameLevelManager] Memanggil MulaiGame() pada {gameManagers[index].GetType().Name}");
            if (gameManagers[index] is IGameManager mgr)
            {
                mgr.MulaiGame();
            }
            else
            {
                Debug.LogError(
                    $"[GameLevelManager] gameManagers[{index}] ({gameManagers[index].GetType().Name}) " +
                    "tidak mengimplementasi IGameManager!");
            }
        }
        else
        {
            Debug.LogError(
                $"[GameLevelManager] gameManagers[{index}] kosong atau tidak ada! " +
                $"Pastikan slot index {index} sudah di-assign di Inspector.");
        }
    }
}
