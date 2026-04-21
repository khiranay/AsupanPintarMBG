using UnityEngine;
using UnityEngine.SceneManagement;

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
    Debug.Log("Level " + levelIndex + " | IsUnlocked: " + IsLevelUnlocked());
    UpdateDisplay();
}

public void UpdateDisplay()
{
    int stars = LevelProgressManager.GetStars(levelIndex);
    bool isUnlocked = IsLevelUnlocked();

    Debug.Log("levelUnlocked object: " + levelUnlocked.name);
    Debug.Log("levelLocked object: " + levelLocked.name);

    levelUnlocked.SetActive(isUnlocked);
    levelLocked.SetActive(!isUnlocked);
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
    Debug.Log("Button diklik! Level: " + levelIndex);
    Debug.Log("IsUnlocked: " + IsLevelUnlocked());

    if (!IsLevelUnlocked()) return;

    Debug.Log("Loading scene: Materi_" + levelIndex);
    PlayerPrefs.SetInt("CurrentLevel", levelIndex);
    SceneManager.LoadScene("Materi_" + levelIndex);
}
}