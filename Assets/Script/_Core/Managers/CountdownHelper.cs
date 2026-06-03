using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Utility countdown reusable untuk semua mini-game.
///
/// CARA PAKAI di game manager manapun:
///   StartCoroutine(CountdownHelper.Hitung(teksCountdown, () => {
///       // kode yang jalan setelah GO!
///   }));
///
/// CARA SETUP di Inspector:
/// 1. Buat TextMeshProUGUI di canvas game (misal teks besar di tengah layar)
/// 2. Set font size besar, warna mencolok
/// 3. Set inactive di default (script ini yang aktifkan/matikan)
/// 4. Drag ke field teksCountdown di game manager
/// </summary>
public static class CountdownHelper
{
    /// <summary>
    /// Menampilkan countdown 3 - 2 - 1 - GO! lalu memanggil onSelesai.
    /// Jika teksCountdown null, countdown tetap berjalan (hanya tanpa visual).
    /// </summary>
    public static IEnumerator Hitung(TextMeshProUGUI teksCountdown, Action onSelesai)
    {
        Debug.Log("[Countdown] Hitung() dipanggil. teksCountdown = " +
                  (teksCountdown == null ? "NULL" : teksCountdown.name));

        if (teksCountdown != null)
        {
            // Pastikan semua parent aktif agar teks bisa tampil
            Transform parent = teksCountdown.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                {
                    Debug.LogWarning("[Countdown] Parent '" + parent.name +
                                     "' tidak aktif! Mengaktifkan agar countdown tampil.");
                    parent.gameObject.SetActive(true);
                }
                parent = parent.parent;
            }

            teksCountdown.gameObject.SetActive(true);

            Debug.Log("[Countdown] activeInHierarchy setelah SetActive(true) = " +
                      teksCountdown.gameObject.activeInHierarchy);

            teksCountdown.text = "3";
            yield return new WaitForSeconds(1f);

            teksCountdown.text = "2";
            yield return new WaitForSeconds(1f);

            teksCountdown.text = "1";
            yield return new WaitForSeconds(1f);

            teksCountdown.text = "GO!";
            yield return new WaitForSeconds(0.5f);

            teksCountdown.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[Countdown] teksCountdown NULL — countdown berjalan tanpa visual.");
            yield return new WaitForSeconds(0.3f);
        }

        Debug.Log("[Countdown] Selesai, memanggil onSelesai.");
        onSelesai?.Invoke();
    }
}
