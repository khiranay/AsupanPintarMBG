using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    public int levelIndex;
    private const int TOTAL_LEVELS = 7;

    [Header("Star Objects")]
    public GameObject star1Unlocked;
    public GameObject star1Locked;
    public GameObject star2Unlocked;
    public GameObject star2Locked;
    public GameObject star3Unlocked;
    public GameObject star3Locked;

    [Header("Level State")]
    public GameObject levelUnlocked;
    public GameObject levelLocked;

    void Start()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        int stars = LevelProgressManager.GetStars(levelIndex);
        bool isUnlocked = IsLevelUnlocked();

        levelUnlocked.SetActive(isUnlocked);
        levelLocked.SetActive(!isUnlocked);

        if (!isUnlocked) return;

        star1Unlocked.SetActive(stars >= 1);
        star1Locked.SetActive(stars < 1);
        star2Unlocked.SetActive(stars >= 2);
        star2Locked.SetActive(stars < 2);
        star3Unlocked.SetActive(stars >= 3);
        star3Locked.SetActive(stars < 3);
    }

    public bool IsLevelUnlocked()
    {
        if (levelIndex == 1) return true;
        int prevStars = LevelProgressManager.GetStars(levelIndex - 1);
        return prevStars >= 1;
    }

    public void RefreshDisplay()
    {
        UpdateDisplay();
    }

    public void OnLevelClicked()
    {
        if (!IsLevelUnlocked()) return;

        // CEK apakah semua level sudah selesai
        bool allCompleted = true;
        for (int i = 1; i <= TOTAL_LEVELS; i++)
        {
            if (LevelProgressManager.GetStars(i) < 1)
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            LevelProgressManager.SetAllLevelsCompleted();
            Debug.Log("[LevelButton] Semua level selesai! Reward akan muncul di RouteMap.");
        }

        PlayerPrefs.SetInt("CurrentLevel", levelIndex);
        SceneLoader.LoadScene(LevelFlowManager.MateriScene);
    }
}