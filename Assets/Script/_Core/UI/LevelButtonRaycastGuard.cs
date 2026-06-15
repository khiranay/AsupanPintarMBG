using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach script ini ke GameObject yang sama dengan LevelButton
/// (atau ke parent yang berisi kumpulan tombol).
///
/// Fungsi:
/// - Memastikan Button.interactable = true saat scene aktif
/// - Memastikan Image (jika ada) di tombol ini raycastTarget = true
/// - Memastikan CanvasGroup (jika ada di parent) tidak memblokir interaksi
/// - Memaksa tombol naik ke sibling terakhir agar dirender di atas layer lain
///
/// Tujuan: menjadi "safety net" agar tombol SELALU bisa diklik
/// meskipun ada GameObject animasi di depannya.
/// </summary>
[DisallowMultipleComponent]
public class LevelButtonRaycastGuard : MonoBehaviour
{
    [Tooltip("Naikkan tombol ke sibling terakhir di parent (render paling atas).")]
    [SerializeField] private bool bringToFront = true;

    [Tooltip("Pastikan CanvasGroup parent.blocksRaycasts = true.")]
    [SerializeField] private bool unblockParentCanvasGroup = true;

    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
    }

    private void Start()
    {
        Apply();
    }

    private void OnEnable()
    {
        // Tunggu frame berikutnya agar animasi di belakangnya sudah settle
        StartCoroutine(ApplyNextFrame());
    }

    private System.Collections.IEnumerator ApplyNextFrame()
    {
        yield return null;
        Apply();
    }

    public void Apply()
    {
        if (this == null || gameObject == null) return;

        // 1. Pastikan Button.interactable = true
        if (btn != null)
        {
            btn.interactable = true;
        }

        // 2. Pastikan Image di tombol ini raycastTarget = true
        foreach (var img in GetComponents<Image>())
            img.raycastTarget = true;

        // 3. Jangan blokir dari CanvasGroup parent
        if (unblockParentCanvasGroup && transform.parent != null)
        {
            var parentGroup = transform.parent.GetComponent<CanvasGroup>();
            if (parentGroup != null)
            {
                parentGroup.blocksRaycasts = true;
                parentGroup.interactable = true;
            }
        }

        // 4. Pindahkan ke sibling terakhir (render paling atas)
        if (bringToFront)
        {
            transform.SetAsLastSibling();
        }
    }
}
