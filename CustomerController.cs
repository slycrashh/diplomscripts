using UnityEngine;
using System.Collections;
using System.Linq;
using static DatabaseManager;
using System.Collections.Generic;

public class CustomerController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public LayerMask chairLayer; // Слои для стульев
    public Transform returnPosition; // Точка возврата

    private Transform targetChair;
    private Dish selectedDish; // Выбранное блюдо
    private bool isDishReady = false; // Флаг, указывающий, что блюдо готово
    private bool isLeaving = false; // Флаг, указывающий, что клиент уходит

    void Start()
    {
        // Проверяем, назначен ли returnPosition
        if (returnPosition == null)
        {
            Debug.Log("Return Position is not assigned in the Inspector!");
            // Создаем временную точку возврата
            GameObject tempReturnPoint = new GameObject("TempReturnPosition");
            tempReturnPoint.transform.position = new Vector3(-10, 0, 0); // Укажите нужные координаты
            returnPosition = tempReturnPoint.transform;
        }

        StartCustomerBehavior();
    }

    private void StartCustomerBehavior()
    {
        // Сбрасываем состояние гостя
        isDishReady = false;
        isLeaving = false;
        selectedDish = null;
        targetChair = null;

        // Начинаем поиск стула
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
                targetChair = null; // Стул уже занят, ищем другой
                FindNearestFreeChair(); // Повторяем поиск
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
        // Двигаемся к стулу
        while (Vector3.Distance(transform.position, targetChair.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetChair.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Остановка на месте и "сесть"
        Debug.Log("Customer has seated.");
        targetChair.GetComponent<Chair>().OccupyChair();

        // Выбираем случайное разблокированное блюдо
        List<Dish> unlockedDishes = DatabaseManager.Instance.GetUnlockedDishes();
        if (unlockedDishes.Count > 0)
        {
            int randomIndex = Random.Range(0, unlockedDishes.Count);
            selectedDish = unlockedDishes[randomIndex];
            Debug.Log($"Customer ordered: {selectedDish.DishName} (Cooking time: {selectedDish.CookTime} seconds)");
        }
        else
        {
            Debug.LogError("No unlocked dishes available.");
            yield break;
        }

        // Ждем либо готовности блюда, либо истечения времени приготовления + 20 секунд
        float waitTime = selectedDish.CookTime + 20f; // Время приготовления + 20 секунд
        float elapsedTime = 0f;

        while (elapsedTime < waitTime && !isDishReady)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (isDishReady)
        {
            Debug.Log("Dish is ready! Customer is leaving with the dish.");
        }
        else
        {
            Debug.LogWarning("Dish was not ready in time. Customer is leaving without food.");
        }

        // Освобождаем стул и возвращаемся
        Debug.Log("Customer is leaving.");
        targetChair.GetComponent<Chair>().FreeChair();
        Debug.Log("Chair is free now.");

        // Возвращаемся в указанную позицию
        if (returnPosition != null)
        {
            Debug.Log("Moving to return position: " + returnPosition.position);
            while (Vector3.Distance(transform.position, returnPosition.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, returnPosition.position, moveSpeed * Time.deltaTime);
                yield return null;
            }
            Debug.Log("Customer has returned to the specified position.");
        }
        else
        {
            Debug.LogError("Return position is not assigned.");
        }

        // Ждем 30 секунд перед повторным запуском поведения
        yield return new WaitForSeconds(30f);

        // Перезапускаем поведение гостя
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

    public int GetSelectedDishId()
    {
        return selectedDish != null ? selectedDish.DishID : -1;
    }
}