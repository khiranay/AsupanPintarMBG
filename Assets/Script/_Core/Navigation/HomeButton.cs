using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButton : MonoBehaviour
{
    [Header("=== PENGATURAN HOME ===")]
    public string namaSceneHome = "Home"; // ganti sesuai nama scene menu utama

    public void OnTombolHomediklik()
    {
        Debug.Log("[HomeButton] Kembali ke Home: " + namaSceneHome);
        SceneManager.LoadScene(namaSceneHome);
    }
}