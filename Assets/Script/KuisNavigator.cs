using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    [Header("=== DATA KUIS SEMUA LEVEL ===")]
    public List<QuizData> semuaKuis;

    [Header("=== PANEL SOAL ===")]
    public GameObject panelSoal;
    public TextMeshProUGUI teksLevelLabel;
    public TextMeshProUGUI teksPertanyaan;
    public Button[] tombolPilihan;
    public TextMeshProUGUI[] teksPilihan;

    [Header("=== PANEL BENAR ===")]
    public GameObject panelBenar;
    public TextMeshProUGUI teksPenjelasanBenar;
    public Image gambarJawabanBenar;      // ← Image jawaban benar
    public Image ikonPanelBenar;
    public Button tombolLanjutBenar;

    [Header("=== PANEL SALAH ===")]
    public GameObject panelSalah;
    public TextMeshProUGUI teksPenjelasanSalah;
    public Image gambarJawabanSalah;      // ← Image jawaban salah
    public Image ikonPanelSalah;
    public Button tombolCobaLagi;
    public Button tombolLanjutSalah;

    [Header("=== FEEDBACK WARNA ===")]
    public Color warnaDefault;
    public Color warnaBenar;
    public Color warnaSalah;

    private int currentLevel = 0;
    private QuizData currentQuiz;
    private bool sudahMenjawab = false;
    private int pilihanPemain = -1;

    void Start()
    {
        panelSoal.SetActive(false);
        panelBenar.SetActive(false);
        panelSalah.SetActive(false);

        if (tombolLanjutBenar != null)
            tombolLanjutBenar.onClick.AddListener(OnTombolLanjut);

        if (tombolCobaLagi != null)
            tombolCobaLagi.onClick.AddListener(OnTombolCobaLagi);

        if (tombolLanjutSalah != null)
            tombolLanjutSalah.onClick.AddListener(OnTombolLanjut);

        for (int i = 0; i < tombolPilihan.Length; i++)
        {
            int index = i;
            tombolPilihan[i].onClick.AddListener(() => OnPilihanDiklik(index));
        }
        MulaiKuis(1);
    }

    public void MulaiKuis(int levelNumber)
    {
        currentQuiz = semuaKuis.Find(q => q.levelId == levelNumber);

        if (currentQuiz == null)
        {
            Debug.LogError($"[QuizManager] Data kuis Level {levelNumber} tidak ditemukan!");
            return;
        }

        currentLevel = levelNumber;
        sudahMenjawab = false;
        pilihanPemain = -1;

        TampilkanSoal();
    }

    void TampilkanSoal()
    {
        panelBenar.SetActive(false);
        panelSalah.SetActive(false);
        panelSoal.SetActive(true);

        if (teksLevelLabel != null)
            teksLevelLabel.text = $"{currentLevel}";

        if (teksPertanyaan != null)
            teksPertanyaan.text = currentQuiz.pertanyaan;

        string[] labelHuruf = { "A", "B", "C", "D" };
        for (int i = 0; i < tombolPilihan.Length; i++)
        {
            SetWarnaTombol(i, warnaDefault);
            tombolPilihan[i].interactable = true;

            if (i < currentQuiz.pilihanJawaban.Length)
            {
                if (teksPilihan[i] != null)
                    teksPilihan[i].text = $"{labelHuruf[i]}. {currentQuiz.pilihanJawaban[i]}";
                tombolPilihan[i].gameObject.SetActive(true);
                teksPilihan[i].color = Color.black;
            }
            else
            {
                tombolPilihan[i].gameObject.SetActive(false);
            }
        }
    }

    void OnPilihanDiklik(int indexDipilih)
    {
        if (sudahMenjawab) return;

        sudahMenjawab = true;
        pilihanPemain = indexDipilih;

        foreach (var tombol in tombolPilihan)
            tombol.interactable = false;

        bool isBenar = (indexDipilih == currentQuiz.indexJawabanBenar);

        SetWarnaTombol(indexDipilih, isBenar ? warnaBenar : warnaSalah);

        if (!isBenar)
            SetWarnaTombol(currentQuiz.indexJawabanBenar, warnaBenar);

        StartCoroutine(TampilkanPanelHasil(isBenar, 0.8f));
    }

    IEnumerator TampilkanPanelHasil(bool isBenar, float delay)
    {
        yield return new WaitForSeconds(delay);
        panelSoal.SetActive(false);

        if (isBenar) TampilkanPanelBenar();
        else TampilkanPanelSalah();
    }

    void TampilkanPanelBenar()
    {
        panelBenar.SetActive(true);

        // Isi teks penjelasan
        if (teksPenjelasanBenar != null)
            teksPenjelasanBenar.text = currentQuiz.penjelasanBenar;

        // Tampilkan gambar jawaban benar
        if (gambarJawabanBenar != null && currentQuiz.ikonBenar != null)
        {
            gambarJawabanBenar.sprite = currentQuiz.ikonBenar;
            gambarJawabanBenar.gameObject.SetActive(true);
        }

        // Tampilkan ikon dekorasi (opsional)
        if (ikonPanelBenar != null && currentQuiz.ikonBenar != null)
        {
            ikonPanelBenar.sprite = currentQuiz.ikonBenar;
            ikonPanelBenar.gameObject.SetActive(true);
        }
    }

    void TampilkanPanelSalah()
    {
        panelSalah.SetActive(true);

        // Isi teks penjelasan
        if (teksPenjelasanSalah != null)
            teksPenjelasanSalah.text = currentQuiz.penjelasanSalah;

        // Tampilkan gambar jawaban salah
        if (gambarJawabanSalah != null && currentQuiz.ikonSalah != null)
        {
            gambarJawabanSalah.sprite = currentQuiz.ikonSalah;
            gambarJawabanSalah.gameObject.SetActive(true);
        }

        // Tampilkan ikon dekorasi (opsional)
        if (ikonPanelSalah != null && currentQuiz.ikonSalah != null)
        {
            ikonPanelSalah.sprite = currentQuiz.ikonSalah;
            ikonPanelSalah.gameObject.SetActive(true);
        }
    }

    void OnTombolLanjut()
{
    panelBenar.SetActive(false);
    panelSalah.SetActive(false);

    LevelProgressManager.CompleteKuis(currentLevel);
    LevelFlowManager.OnKuisSelesai();
    string namaScene = $"Game_Level{currentLevel}";
    Debug.Log($"[QuizManager] Masuk ke scene {namaScene}");
    SceneManager.LoadScene(namaScene);
}

    void OnTombolCobaLagi()
    {
        panelSalah.SetActive(false);
        sudahMenjawab = false;
        TampilkanSoal();
    }
    

    void SetWarnaTombol(int index, Color warna)
    {
        if (index < 0 || index >= tombolPilihan.Length) return;
        Image bg = tombolPilihan[index].GetComponent<Image>();
        if (bg != null) bg.color = warna;
    }
}