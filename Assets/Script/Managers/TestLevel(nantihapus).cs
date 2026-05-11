using UnityEngine;

public class TestLevel : MonoBehaviour
{
    public int testLevel = 3; // ganti angka sesuai level yang mau dites

    void Awake()
    {
        PlayerPrefs.SetInt("CurrentLevel", testLevel);
        PlayerPrefs.Save();
    }
}