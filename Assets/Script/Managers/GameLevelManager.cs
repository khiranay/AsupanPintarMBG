using UnityEngine;

public class GameLevelManager : MonoBehaviour
{
    public GameObject[] gameLevelPanels;

    [Header("Popup Perintah")]
    public GameObject popupPerintah;    // popup instruksi
    public GameObject[] popupPerLevel; 
    // Jika setiap level punya popup berbeda:
    // Element 0 = popup perintah level 1
    // Element 1 = popup perintah level 2
    // dst...

    private int currentLevel;

    void Start()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        // Matikan semua panel game dulu
        foreach (var panel in gameLevelPanels)
        {
            panel.SetActive(false);
        }

        // Tampilkan popup perintah dulu
        TampilkanPopupPerintah();
    }

    void TampilkanPopupPerintah()
    {
        // Jika popup sama untuk semua level
        if (popupPerintah != null)
        {
            popupPerintah.SetActive(true);
        }

        // Jika popup beda tiap level
        for (int i = 0; i < popupPerLevel.Length; i++)
        {
            popupPerLevel[i].SetActive(false);
        }
        int index = currentLevel - 1;
        if (index >= 0 && index < popupPerLevel.Length)
        {
            popupPerLevel[index].SetActive(true);
        }
    }

    // Assign ke tombol "Mulai" di popup perintah
    public void OnTombolMulai()
    {
        // Tutup semua popup perintah
        if (popupPerintah != null)
            popupPerintah.SetActive(false);

        foreach (var popup in popupPerLevel)
        {
            popup.SetActive(false);
        }

        // Baru aktifkan panel game sesuai level
        int index = currentLevel - 1;
        if (index >= 0 && index < gameLevelPanels.Length)
        {
            gameLevelPanels[index].SetActive(true);
        }
    }
}