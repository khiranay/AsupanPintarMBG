using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePindah : MonoBehaviour
{
    [Header("Nama scene tujuan")]
    public string namaScene;

    public void Pindah()
    {
        Debug.Log("[ScenePindah] Pindah ke: " + namaScene);
        SceneManager.LoadScene(namaScene);
    }
}