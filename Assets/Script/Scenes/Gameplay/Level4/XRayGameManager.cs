using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class XRayGameManager : MonoBehaviour
{
    [Header("Item Data")]
    public FoodItemData[] semuaItem;
    public GameObject foodItemPrefab;

    [Header("Conveyor")]
    public Transform itemContainer;
    public Transform spawnPoint;
    public Transform scanPoint;
    public Transform exitPoint;
    public float conveyorSpeed = 500f; // Kecepatan dinaikkan sedikit agar tidak terlalu lambat

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
    public int totalItem = 10;
    public float gameDuration = 60f;

    private List<FoodItemData> itemQueue = new List<FoodItemData>();
    private FoodItemData currentItem;
    private int skor = 0;
    private int itemProcessed = 0;
    private float timeLeft;
    private bool isWaitingDecision = false;
    
    private GameObject currentFoodObj;
    private FoodItem4 currentFoodComp;

    void Start()
    {
        timeLeft = gameDuration;

        // Nonaktifkan tombol dulu
        tombolMakan.interactable = false;
        tombolBuang.interactable = false;
        
        if (glowEffect != null) glowEffect.gameObject.SetActive(false);
        if (scanLine != null) scanLine.SetActive(false);

        // Assign event tombol
        tombolMakan.onClick.AddListener(() => OnKeputusan(true));
        tombolBuang.onClick.AddListener(() => OnKeputusan(false));

        // Game belum dimulai di sini. Akan dimulai saat MulaiGame() dipanggil.
    }

    // Dipanggil dari tombol X di popup perintah
    public void MulaiGame()
    {
        // Generate antrian item random
        GenerateItemQueue();
        StartCoroutine(RunGame());
    }

    void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            int detik = Mathf.CeilToInt(timeLeft);
            if (teksTimer != null)
                teksTimer.text = string.Format("{0:00}:{1:00}", detik / 60, detik % 60);
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
        // Pertama kali mulai, spawn item pertama di Spawn Point
        if (itemQueue.Count > 0)
        {
            SpawnAtSpawnPoint(itemQueue[0]);
        }

        for (int i = 0; i < itemQueue.Count; i++)
        {
            if (timeLeft <= 0) break;

            currentItem = itemQueue[i];

            // 1. Gerakkan makanan ke Scan Point (tengah)
            yield return StartCoroutine(MoveToPoint(currentFoodObj.transform, scanPoint.position));

            // 2. Mulai spawn item berikutnya di titik antrean (Spawn Point) agar siap menunggu
            GameObject nextFoodObj = null;
            FoodItem4 nextFoodComp = null;
            if (i + 1 < itemQueue.Count)
            {
                nextFoodObj = Instantiate(foodItemPrefab, itemContainer);
                nextFoodObj.transform.position = spawnPoint.position;
                nextFoodComp = nextFoodObj.GetComponent<FoodItem4>();
                if (nextFoodComp != null) nextFoodComp.Setup(itemQueue[i + 1]);
            }

            // 3. Mulai Animasi Scan
            yield return StartCoroutine(AnimasiScan(currentItem));

            // 4. Tampilkan hasil & tunggu keputusan dari klik tombol
            isWaitingDecision = true;
            tombolMakan.interactable = true;
            tombolBuang.interactable = true;

            while (isWaitingDecision)
            {
                yield return null;
            }

            // 5. Setelah dipilih, matikan lampu glow
            if (glowEffect != null) glowEffect.gameObject.SetActive(false);
            if (teksStatus != null) teksStatus.text = "";

            // 6. Hapus makanan (Langsung Hilang)
            Destroy(currentFoodObj);

            // Set currentFood menjadi nextFood yang sudah menunggu untuk putaran berikutnya
            currentFoodObj = nextFoodObj;
            currentFoodComp = nextFoodComp;

            itemProcessed++;
            yield return new WaitForSeconds(0.1f); // Jeda sangat singkat
        }

        GameOver();
    }

    void SpawnAtSpawnPoint(FoodItemData item)
    {
        if (teksStatus != null) teksStatus.text = "ITEM MASUK";
        currentFoodObj = Instantiate(foodItemPrefab, itemContainer);
        currentFoodObj.transform.position = spawnPoint.position;
        currentFoodComp = currentFoodObj.GetComponent<FoodItem4>();
        if (currentFoodComp != null)
        {
            currentFoodComp.Setup(item);
        }
    }

    IEnumerator MoveToPoint(Transform objTransform, Vector3 targetPos)
    {
        // Loop memindahkan objek pelan-pelan sampai dekat dengan target
        while (Vector3.Distance(objTransform.position, targetPos) > 1f)
        {
            objTransform.position = Vector3.MoveTowards(objTransform.position, targetPos, conveyorSpeed * Time.deltaTime);
            yield return null;
        }
        objTransform.position = targetPos; // Pastikan posisi akhirnya pas
    }

    IEnumerator AnimasiScan(FoodItemData item)
    {
        if (teksStatus != null) teksStatus.text = "SCANNING...";
        if (scanLine != null) scanLine.SetActive(true);

        // Animasi garis scan turun
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

        // Reveal X-Ray (Gambar Berubah)
        if (currentFoodComp != null) currentFoodComp.TampilkanXRay();
        if (teksStatus != null) teksStatus.text = "SCAN COMPLETE!";

        // Tampilkan glow sesuai kondisi
        if (glowEffect != null)
        {
            glowEffect.gameObject.SetActive(true);
            // Pastikan posisi glow ngikutin makanan
            glowEffect.transform.position = currentFoodObj.transform.position; 
            glowEffect.sprite = item.isAman ? spriteGlowHijau : spriteGlowMerah;
        }

        // Shake jika berbahaya
        if (!item.isAman)
        {
            yield return StartCoroutine(ShakeEffect(currentFoodObj.transform));
        }

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
            float xOffset = Mathf.Sin(elapsed * 50f) * 10f; // Goyang 10 unit ke kiri/kanan
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
            if (teksSkor != null) teksSkor.text = skor.ToString();
        }

        isWaitingDecision = false;
    }

    void GameOver()
    {
        int level = PlayerPrefs.GetInt("CurrentLevel", 1);
        LevelProgressManager.CompleteMiniGame(level);
        
        Debug.Log("Game Over! Skor: " + skor);
    }
}