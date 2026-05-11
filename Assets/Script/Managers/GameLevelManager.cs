using UnityEngine;

public class GameLevelManager : MonoBehaviour
{
    public GameObject[] gameLevelPanels;

    [Header("Popup Perintah")]
    public GameObject popupPerintah;
    public GameObject[] popupPerLevel;

    [Header("Game Manager Per Level")]
    public WhackAMoleManager[] whackAMoleManagers;

    private int currentLevel;

    void Start()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        Debug.Log("CurrentLevel: " + currentLevel);

        foreach (var panel in gameLevelPanels)
        {
            panel.SetActive(false);
        }

        Debug.Log("Memanggil TampilkanPopupPerintah");
        TampilkanPopupPerintah();
    }

    void TampilkanPopupPerintah()
    {
        for (int i = 0; i < popupPerLevel.Length; i++)
        {
            popupPerLevel[i].SetActive(false);
        }

        int index = currentLevel - 1;

        if (index >= 0 && index < popupPerLevel.Length)
        {
            // Aktifkan parent (GameLevel) dulu
            gameLevelPanels[index].SetActive(true);

            // Baru aktifkan popup
            popupPerLevel[index].SetActive(true);
        }
    }

    // Assign ke tombol X di popup perintah
    public void OnTombolTutupPopup()
    {
        // Tutup popup
        foreach (var popup in popupPerLevel)
        {
            popup.SetActive(false);
        }

        int index = currentLevel - 1;

        // Panggil MulaiGame() jika level ini pakai WhackAMole
        if (index >= 0 && index < whackAMoleManagers.Length
            && whackAMoleManagers[index] != null)
        {
            whackAMoleManagers[index].MulaiGame();
        }
    }
}