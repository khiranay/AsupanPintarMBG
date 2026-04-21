using UnityEngine;

public class LevelButton : MonoBehaviour
{
    public int levelIndex; // set di Inspector: 1, 2, 3, dst

    [Header("Star Objects")]
    public GameObject star1Unlocked;
    public GameObject star1Locked;
    public GameObject star2Unlocked;
    public GameObject star2Locked;
    public GameObject star3Unlocked;
    public GameObject star3Locked;

    [Header("Level State")]
    public GameObject levelUnlocked; // Level1_Unlocked
    public GameObject levelLocked;   // Level1 (yang ada lock)

    void Start()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        int stars = LevelProgressManager.GetStars(levelIndex);
        bool isUnlocked = IsLevelUnlocked();

        // Toggle panel unlocked/locked
        levelUnlocked.SetActive(isUnlocked);
        levelLocked.SetActive(!isUnlocked);

        if (!isUnlocked) return;

        // Update bintang
        star1Unlocked.SetActive(stars >= 1);
        star1Locked.SetActive(stars < 1);

        star2Unlocked.SetActive(stars >= 2);
        star2Locked.SetActive(stars < 2);

        star3Unlocked.SetActive(stars >= 3);
        star3Locked.SetActive(stars < 3);
    }

    bool IsLevelUnlocked()
    {
        if (levelIndex == 1) return true; // Level 1 selalu terbuka
        // Level N terbuka jika level sebelumnya punya minimal 1 bintang
        return LevelProgressManager.GetStars(levelIndex - 1) >= 1;
    }

    // Assign ke tombol OnClick
    public void OnLevelClicked()
    {
        if (!IsLevelUnlocked()) return;

        int stars = LevelProgressManager.GetStars(levelIndex);

        // Tentukan scene mana yang dibuka
        if (stars == 0)
        {
            // Langsung ke Materi
            SceneLoader.LoadMateri(levelIndex);
        }
        else
        {
            // Tampilkan popup pilihan (ulang dari materi/kuis/game)
            LevelSelectPopup.Show(levelIndex, stars);
        }
    }
}