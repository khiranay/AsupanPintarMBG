using UnityEngine;
using UnityEngine.SceneManagement;

public class MateriManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelMateri
    {
        public int levelIndex;
        public GameObject[] panels; // panel-panel untuk level ini
    }

    public LevelMateri[] allLevels; // semua level beserta panel-nya

    private int currentPanel = 0;
    private int currentLevel = 1;
    private GameObject[] activePanels;

    void Start()
    {
        // Ambil level yang sedang dimainkan
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        // Matikan semua group level
        foreach (var level in allLevels)
        {
            foreach (var panel in level.panels)
            {
                panel.SetActive(false);
            }
        }

        // Cari panel sesuai level
        foreach (var level in allLevels)
        {
            if (level.levelIndex == currentLevel)
            {
                activePanels = level.panels;
                break;
            }
        }

        // Tampilkan panel pertama
        currentPanel = 0;
        ShowPanel(0);
    }

    void ShowPanel(int index)
    {
        for (int i = 0; i < activePanels.Length; i++)
        {
            activePanels[i].SetActive(false);
        }
        activePanels[index].SetActive(true);
    }

    public void OnClickLanjut()
    {
        currentPanel++;

        if (currentPanel < activePanels.Length)
        {
            ShowPanel(currentPanel);
        }
        else
        {
            // Selesai → simpan bintang 1 → pindah ke Kuis
            LevelProgressManager.CompleteMateri(currentLevel);
            SceneManager.LoadScene("Kuis");
        }
    }
}