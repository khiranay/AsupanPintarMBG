using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WhackAMoleManager : MonoBehaviour, IGameManager
{
    [Header("Moles")]
    public Mole[] holes;

    [Header("Setting Game")]
    public float gameDuration = 15f;
    public float moleVisibleTime = 1.2f;
    public float spawnInterval = 0.8f;

    [Header("Weighted Spawn")]
    public float[] spawnWeights = { 40f, 30f, 20f, 10f };

    // BUG FIX #8: Array pre-alokasi untuk hindari GC di SpawnMoles()
    private static readonly float[] WeightsFase1 = { 40f, 30f, 20f, 10f };
    private static readonly float[] WeightsFase2 = { 25f, 25f, 25f, 25f };

    [Header("UI")]
    public TextMeshProUGUI teksSkor;
    public TextMeshProUGUI teksWaktu;
    public TextMeshProUGUI teksCountdown; // ← tambahkan TextMeshPro countdown
    public GameObject popupHasil;
    public TextMeshProUGUI teksHasilSkor;

    private int score = 0;
    private float timeLeft;
    private bool isPlaying = false;

    void Start()
    {
        timeLeft = gameDuration;

        foreach (var hole in holes)
        {
            hole.Init(this);
        }

        // Tidak langsung mulai, tunggu dipanggil dari GameLevelManager
    }

    // Dipanggil dari GameLevelManager.OnTombolMulai()
    public void MulaiGame()
    {
        StartCoroutine(CountdownHelper.Hitung(teksCountdown, () =>
        {
            isPlaying = true;
            StartCoroutine(SpawnMoles());
            StartCoroutine(Countdown());
        }));
    }

    IEnumerator SpawnMoles()
    {
        while (isPlaying)
        {
            float timeRatio = timeLeft / gameDuration;

            // BUG FIX #8: Pakai array yang sudah pre-alokasi, bukan new float[] setiap frame
            if (timeRatio > 0.5f)
            {
                spawnWeights  = WeightsFase1;
                spawnInterval = 0.8f;
                moleVisibleTime = 1.2f;
            }
            else if (timeRatio > 0.25f)
            {
                spawnWeights  = WeightsFase2;
                spawnInterval = 0.6f;
                moleVisibleTime = 1.0f;
            }
            else
            {
                spawnWeights  = WeightsFase2;
                spawnInterval = 0.4f;
                moleVisibleTime = 0.8f;
            }

            int index = GetWeightedRandomIndex();
            StartCoroutine(holes[index].ShowMole(moleVisibleTime));

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    int GetWeightedRandomIndex()
    {
        float total = 0f;
        foreach (float w in spawnWeights) total += w;

        float random = Random.Range(0f, total);
        float cumulative = 0f;

        for (int i = 0; i < spawnWeights.Length; i++)
        {
            cumulative += spawnWeights[i];
            if (random <= cumulative) return i;
        }

        return spawnWeights.Length - 1;
    }

    IEnumerator Countdown()
    {
        while (timeLeft > 0)
        {
            teksWaktu.text = FormatWaktu(timeLeft);
            yield return new WaitForSeconds(0.1f);
            timeLeft -= 0.1f;
        }

        teksWaktu.text = "00:00";
        GameOver();
    }

    string FormatWaktu(float time)
    {
        int detik = Mathf.CeilToInt(time);
        int menit = detik / 60;
        int sisaDetik = detik % 60;
        return string.Format("{0:00}:{1:00}", menit, sisaDetik);
    }

    public void AddScore(int nilai)
    {
        score += nilai;
        score = Mathf.Min(score, 100);
        teksSkor.text = score.ToString("D4");
    }

    void GameOver()
    {
        isPlaying = false;

        foreach (var hole in holes)
        {
            hole.HideMole();
        }

        teksHasilSkor.text = score.ToString();
        popupHasil.SetActive(true);

        int level = PlayerPrefs.GetInt("CurrentLevel", 1);
        LevelProgressManager.CompleteMiniGame(level);
    }

    public void OnTombolSelesai()
    {
        LevelFlowManager.OnGameSelesai();
    }

    public void OnTombolUlang()
    {
        SceneLoader.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
