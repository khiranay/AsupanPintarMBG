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

    [Header("Popup Hasil")]
    public GameObject popupHasil;
    public TextMeshProUGUI teksHasilSkor;
    public TextMeshProUGUI teksHasilBenar;
    public TextMeshProUGUI teksHasilSalah;

    [Header("Poin")]
    public int poinBenar = 10;
    public int poinSalah = -5;

    [Header("Floating Score Text")]
    [Tooltip("Prefab TextMeshProUGUI untuk efek skor melayang")]
    public GameObject floatingTextPrefab;
    [Tooltip("Parent transform untuk floating text (drag root GameObject)")]
    public Transform uiParent;

    // ─── Private ───────────────────────────────────────────────────────
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

    // ─────────────────────────────────────────────────────────────────

    void Start()
    {
        timeLeft = gameDuration;

        tombolMakan.interactable = false;
        tombolBuang.interactable = false;

        if (glowEffect != null) glowEffect.gameObject.SetActive(false);
        if (scanLine  != null) scanLine.SetActive(false);
        if (popupHasil != null) popupHasil.SetActive(false);

        tombolMakan.onClick.AddListener(() => OnKeputusan(true));
        tombolBuang.onClick.AddListener(() => OnKeputusan(false));
    }

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

            if (timeLeft <= 0 && isWaitingDecision)
            {
                tombolMakan.interactable = false;
                tombolBuang.interactable = false;
                isWaitingDecision = false;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────

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
        for (int i = 0; i < itemQueue.Count; i++)
        {
            if (timeLeft <= 0) break;

            currentItem = itemQueue[i];

            // 1. Spawn 1 item saja di spawnPoint (tidak ada pre-spawn)
            SpawnItem(currentItem);

            // 2. Gerakkan ke Scan Point
            yield return StartCoroutine(MoveToPoint(currentFoodObj.transform, scanPoint.position));

            if (timeLeft <= 0) { CleanupCurrentFood(); break; }

            // 3. Animasi Scan
            yield return StartCoroutine(AnimasiScan(currentItem));

            if (timeLeft <= 0) { CleanupCurrentFood(); break; }

            // 4. Tunggu keputusan player
            isWaitingDecision = true;
            tombolMakan.interactable = true;
            tombolBuang.interactable = true;

            while (isWaitingDecision)
                yield return null;

            // 5. Cleanup item & UI — berlaku untuk SEMUA kasus:
            //    a) player decide  → currentFoodObj sudah null (di-destroy di OnKeputusan)
            //    b) timeout        → currentFoodObj masih ada, harus di-cleanup di sini
            if (currentFoodObj != null)
            {
                currentFoodObj.SetActive(false);
                Destroy(currentFoodObj);
                currentFoodObj = null;
                currentFoodComp = null;
            }
            if (glowEffect != null) glowEffect.gameObject.SetActive(false);
            if (teksStatus != null) teksStatus.text = "";
            if (scanLine   != null) scanLine.SetActive(false);

            itemProcessed++;
            yield return new WaitForSeconds(0.2f);
        }

        GameOver();
    }

    // ─────────────────────────────────────────────────────────────────

    void SpawnItem(FoodItemData item)
    {
        currentFoodObj = Instantiate(foodItemPrefab, itemContainer);
        currentFoodObj.transform.position = spawnPoint.position;
        currentFoodComp = currentFoodObj.GetComponent<FoodItem4>();
        if (currentFoodComp != null)
            currentFoodComp.Setup(item);
    }

    void CleanupCurrentFood()
    {
        tombolMakan.interactable = false;
        tombolBuang.interactable = false;
        if (currentFoodObj != null)
        {
            Destroy(currentFoodObj);
            currentFoodObj = null;
        }
        if (glowEffect != null) glowEffect.gameObject.SetActive(false);
        if (scanLine   != null) scanLine.SetActive(false);
    }

    IEnumerator MoveToPoint(Transform objTransform, Vector3 targetPos)
    {
        while (objTransform != null && Vector3.Distance(objTransform.position, targetPos) > 1f)
        {
            if (timeLeft <= 0) yield break;
            objTransform.position = Vector3.MoveTowards(
                objTransform.position, targetPos, conveyorSpeed * Time.deltaTime);
            yield return null;
        }
        if (objTransform != null) objTransform.position = targetPos;
    }

    IEnumerator AnimasiScan(FoodItemData item)
    {
        if (teksStatus != null) teksStatus.text = "SCANNING...";
        if (scanLine   != null) scanLine.SetActive(true);

        RectTransform scanRect = scanLine != null ? scanLine.GetComponent<RectTransform>() : null;
        float scanHeight = 200f;
        float elapsed   = 0f;

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
        float elapsed = 0f;

        while (elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            float xOffset = Mathf.Sin(elapsed * 50f) * 10f;
            objTransform.position = new Vector3(originalPos.x + xOffset, originalPos.y, originalPos.z);
            yield return null;
        }

        objTransform.position = originalPos;
    }

    // ─────────────────────────────────────────────────────────────────

    void OnKeputusan(bool pilihMakan)
    {
        tombolMakan.interactable = false;
        tombolBuang.interactable = false;

        bool benar = (pilihMakan == currentItem.isAman);
        Vector3 posisiTeks = currentFoodObj != null
            ? currentFoodObj.transform.position
            : Vector3.zero;

        if (benar)
        {
            skor += poinBenar;
            jumlahBenar++;
            if (teksSkor != null) teksSkor.text = skor.ToString();
            MunculkanFloatingText("+" + poinBenar, Color.green, posisiTeks);
        }
        else
        {
            jumlahSalah++;
            MunculkanFloatingText(poinSalah.ToString(), Color.red, posisiTeks);
        }

        // Langsung destroy & sembunyikan semua visual setelah keputusan
        if (currentFoodObj != null)
        {
            Destroy(currentFoodObj);   // jadwal destroy akhir frame
            currentFoodObj.SetActive(false); // langsung hilang di frame ini
            currentFoodObj = null;
            currentFoodComp = null;
        }
        if (glowEffect != null) glowEffect.gameObject.SetActive(false);
        if (teksStatus != null) teksStatus.text = "";
        if (scanLine   != null) scanLine.SetActive(false);

        isWaitingDecision = false;
    }

    // ─────────────────────────────────────────────────────────────────

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
        float durasi  = 1f;
        float timer   = 0f;
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

    // ─────────────────────────────────────────────────────────────────

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
            // Skor akhir = (benar × poinBenar) - (salah × |poinSalah|)
            int skorAkhir = Mathf.Max(0,
                (jumlahBenar * poinBenar) - (jumlahSalah * Mathf.Abs(poinSalah)));

            if (teksHasilSkor  != null) teksHasilSkor.text  = skorAkhir.ToString();
            if (teksHasilBenar != null) teksHasilBenar.text = jumlahBenar.ToString("00");
            if (teksHasilSalah != null) teksHasilSalah.text = jumlahSalah.ToString("00");
            popupHasil.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[XRayGameManager] popupHasil belum di-assign di Inspector!");
        }
    }

    // ─────────────────────────────────────────────────────────────────

    public void OnTombolSelesai() => LevelFlowManager.OnGameSelesai();
    public void OnTombolUlang()   => SceneLoader.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
}
