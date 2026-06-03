using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class XRayGameManager : MonoBehaviour, IGameManager
{
    [Header("Item Data")]
    public FoodItemData[] semuaItem;
    public GameObject foodItemPrefab;

    [Header("Conveyor")]
    public Transform itemContainer;
    public Transform spawnPoint;
    public Transform scanPoint;
    public Transform exitPoint;
    public float conveyorSpeed = 500f;

    [Header("Scan")]
    public GameObject scanLine;
    public Image glowEffect;
    public TextMeshProUGUI teksStatus;
    public Sprite spriteGlowHijau;
    public Sprite spriteGlowMerah;
    public float scanDuration = 1.5f;

    [Header("Tombol")]
    public Button tombolMakan;
    public Button tombolBuang;

    [Header("UI")]
    public TextMeshProUGUI teksSkor;
    public TextMeshProUGUI teksTimer;
    [Tooltip("TextMeshProUGUI untuk tampilan 3-2-1-GO! (opsional)")]
    public TextMeshProUGUI teksCountdown;
    public int totalItem = 10;
    public float gameDuration = 60f;

    // BUG FIX #2: Tambahkan popup hasil yang sebelumnya tidak ada
    [Header("Popup Hasil")]
    public GameObject popupHasil;
    public TextMeshProUGUI teksHasilSkor;
    public TextMeshProUGUI teksHasilBenar;
    public TextMeshProUGUI teksHasilSalah;

    private List<FoodItemData> itemQueue = new List<FoodItemData>();
    private FoodItemData currentItem;
    private int skor = 0;
    private int jumlahBenar = 0;
    private int jumlahSalah = 0;
    private int itemProcessed = 0;
    private float timeLeft;
    private bool isWaitingDecision = false;
    private bool gameEnded = false;

    private GameObject currentFoodObj;
    private FoodItem4 currentFoodComp;

    void Start()
    {
        timeLeft = gameDuration;

        tombolMakan.interactable = false;
        tombolBuang.interactable = false;

        if (glowEffect != null) glowEffect.gameObject.SetActive(false);
        if (scanLine != null) scanLine.SetActive(false);
        if (popupHasil != null) popupHasil.SetActive(false);

        tombolMakan.onClick.AddListener(() => OnKeputusan(true));
        tombolBuang.onClick.AddListener(() => OnKeputusan(false));
    }

    // Dipanggil dari tombol X di popup perintah
    public void MulaiGame()
    {
        StartCoroutine(CountdownHelper.Hitung(teksCountdown, () =>
        {
            GenerateItemQueue();
            StartCoroutine(RunGame());
        }));
    }

    void Update()
    {
        if (gameEnded) return;

        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            int detik = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
            if (teksTimer != null)
                teksTimer.text = string.Format("{0:00}:{1:00}", detik / 60, detik % 60);

            // BUG FIX #3: Jika waktu habis saat menunggu keputusan player,
            // paksa keluar dari waiting state agar RunGame() bisa selesai
            if (timeLeft <= 0 && isWaitingDecision)
            {
                tombolMakan.interactable = false;
                tombolBuang.interactable = false;
                isWaitingDecision = false;
            }
        }
    }

    void GenerateItemQueue()
    {
        for (int i = 0; i < totalItem; i++)
        {
            int index = Random.Range(0, semuaItem.Length);
            itemQueue.Add(semuaItem[index]);
        }
    }

    IEnumerator RunGame()
    {
        if (itemQueue.Count > 0)
        {
            SpawnAtSpawnPoint(itemQueue[0]);
        }

        for (int i = 0; i < itemQueue.Count; i++)
        {
            if (timeLeft <= 0) break;

            currentItem = itemQueue[i];

            // 1. Gerakkan makanan ke Scan Point
            yield return StartCoroutine(MoveToPoint(currentFoodObj.transform, scanPoint.position));

            if (timeLeft <= 0) { CleanupCurrentFood(); break; }

            // 2. Spawn item berikutnya di Spawn Point
            GameObject nextFoodObj = null;
            FoodItem4 nextFoodComp = null;
            if (i + 1 < itemQueue.Count)
            {
                nextFoodObj = Instantiate(foodItemPrefab, itemContainer);
                nextFoodObj.transform.position = spawnPoint.position;
                nextFoodComp = nextFoodObj.GetComponent<FoodItem4>();
                if (nextFoodComp != null) nextFoodComp.Setup(itemQueue[i + 1]);
            }

            // 3. Animasi Scan
            yield return StartCoroutine(AnimasiScan(currentItem));

            if (timeLeft <= 0) { CleanupCurrentFood(); break; }

            // 4. Tunggu keputusan player (Update() akan set isWaitingDecision=false jika waktu habis)
            isWaitingDecision = true;
            tombolMakan.interactable = true;
            tombolBuang.interactable = true;

            while (isWaitingDecision)
            {
                yield return null;
            }

            // 5. Bersihkan UI scan
            if (glowEffect != null) glowEffect.gameObject.SetActive(false);
            if (teksStatus != null) teksStatus.text = "";

            // 6. Hapus makanan saat ini
            if (currentFoodObj != null) Destroy(currentFoodObj);

            currentFoodObj = nextFoodObj;
            currentFoodComp = nextFoodComp;

            itemProcessed++;
            yield return new WaitForSeconds(0.1f);
        }

        GameOver();
    }

    void CleanupCurrentFood()
    {
        tombolMakan.interactable = false;
        tombolBuang.interactable = false;
        if (currentFoodObj != null) Destroy(currentFoodObj);
        if (glowEffect != null) glowEffect.gameObject.SetActive(false);
        if (scanLine != null) scanLine.SetActive(false);
    }

    void SpawnAtSpawnPoint(FoodItemData item)
    {
        if (teksStatus != null) teksStatus.text = "ITEM MASUK";
        currentFoodObj = Instantiate(foodItemPrefab, itemContainer);
        currentFoodObj.transform.position = spawnPoint.position;
        currentFoodComp = currentFoodObj.GetComponent<FoodItem4>();
        if (currentFoodComp != null)
            currentFoodComp.Setup(item);
    }

    IEnumerator MoveToPoint(Transform objTransform, Vector3 targetPos)
    {
        while (objTransform != null && Vector3.Distance(objTransform.position, targetPos) > 1f)
        {
            if (timeLeft <= 0) yield break;
            objTransform.position = Vector3.MoveTowards(objTransform.position, targetPos, conveyorSpeed * Time.deltaTime);
            yield return null;
        }
        if (objTransform != null) objTransform.position = targetPos;
    }

    IEnumerator AnimasiScan(FoodItemData item)
    {
        if (teksStatus != null) teksStatus.text = "SCANNING...";
        if (scanLine != null) scanLine.SetActive(true);

        RectTransform scanRect = scanLine != null ? scanLine.GetComponent<RectTransform>() : null;
        float scanHeight = 200f;
        float elapsed = 0f;

        while (elapsed < scanDuration)
        {
            elapsed += Time.deltaTime;
            if (scanRect != null)
            {
                float y = Mathf.Lerp(scanHeight / 2, -scanHeight / 2, elapsed / scanDuration);
                scanRect.anchoredPosition = new Vector2(scanRect.anchoredPosition.x, y);
            }
            yield return null;
        }

        if (scanLine != null) scanLine.SetActive(false);

        if (currentFoodComp != null) currentFoodComp.TampilkanXRay();
        if (teksStatus != null) teksStatus.text = "SCAN COMPLETE!";

        if (glowEffect != null)
        {
            glowEffect.gameObject.SetActive(true);
            glowEffect.transform.position = currentFoodObj.transform.position;
            glowEffect.sprite = item.isAman ? spriteGlowHijau : spriteGlowMerah;
        }

        if (!item.isAman)
            yield return StartCoroutine(ShakeEffect(currentFoodObj.transform));

        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator ShakeEffect(Transform objTransform)
    {
        Vector3 originalPos = objTransform.position;
        float shakeDuration = 0.4f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float xOffset = Mathf.Sin(elapsed * 50f) * 10f;
            objTransform.position = new Vector3(originalPos.x + xOffset, originalPos.y, originalPos.z);
            yield return null;
        }

        objTransform.position = originalPos;
    }

    void OnKeputusan(bool pilihMakan)
    {
        tombolMakan.interactable = false;
        tombolBuang.interactable = false;

        bool benar = (pilihMakan == currentItem.isAman);

        if (benar)
        {
            skor += 10;
            jumlahBenar++;
            if (teksSkor != null) teksSkor.text = skor.ToString();
        }
        else
        {
            jumlahSalah++;
        }

        isWaitingDecision = false;
    }

    // BUG FIX #2: GameOver sekarang menampilkan popup hasil dan menyimpan progress
    void GameOver()
    {
        if (gameEnded) return;
        gameEnded = true;

        tombolMakan.interactable = false;
        tombolBuang.interactable = false;

        int level = PlayerPrefs.GetInt("CurrentLevel", 1);
        LevelProgressManager.CompleteMiniGame(level);

        if (popupHasil != null)
        {
            if (teksHasilSkor != null)  teksHasilSkor.text  = skor.ToString();
            if (teksHasilBenar != null) teksHasilBenar.text = jumlahBenar.ToString();
            if (teksHasilSalah != null) teksHasilSalah.text = jumlahSalah.ToString();
            popupHasil.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[XRayGameManager] popupHasil belum di-assign di Inspector!");
        }
    }

    // Hubungkan ke tombol Selesai di popup hasil
    public void OnTombolSelesai()
    {
        LevelFlowManager.OnGameSelesai();
    }

    // Hubungkan ke tombol Ulang di popup hasil
    public void OnTombolUlang()
    {
        SceneLoader.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
