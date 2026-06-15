using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach script ini ke GameObject yang berisi animasi/video/background
/// yang ditempatkan DI DEPAN tombol-tombol UI.
///
/// Fungsi:
/// - Saat GameObject ini aktif (atau child-nya aktif), otomatis nonaktifkan
///   raycastTarget pada semua komponen Image, RawImage, dan CanvasGroup.
/// - Mencegah animasi "menelan" klik yang seharusnya masuk ke tombol di belakangnya.
///
/// Cara pakai:
/// 1. Attach script ini ke GameObject yang membungkus RawImage/Image
///    tempat VideoPlayer atau Animator播放 animasi.
/// 2. Tidak perlu setup apa-apa di Inspector. Biarkan kosong.
/// </summary>
[DisallowMultipleComponent]
public class DisableRaycastOnEnable : MonoBehaviour
{
    [Tooltip("Juga disable raycast di seluruh child object (recursive).")]
    [SerializeField] private bool affectChildren = true;

    [Tooltip("Akan disable raycast di awal Start() juga, untuk kasus " +
             "objek sudah aktif sebelum script ini attach.")]
    [SerializeField] private bool disableOnStart = true;

    private void Start()
    {
        if (disableOnStart)
            Apply();
    }

    private void OnEnable()
    {
        // Jalankan di akhir frame agar child yang baru spawn juga kena
        StartCoroutineSafe();
    }

    private void OnTransformChildrenChanged()
    {
        // Jika child baru ditambahkan saat runtime, apply juga
        if (isActiveAndEnabled)
            Apply();
    }

    private System.Collections.IEnumerator StartCoroutineSafe()
    {
        // Tunggu satu frame agar semua child selesai di-spawn
        yield return null;
        Apply();
    }

    /// <summary>
    /// Disable raycast pada semua Image, RawImage, dan CanvasGroup
    /// di GameObject ini (dan opsional child-nya).
    /// </summary>
    public void Apply()
    {
        if (this == null || gameObject == null) return;

        // Image di GameObject ini
        foreach (var img in GetComponents<Image>())
            img.raycastTarget = false;

        // RawImage di GameObject ini (untuk VideoPlayer output)
        foreach (var raw in GetComponents<RawImage>())
            raw.raycastTarget = false;

        // CanvasGroup di GameObject ini
        foreach (var cg in GetComponents<CanvasGroup>())
        {
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        if (affectChildren)
        {
            // Recursive ke semua child
            var images = GetComponentsInChildren<Image>(true);
            foreach (var img in images)
                img.raycastTarget = false;

            var raws = GetComponentsInChildren<RawImage>(true);
            foreach (var raw in raws)
                raw.raycastTarget = false;

            var groups = GetComponentsInChildren<CanvasGroup>(true);
            foreach (var cg in groups)
            {
                cg.blocksRaycasts = false;
                cg.interactable = false;
            }
        }

        Debug.Log($"[DisableRaycastOnEnable] Applied to '{name}' " +
                  $"(children: {affectChildren})");
    }

    /// <summary>
    /// Panggil manual dari kode lain jika perlu.
    /// </summary>
    public void DisableRaycastNow() => Apply();
}
