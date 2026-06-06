using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DragDropManager : MonoBehaviour, IGameManager
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

    [Header("Timer")]
    [Tooltip("Durasi game dalam detik")]
    public float durasiTimer = 60f;
    [Tooltip("TextMeshProUGUI untuk tampilan waktu (format MM:SS)")]
    public TextMeshProUGUI teksWaktu;
    private float timeLeft;

    [Header("Floating Score Text")]
    [Tooltip("Prefab TextMeshProUGUI untuk efek skor melayang (+10 / -5)")]
    public GameObject floatingTextPrefab;
    [Tooltip("Parent transform untuk floating text (drag GameObject Canvas/Panel)")]
    public Transform uiParent;

    [Header("Poin")]
    public int poinBenar = 10;
    public int poinSalah = -5;

    [Header("Popup Perintah (Instruksi)")]
    public GameObject popupPerintah;

    [Header("Countdown")]
    public TextMeshProUGUI teksCountdown;

    [Header("Popup Hasil")]
    public GameObject popup;
    public TextMeshProUGUI popupScoreText;
    public TextMeshProUGUI popupBenarText;
    public TextMeshProUGUI popupSalahText;

    private int jumlahBenar = 0;
    private int jumlahSalah = 0;
    private List<GameObject> activeFoods = new List<GameObject>();
    private bool gameStarted = false;
    private bool gameOver = false;

    // ─────────────────────────────────────────────────────────

    private void Start()
    {
        score = 0;
        foodProcessed = 0;
        jumlahBenar = 0;
        jumlahSalah = 0;
        timeLeft = durasiTimer;
        gameStarted = false;
        gameOver = false;

        if (scoreText != null) scoreText.text = "0";
        UpdateTimerUI();
    }

    void Update()
    {
        if (!gameStarted || gameOver) return;

        // ── Timer countdown ──────────────────────────────────
        timeLeft -= Time.deltaTime;
        UpdateTimerUI();
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            UpdateTimerUI();
            StartCoroutine(ShowResult());
            return;
        }

        // ── Conveyor movement ────────────────────────────────
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
                    // Kembalikan ke spawn point agar bisa di-sort lagi
                    food.transform.position = spawnPoint.position;
                }
            }
        }
    }

    // ─────────────────────────────────────────────────────────

    public void MulaiGame() => OnTutupPopupPerintah();

    public void OnTutupPopupPerintah()
    {
        if (popupPerintah != null) popupPerintah.SetActive(false);
        StartCoroutine(CountdownHelper.Hitung(teksCountdown, StartGame));
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
            if (gameOver) yield break;
            SpawnRandomFood();
            spawned++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRandomFood()
    {
        int index = Random.Range(0, foodPrefabs.Length);
        GameObject food = Instantiate(foodPrefabs[index], spawnPoint.position,
                                      Quaternion.identity, spawnPoint.parent);
        activeFoods.Add(food);
    }

    // ─────────────────────────────────────────────────────────

    public void OnFoodDropped(FoodItem food, bool isBenar, Transform area)
    {
        if (gameOver) return;

        if (isBenar)
        {
            score += poinBenar;
            jumlahBenar++;
            if (scoreText != null) scoreText.text = score.ToString();
            MunculkanFloatingText("+" + poinBenar, Color.green, food.transform.position);
        }
        else
        {
            jumlahSalah++;
            MunculkanFloatingText(poinSalah.ToString(), Color.red, food.transform.position);
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
        if (gameOver) return;
        if (foodProcessed >= totalFood)
            StartCoroutine(ShowResult());
    }

    // ─────────────────────────────────────────────────────────

    void UpdateTimerUI()
    {
        if (teksWaktu == null) return;
        int detik = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
        teksWaktu.text = string.Format("{0:00}:{1:00}", detik / 60, detik % 60);
    }

    // ─────────────────────────────────────────────────────────

    void MunculkanFloatingText(string teks, Color warna, Vector3 posisi)
    {
        if (floatingTextPrefab == null) return;

        Transform parent = uiParent != null ? uiParent : transform;
        GameObject obj = Instantiate(floatingTextPrefab, parent);
        obj.transform.position = posisi;

        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = teks;
            tmp.color = warna;
        }

        StartCoroutine(AnimasiFloatingText(obj));
    }

    IEnumerator AnimasiFloatingText(GameObject obj)
    {
        float durasi = 1f;
        float timer = 0f;
        Vector3 startPos = obj.transform.position;
        Vector3 endPos   = startPos + Vector3.up * 80f;

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        while (timer < durasi)
        {
            if (obj == null) yield break;
            timer += Time.deltaTime;
            float t = timer / durasi;
            obj.transform.position = Vector3.Lerp(startPos, endPos, t);
            cg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        if (obj != null) Destroy(obj);
    }

    // ─────────────────────────────────────────────────────────

    IEnumerator ShowResult()
    {
        if (gameOver) yield break;
        gameOver = true;
        gameStarted = false;

        // Hancurkan semua food yang masih aktif
        foreach (var f in activeFoods)
            if (f != null) Destroy(f);
        activeFoods.Clear();

        yield return new WaitForSeconds(0.5f);

        int maxScore = totalFood * poinBenar;
        // Skor akhir = (benar × poinBenar) - (salah × |poinSalah|), min 0
        int skorAkhir = Mathf.Clamp(
            (jumlahBenar * poinBenar) - (jumlahSalah * Mathf.Abs(poinSalah)),
            0, maxScore);

        if (popupScoreText != null) popupScoreText.text = skorAkhir.ToString();
        if (popupBenarText != null) popupBenarText.text = jumlahBenar.ToString("00");
        if (popupSalahText != null) popupSalahText.text = jumlahSalah.ToString("00");

        int level = LevelFlowManager.GetCurrentLevel();
        LevelProgressManager.CompleteMiniGame(level);

        popup.SetActive(true);
    }

    // ─────────────────────────────────────────────────────────

    public void OnTombolSelesai() => LevelFlowManager.OnGameSelesai();
    public void OnTombolKembali() => LevelFlowManager.GoToRouteMap();
    public void OnTombolUlang()   => SceneLoader.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
}
