using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DragDropManager : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject[] foodPrefabs;
    public Transform spawnPoint;
    public Transform conveyorEnd;
    public float conveyorSpeed = 1f;
    public float spawnInterval = 3f;

    [Header("Score")]
    public TextMeshProUGUI scoreText;
    public int score = 0;
    public int totalFood = 10;
    private int foodProcessed = 0;

    [Header("Popup Perintah (Instruksi)")]
    [Tooltip("Assign popup instruksi 'Match It' di sini")]
    public GameObject popupPerintah;

    [Header("Popup Hasil")]
    public GameObject popup;
    public TextMeshProUGUI popupScoreText;
    public TextMeshProUGUI popupBenarText;
    public TextMeshProUGUI popupSalahText;

    private int jumlahBenar = 0;
    private int jumlahSalah = 0;
    private List<GameObject> activeFoods = new List<GameObject>();

    // ── Flag: game belum mulai sampai popup perintah ditutup ──
    private bool gameStarted = false;

    // ─────────────────────────────────────────────────────────

    private void Start()
    {
        score = 0;
        foodProcessed = 0;
        jumlahBenar = 0;
        jumlahSalah = 0;

        if (scoreText != null)
            scoreText.text = "0";

        // Tampilkan popup perintah, game BELUM mulai
        if (popupPerintah != null)
        {
            popupPerintah.SetActive(true);
            gameStarted = false;
        }
        else
        {
            // Tidak ada popup → langsung mulai
            StartGame();
        }
    }

    void Update()
    {
        // Conveyor hanya jalan jika game sudah dimulai
        if (!gameStarted) return;

        for (int i = activeFoods.Count - 1; i >= 0; i--)
        {
            GameObject food = activeFoods[i];
            if (food == null)
            {
                activeFoods.RemoveAt(i);
                continue;
            }

            FoodItem fi = food.GetComponent<FoodItem>();
            if (fi != null && !fi.isDragging)
            {
                food.transform.position += Vector3.left * conveyorSpeed * Time.deltaTime;

                if (food.transform.position.x < conveyorEnd.position.x)
                {
                    jumlahSalah++;
                    foodProcessed++;
                    activeFoods.RemoveAt(i);
                    Destroy(food);
                    CheckGameEnd();
                }
            }
        }
    }

    /// <summary>
    /// Dipanggil oleh tombol "Tutup / Mulai" di popup perintah.
    /// Assign ke OnClick() tombol X / Mulai di popup instruksi.
    /// </summary>
    public void OnTutupPopupPerintah()
    {
        if (popupPerintah != null)
            popupPerintah.SetActive(false);

        StartGame();
    }

    private void StartGame()
    {
        gameStarted = true;
        StartCoroutine(SpawnFood());
    }

    // ─────────────────────────────────────────────────────────

    IEnumerator SpawnFood()
    {
        int spawned = 0;
        while (spawned < totalFood)
        {
            SpawnRandomFood();
            spawned++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRandomFood()
    {
        int index = Random.Range(0, foodPrefabs.Length);
        GameObject food = Instantiate(foodPrefabs[index], spawnPoint.position, Quaternion.identity, spawnPoint.parent);
        activeFoods.Add(food);
        Debug.Log("Spawned: " + food.name + " pos: " + food.transform.position);
    }

    public void OnFoodDropped(FoodItem food, bool isBenar, Transform area)
    {
        if (isBenar)
        {
            score += 10;
            jumlahBenar++;
            if (scoreText != null)
                scoreText.text = score.ToString();
        }
        else
        {
            jumlahSalah++;
        }

        foodProcessed++;
        activeFoods.Remove(food.gameObject);
        food.transform.SetParent(area);
        food.GetComponent<CanvasGroup>().blocksRaycasts = false;
        Destroy(food.gameObject, 0.3f);

        CheckGameEnd();
    }

    void CheckGameEnd()
    {
        if (foodProcessed >= totalFood)
            StartCoroutine(ShowResult());
    }

    IEnumerator ShowResult()
    {
        yield return new WaitForSeconds(0.5f);

        int maxScore = totalFood * 10;
        if (popupScoreText != null)
            popupScoreText.text = score + "/" + maxScore;
        if (popupBenarText != null)
            popupBenarText.text = jumlahBenar.ToString();
        if (popupSalahText != null)
            popupSalahText.text = jumlahSalah.ToString();

        int level = LevelFlowManager.GetCurrentLevel();
        LevelProgressManager.CompleteMiniGame(level);

        popup.SetActive(true);
    }

    public void OnTombolSelesai()  => LevelFlowManager.OnGameSelesai();
    public void OnTombolKembali()  => LevelFlowManager.GoToRouteMap();
    public void OnTombolUlang()
    {
        SceneLoader.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
