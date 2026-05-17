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
    public float conveyorSpeed = 200f;

    [Header("Scan")]
    public Image itemDisplay;
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
    private bool isScanning = false;

    void Start()
    {
        timeLeft = gameDuration;

        // Nonaktifkan tombol dulu
        tombolMakan.interactable = false;
        tombolBuang.interactable = false;
        glowEffect.gameObject.SetActive(false);
        scanLine.SetActive(false);

        // Assign tombol
        tombolMakan.onClick.AddListener(() => OnKeputusan(true));
        tombolBuang.onClick.AddListener(() => OnKeputusan(false));

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
        foreach (var item in itemQueue)
        {
            if (timeLeft <= 0) break;

            currentItem = item;
            yield return StartCoroutine(ProsesSatuItem(item));

            itemProcessed++;
        }

        GameOver();
    }

    IEnumerator ProsesSatuItem(FoodItemData item)
    {
        // 1. Tampilkan item masuk (normal)
        itemDisplay.sprite = item.spriteNormal;
        itemDisplay.gameObject.SetActive(true);
        teksStatus.text = "ITEM MASUK";

        yield return new WaitForSeconds(0.5f);

        // 2. Animasi scan
        yield return StartCoroutine(AnimasiScan(item));

        // 3. Tampilkan hasil & tunggu keputusan
        isWaitingDecision = true;
        tombolMakan.interactable = true;
        tombolBuang.interactable = true;

        // Tunggu sampai player memilih
        while (isWaitingDecision)
        {
            yield return null;
        }

        // 4. Reset untuk item berikutnya
        yield return new WaitForSeconds(0.3f);
        glowEffect.gameObject.SetActive(false);
        itemDisplay.gameObject.SetActive(false);
    }

    IEnumerator AnimasiScan(FoodItemData item)
    {
        teksStatus.text = "SCANNING...";
        scanLine.SetActive(true);

        // Animasi garis scan turun
        RectTransform scanRect = scanLine.GetComponent<RectTransform>();
        float scanHeight = 200f;
        float elapsed = 0f;

        while (elapsed < scanDuration)
        {
            elapsed += Time.deltaTime;
            float y = Mathf.Lerp(scanHeight / 2, -scanHeight / 2, elapsed / scanDuration);
            scanRect.anchoredPosition = new Vector2(0, y);
            yield return null;
        }

        scanLine.SetActive(false);

        // Reveal X-Ray
        itemDisplay.sprite = item.spriteXRay;
        teksStatus.text = "SCAN COMPLETE!";

        // Tampilkan glow sesuai kondisi
        glowEffect.gameObject.SetActive(true);
        glowEffect.sprite = item.isAman ? spriteGlowHijau : spriteGlowMerah;

        // Shake jika berbahaya
        if (!item.isAman)
        {
            StartCoroutine(ShakeEffect());
        }

        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator ShakeEffect()
    {
        RectTransform rect = itemDisplay.rectTransform;
        Vector2 originalPos = rect.anchoredPosition;
        float shakeDuration = 0.4f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = originalPos.x + Mathf.Sin(elapsed * 50f) * 5f;
            rect.anchoredPosition = new Vector2(x, originalPos.y);
            yield return null;
        }

        rect.anchoredPosition = originalPos;
    }

    void OnKeputusan(bool pilihMakan)
    {
        tombolMakan.interactable = false;
        tombolBuang.interactable = false;

        bool benar = (pilihMakan == currentItem.isAman);

        if (benar)
        {
            skor += 10;
            teksSkor.text = skor.ToString();
        }

        isWaitingDecision = false;
    }

    void GameOver()
    {
        int level = PlayerPrefs.GetInt("CurrentLevel", 1);
        LevelProgressManager.CompleteMiniGame(level);

        // Tampilkan popup hasil
        Debug.Log("Game Over! Skor: " + skor);
    }
}