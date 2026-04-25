using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [Header("Popup Setting")]
    public GameObject popupSetting;
    public GameObject overlay;

    [Header("Popup Kenali MBG")]
    public GameObject popupKenaliMBG;

    // Setting
    public void OnClickSetting()
    {
        popupSetting.SetActive(true);
        overlay.SetActive(true);
    }

    public void OnClickCloseSetting()
    {
        popupSetting.SetActive(false);
        overlay.SetActive(false);
    }

    // Kenali MBG
    public void OnClickKenaliMBG()
    {
        popupKenaliMBG.SetActive(true);
        overlay.SetActive(true);
    }

    public void OnClickCloseKenaliMBG()
    {
        popupKenaliMBG.SetActive(false);
        overlay.SetActive(false);
    }
}