using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingManager : MonoBehaviour
{
    public static CookingManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] public GameObject[] cookingProgressPanels; // Массив панелей для нескольких блюд
    [SerializeField] public Image[] progressBarFills;
    [SerializeField] public TextMeshProUGUI[] dishNameTexts;
    [SerializeField] public TextMeshProUGUI[] speedBonusTexts;

    private bool[] isCookingSlots; // Трекинг состояния каждого слота
    private Coroutine[] cookingCoroutines; // Массив корутин
    private int maxSimultaneousDishes = 1; // Начальное значение

    private void Awake()
    {
        Instance = this;
        isCookingSlots = new bool[2]; // Максимум 2 слота
        cookingCoroutines = new Coroutine[2];
    }

    public void UpdateCookingCapacity()
    {
        // Проверяем количество нанятых поваров
        int hiredChefs = DatabaseManager.Instance.GetHiredEmployeesCount();
        maxSimultaneousDishes = hiredChefs >= 1 ? 2 : 1;
    }

    public bool StartCooking(int dishId, string dishName, int baseCookTime)
    {
        // Ищем свободный слот
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

        // Настройка UI для этого слота
        cookingProgressPanels[slotIndex].SetActive(true);
        dishNameTexts[slotIndex].text = dishName;
        progressBarFills[slotIndex].fillAmount = 0f;

        float speedMultiplier = DatabaseManager.Instance.GetCookingSpeedMultiplier();
        float actualCookTime = baseCookTime * speedMultiplier;

        if (speedMultiplier < 1f)
        {
            float bonusPercent = (1f - speedMultiplier) * 100f;
            speedBonusTexts[slotIndex].text = $"+{bonusPercent:F0}% скорость";
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