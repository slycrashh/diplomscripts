using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DecorTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("��������� ���������")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;
    public Vector3 offset = new Vector3(50, -50, 0); // �������� �� �������
    [TextArea(3, 5)]
    public string tooltipContent = "��������: �������\n�����: +10% ������";
    [Header("�������� ������")]
    public string decorName;
    public string decorPrice;
    public string decorBonus;
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
        //tooltipText.text = tooltipContent;
        tooltipText.text = $"<b>{decorName}</b>\n����: {decorPrice}\n����� � �������: {decorBonus}";
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