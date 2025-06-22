using UnityEngine;
using System.Collections;
using System.Linq;
using static DatabaseManager;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CustomerController : MonoBehaviour
{
    public GameObject orderUIPrefab; // Префаб UI для заказа
    public Vector2 uiOffset = new Vector2(0, 50f); // Смещение в пикселях

    private GameObject orderUIInstance;
    private RectTransform uiTransform;
    private Canvas mainCanvas;
    public float moveSpeed = 2f;
    public LayerMask chairLayer; // Слои для стульев
    public Transform returnPosition; // Точка возврата
    private bool hasOrderExpired = false;
    private Transform targetChair;
    private Dish selectedDish; // Выбранное блюдо
    private bool isDishReady = false; // Флаг, указывающий, что блюдо готово
    private bool isLeaving = false; // Флаг, указывающий, что клиент уходит
    private float timeRemaining; // Таймер ожидания
    private bool isWaiting = true;
    private float returnTime = 120f; // Время возвращения (2 минуты для первого гостя)
    private int completedOrders = 0; // Количество выполненных заказов

    void Start()
    {
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Основной Canvas не найден в сцене!");
            enabled = false;
            return;
        }
        // Инициализация UI
        orderUIInstance = Instantiate(orderUIPrefab, MenuManager.Instance.transform);
        uiTransform = orderUIInstance.GetComponent<RectTransform>();
        orderUIInstance.SetActive(false);

        // Инициализация позиции возврата
        if (returnPosition == null)
        {
            Debug.Log("Return Position is not assigned in the Inspector!");
            GameObject tempReturnPoint = new GameObject("TempReturnPosition");
            tempReturnPoint.transform.position = new Vector3(-10, 0, 0);
            returnPosition = tempReturnPoint.transform;
        }

        StartCustomerBehavior(); // Запуск основного поведения
    }
    void UpdateUIPosition()
    {
        if (orderUIInstance == null || mainCanvas == null) return;

        // Конвертируем мировые координаты в экранные
        Vector3 worldPos = transform.position + new Vector3(0, 1.5f, 0);
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // Для Screen Space - Overlay
        if (mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            uiTransform.position = screenPos + uiOffset;
        }
        // Для Screen Space - Camera
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mainCanvas.transform as RectTransform,
                screenPos,
                mainCanvas.worldCamera,
                out Vector2 localPoint);

            uiTransform.localPosition = localPoint + uiOffset;
        }
    }
    private void StartCustomerBehavior()
    {
        isDishReady = false;
        isLeaving = false;
        selectedDish = null;
        targetChair = null;

        FindNearestFreeChair();
    }

    void FindNearestFreeChair()
    {
        Collider2D[] chairs = Physics2D.OverlapCircleAll(transform.position, 50f, chairLayer);
        float minDistance = Mathf.Infinity;
        foreach (Collider2D chair in chairs)
        {
            Chair chairComponent = chair.GetComponent<Chair>();
            if (chairComponent != null && chairComponent.IsAvailable)
            {
                float distance = Vector3.Distance(transform.position, chair.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    targetChair = chair.transform;
                }
            }
        }

        if (targetChair != null)
        {
            Chair chairComponent = targetChair.GetComponent<Chair>();
            if (chairComponent != null && chairComponent.OccupyChair())
            {
                Debug.Log("Found nearest free chair at position: " + targetChair.position);
                StartCoroutine(CustomerRoutine());
            }
            else
            {
                Debug.LogError("Chair is already occupied.");
                targetChair = null;
                FindNearestFreeChair();
            }
        }
        else
        {
            Debug.LogError("No free chair found.");
        }
    }

    IEnumerator CustomerRoutine()
    {
        Debug.Log("Starting customer routine.");

        // Движение к стулу
        while (Vector3.Distance(transform.position, targetChair.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetChair.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log("Customer has seated.");
        targetChair.GetComponent<Chair>().OccupyChair();

        // Выбираем случайное разблокированное блюдо
        List<Dish> unlockedDishes = DatabaseManager.Instance.GetUnlockedDishes();
        if (unlockedDishes.Count > 0)
        {
            int randomIndex = Random.Range(0, unlockedDishes.Count);
            selectedDish = unlockedDishes[randomIndex];
            float waitTime = selectedDish.CookTime + 20f;
            Debug.Log($"Customer ordered: {selectedDish.DishName} (Waiting: {waitTime} seconds)");

            // Создаем UI элемент в основном Canvas
            if (orderUIPrefab != null)
            {
                // Находим основной Canvas
                Canvas mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas != null)
                {
                    // Создаем экземпляр UI
                    orderUIInstance = Instantiate(orderUIPrefab, mainCanvas.transform);
                    orderUIInstance.transform.SetSiblingIndex(0);
                    uiTransform = orderUIInstance.GetComponent<RectTransform>();

                    // Настраиваем начальные параметры
                    uiTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    uiTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    uiTransform.pivot = new Vector2(0.5f, 0);

                    // Находим элементы UI
                    TextMeshProUGUI dishText = orderUIInstance.transform.Find("DishName").GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI timerText = orderUIInstance.transform.Find("Timer").GetComponent<TextMeshProUGUI>();
                    Button serveButton = orderUIInstance.transform.Find("ServeButton").GetComponent<Button>();

                    // Настраиваем содержимое
                    dishText.text = selectedDish.DishName;
                    serveButton.onClick.AddListener(() => {
                        if (!isDishReady && DatabaseManager.Instance.GetDishStockQuantity(selectedDish.DishID) > 0)
                        {
                            DatabaseManager.Instance.DecreaseDishStock(selectedDish.DishID);
                            isDishReady = true;
                        }
                    });

                    orderUIInstance.SetActive(true);
                    Debug.Log("UI элемент успешно создан и активирован");
                }
                else
                {
                    Debug.LogError("Основной Canvas не найден в сцене!");
                }
            }

            // Ожидание с таймером
            float timer = waitTime;
            while (timer > 0 && !isDishReady)
            {
                timer -= Time.deltaTime;

                // Обновляем позицию UI
                if (orderUIInstance != null)
                {
                    // Конвертируем мировые координаты в экранные
                    Vector3 worldPos = transform.position + new Vector3(0, 1.5f, 0);
                    Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                    uiTransform.position = screenPos;

                    // Обновляем таймер
                    TextMeshProUGUI timerText = orderUIInstance.transform.Find("Timer").GetComponent<TextMeshProUGUI>();
                    timerText.text = $"{Mathf.CeilToInt(timer)} сек";
                }

                yield return null;
            }

            // Уничтожаем UI
            if (orderUIInstance != null)
            {
                Destroy(orderUIInstance);
                orderUIInstance = null;
            }

            // Если время вышло
            if (timer <= 0 && !isDishReady)
            {
                Debug.Log("Время вышло! Гость уходит без блюда.");
                isDishReady = true;
            }
        }
        else
        {
            Debug.LogError("No unlocked dishes available.");
            yield break;
        }

        // Логика ухода
        DatabaseManager.Instance.AddMoney(selectedDish.Price);
        MoneyUI moneyUI = FindObjectOfType<MoneyUI>();
        if (moneyUI != null)
        {
            
        }
        Debug.Log("Customer is leaving.");
        targetChair.GetComponent<Chair>().FreeChair();

        if (returnPosition != null)
        {
            while (Vector3.Distance(transform.position, returnPosition.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, returnPosition.position, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        yield return new WaitForSeconds(returnTime);
        StartCustomerBehavior();
    }

    /// <summary>
    /// Вызывается, когда блюдо готово
    /// </summary>
    public void OnDishReady()
    {
        if (!isDishReady)
        {
            isDishReady = true;
            Debug.Log("Dish is ready! Customer is leaving with the dish.");
        }
    }

    /// <summary>
    /// Получить выбранное блюдо
    /// </summary>
    public Dish GetSelectedDish()
    {
        return selectedDish;
    }

    /// <summary>
    /// Получить ID выбранного блюда
    /// </summary>
    public int GetSelectedDishId()
    {
        return selectedDish != null ? selectedDish.DishID : -1;
    }
    public void ServeDish(Dish dish)
    {
        selectedDish = dish;
        isDishReady = true;
        Debug.Log($"Блюдо {dish.DishName} выдано гостю.");
    }
    public void ReceiveDish()
    {
        if (!isDishReady)
        {
            isDishReady = true;
            Debug.Log("Блюдо получено через меню");
        }
    }
}