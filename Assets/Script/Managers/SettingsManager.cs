using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public Slider sliderVolume;
    public AudioSource musikUtama;

    void Start()
    {
        // Load nilai volume yang tersimpan
        float savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
        sliderVolume.value = savedVolume;
        musikUtama.volume = savedVolume;

        // Listen perubahan slider
        sliderVolume.onValueChanged.AddListener(OnVolumeChanged);
    }

    void OnVolumeChanged(float value)
    {
        // Ubah volume audio
        musikUtama.volume = value;

        // Simpan
        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();
    }

    public void OnClickLanjut()
    {
        gameObject.SetActive(false); // tutup popup
    }

    public void OnClickKeluar()
    {
        Application.Quit(); // keluar game
    }
}