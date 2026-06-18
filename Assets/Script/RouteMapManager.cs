using UnityEngine;
using UnityEngine.UI;

public class RouteMapManager : MonoBehaviour
{
    [Header("Popup Reward Semua Level")]
    public GameObject popupRewardAllLevels;

    [Header("Force Top Canvas (PATCH)")]
    public Canvas routeMapCanvas;

    void Start()
    {
        ForceCanvasToTop();
        RefreshAllLevelButtons();
        HandlePopups();
    }

    void HandlePopups()
    {
        // ← TAMBAHAN: Validasi ulang semua level
        if (LevelProgressManager.AreAllLevelsCompleted() && 
            !LevelProgressManager.IsAllLevelsCompletedFlagSet())
        {
            LevelProgressManager.SetAllLevelsCompleted();
            Debug.Log("[RouteMapManager] Flag reward diset ulang karena semua level sudah selesai!");
        }

        if (LevelProgressManager.IsAllLevelsCompletedFlagSet())
        {
            ShowPopupReward();
            return;
        }

        if (popupRewardAllLevels != null)
        {
            popupRewardAllLevels.SetActive(false);
        }
    }

    void ShowPopupReward()
    {
        if (popupRewardAllLevels != null)
        {
            popupRewardAllLevels.SetActive(true);
            Debug.Log("[RouteMapManager] Menampilkan popup reward semua level!");
            LevelProgressManager.ClearAllLevelsCompletedFlag();
        }
        else
        {
            Debug.LogWarning("[RouteMapManager] popupRewardAllLevels belum di-assign!");
        }
    }

    void ForceCanvasToTop()
    {
        if (routeMapCanvas == null)
            routeMapCanvas = GetComponentInChildren<Canvas>(true);

        if (routeMapCanvas != null)
        {
            routeMapCanvas.overrideSorting = true;
            routeMapCanvas.sortingOrder = 100;
            Debug.Log($"[RouteMapManager] Canvas '{routeMapCanvas.name}' paksa ke sortingOrder = 100");
        }
        else
        {
            Debug.LogWarning("[RouteMapManager] Tidak ada Canvas ditemukan.");
        }
    }

    void RefreshAllLevelButtons()
    {
        LevelButton[] allButtons = FindObjectsByType<LevelButton>(FindObjectsSortMode.None);
        foreach (var btn in allButtons)
        {
            btn.RefreshDisplay();
        }
        Debug.Log($"[RouteMapManager] Refreshed {allButtons.Length} LevelButtons");
    }

    public void TutupPopupReward()
    {
        if (popupRewardAllLevels != null)
            popupRewardAllLevels.SetActive(false);
    }
}