using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Mole : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite spriteNormal;
    public Sprite spriteKena;

    [Header("Komponen")]
    public Image moleImage;
    public Button moleButton;

    private bool isVisible = false;
    private bool isHit = false;
    private WhackAMoleManager gameManager;

    public void Init(WhackAMoleManager manager)
    {
        gameManager = manager;
        moleImage.gameObject.SetActive(false);
        moleButton.onClick.AddListener(OnMoleClicked);
    }

    public IEnumerator ShowMole(float visibleDuration)
    {
        if (isVisible) yield break;

        isVisible = true;
        isHit = false;
        moleImage.sprite = spriteNormal;
        moleImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(visibleDuration);

        if (!isHit) HideMole();
    }

    void OnMoleClicked()
    {
        if (!isVisible || isHit) return;

        isHit = true;
        moleImage.sprite = spriteKena;
        gameManager.AddScore(10);

        StartCoroutine(HideAfterDelay(0.3f));
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideMole();
    }

    public void HideMole()
    {
        isVisible = false;
        moleImage.gameObject.SetActive(false);
    }
}