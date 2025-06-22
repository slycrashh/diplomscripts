using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TooltipScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Настройки подсказки")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;
    public Vector3 offset = new Vector3(50, -50, 0); // Смещение от курсора
    [TextArea(4, 5)]
    public string tooltipContent = "";
    private Canvas canvas;
    private RectTransform tooltipRect;
    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        tooltipPanel.SetActive(false);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipText.text = tooltipContent;
        tooltipPanel.SetActive(true);

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            canvas.worldCamera,
            out pos);

        tooltipRect.localPosition = pos + (Vector2)offset;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);
    }
}