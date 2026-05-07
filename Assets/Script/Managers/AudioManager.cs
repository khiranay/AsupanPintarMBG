using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource bgm;

    public GameObject soundOnIcon;
    public GameObject soundOffIcon;

    public void ToggleSound()
    {
        bool isMuted = bgm.mute;
        bgm.mute = !isMuted;

        soundOnIcon.SetActive(isMuted);
        soundOffIcon.SetActive(!isMuted);
    }
}