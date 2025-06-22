using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingManager : MonoBehaviour
{
    public static CookingManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] public GameObject[] cookingProgressPanels; // ������ ������� ��� ���������� ����
    [SerializeField] public Image[] progressBarFills;
    [SerializeField] public TextMeshProUGUI[] dishNameTexts;
    [SerializeField] public TextMeshProUGUI[] speedBonusTexts;

    private bool[] isCookingSlots; // ������� ��������� ������� �����
    private Coroutine[] cookingCoroutines; // ������ �������
    private int maxSimultaneousDishes = 1; // ��������� ��������

    private void Awake()
    {
        Instance = this;
        isCookingSlots = new bool[2]; // �������� 2 �����
        cookingCoroutines = new Coroutine[2];
    }

    public void UpdateCookingCapacity()
    {
        // ��������� ���������� ������� �������
        int hiredChefs = DatabaseManager.Instance.GetHiredEmployeesCount();
        maxSimultaneousDishes = hiredChefs >= 1 ? 2 : 1;
    }

    public bool StartCooking(int dishId, string dishName, int baseCookTime)
    {
        // ���� ��������� ����
        for (int i = 0; i < maxSimultaneousDishes; i++)
        {
            if (!isCookingSlots[i])
            {
                cookingCoroutines[i] = StartCoroutine(CookDishCoroutine(i, dishId, dishName, baseCookTime));
                return true;
            }
        }
        return false;
    }

    public IEnumerator CookDishCoroutine(int slotIndex, int dishId, string dishName, int baseCookTime)
    {
        isCookingSlots[slotIndex] = true;

        // ��������� UI ��� ����� �����
        cookingProgressPanels[slotIndex].SetActive(true);
        dishNameTexts[slotIndex].text = dishName;
        progressBarFills[slotIndex].fillAmount = 0f;

        float speedMultiplier = DatabaseManager.Instance.GetCookingSpeedMultiplier();
        float actualCookTime = baseCookTime * speedMultiplier;

        if (speedMultiplier < 1f)
        {
            float bonusPercent = (1f - speedMultiplier) * 100f;
            speedBonusTexts[slotIndex].text = $"+{bonusPercent:F0}% ��������";
        }
        else
        {
            speedBonusTexts[slotIndex].text = "";
        }

        float timer = 0;
        while (timer < actualCookTime)
        {
            timer += Time.deltaTime;
            progressBarFills[slotIndex].fillAmount = timer / actualCookTime;
            yield return null;
        }

        DatabaseManager.Instance.IncreaseDishStock(dishId);
        cookingProgressPanels[slotIndex].SetActive(false);
        isCookingSlots[slotIndex] = false;
    }
}