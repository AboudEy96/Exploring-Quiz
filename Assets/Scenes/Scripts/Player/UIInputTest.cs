using UnityEngine;
using UnityEngine.EventSystems;

public class UIInputTest : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("CANVAS CLICK: " + eventData.pointerCurrentRaycast.gameObject);
    }
}