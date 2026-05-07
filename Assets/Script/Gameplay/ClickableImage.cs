using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableImage : MonoBehaviour, IPointerClickHandler
{
    public SpotTheDifference gameManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        gameManager.OnClickImageB(eventData);
    }
}