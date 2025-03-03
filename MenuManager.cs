using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static DatabaseManager;

public class MenuManager : MonoBehaviour
{
    // Панели меню
    public GameObject mainMenuPanel;
    public GameObject ingredientsMenuPanel;
    public GameObject hireStaffMenuPanel;
    public GameObject unlockDishesMenuPanel;
    public GameObject cookDishMenuPanel; // Новая панель для приготовления блюд
                                         // Кнопки для разблокировки блюд
    public Button[] unlockDishButtons;
    // Кнопки для блюд
    public Button[] dishButtons; // Массив кнопок для блюд
    public DatabaseManager databaseManager; // Ссылка на DatabaseManager
    private int unlockedDishesCount = 1; // Начинаем с 1, так как первое блюдо уже разблокировано
    void Start()
    {
        // Убедитесь, что DatabaseManager инициализирован
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("DatabaseManager не инициализирован.");
            return;
        }

        HideAllMenus();
        InitializeDishButtons();
        InitializeUnlockDishButtons();
        // Инициализация databaseManager
        databaseManager = DatabaseManager.Instance;
        if (databaseManager == null)
        {
            Debug.LogError("DatabaseManager не найден.");
        }

        // Инициализация dishButtons
        if (dishButtons == null || dishButtons.Length == 0)
        {
            Debug.LogError("Кнопки блюд (dishButtons) не инициализированы.");
        }
    }

    // Инициализация кнопок для блюд
    private void InitializeDishButtons()
    {
        for (int i = 0; i < dishButtons.Length; i++)
        {
            int dishIndex = i; // Локальная переменная для замыкания
            dishButtons[i].onClick.AddListener(() => OnDishButtonClicked(dishIndex));
        }
    }
    private void InitializeUnlockDishButtons()
    {
        for (int i = 0; i < unlockDishButtons.Length; i++)
        {
            // Первое блюдо всегда доступно для разблокировки
            if (i == 0)
            {
                unlockDishButtons[i].interactable = true;
            }
            else
            {
                // Остальные блюда доступны только если предыдущее блюдо разблокировано
                unlockDishButtons[i].interactable = i < unlockedDishesCount;
            }

            // Добавляем обработчик нажатия
            int dishIndex = i; // Локальная переменная для замыкания
            unlockDishButtons[i].onClick.RemoveAllListeners();
            unlockDishButtons[i].onClick.AddListener(() => OnUnlockDishButtonClicked(dishIndex));
        }
    }

    // Показать главное меню
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        ingredientsMenuPanel.SetActive(false);
        hireStaffMenuPanel.SetActive(false);
        unlockDishesMenuPanel.SetActive(false);
        cookDishMenuPanel.SetActive(false);
    }

    // Показать меню покупки ингредиентов
    public void ShowIngredientsMenu()
    {
        mainMenuPanel.SetActive(false);
        ingredientsMenuPanel.SetActive(true);
        hireStaffMenuPanel.SetActive(false);
        unlockDishesMenuPanel.SetActive(false);
        cookDishMenuPanel.SetActive(false);
    }

    // Показать меню найма персонала
    public void ShowHireStaffMenu()
    {
        mainMenuPanel.SetActive(false);
        ingredientsMenuPanel.SetActive(false);
        hireStaffMenuPanel.SetActive(true);
        unlockDishesMenuPanel.SetActive(false);
        cookDishMenuPanel.SetActive(false);
    }

    // Показать меню разблокировки блюд
    public void ShowUnlockDishesMenu()
    {
        mainMenuPanel.SetActive(false);
        ingredientsMenuPanel.SetActive(false);
        hireStaffMenuPanel.SetActive(false);
        unlockDishesMenuPanel.SetActive(true);
        cookDishMenuPanel.SetActive(false);

        // Обновляем состояние кнопок разблокировки
        InitializeUnlockDishButtons();
    }

    // Показать меню приготовления блюд
    public void ShowCookDishMenu()
    {
        mainMenuPanel.SetActive(false);
        ingredientsMenuPanel.SetActive(false);
        hireStaffMenuPanel.SetActive(false);
        unlockDishesMenuPanel.SetActive(false);
        cookDishMenuPanel.SetActive(true);

        UpdateDishMenu(); // Обновляем состояние кнопок при открытии меню
    }

    // Обновить состояние кнопок в меню приготовления блюд
    // Обновить состояние кнопок в меню приготовления блюд
    public void UpdateDishMenu()
    {
        List<Dish> unlockedDishes = databaseManager.GetUnlockedDishes();
        Debug.Log($"Тест 1");
        for (int i = 0; i < dishButtons.Length; i++)
        {
            if (i < unlockedDishes.Count)
            {
                Debug.Log($"Тест 2 +");
                // Блюдо разблокировано
                Dish dish = unlockedDishes[i];
                //dishButtons[i].GetComponentInChildren<Text>().text = dish.DishName;
                dishButtons[i].interactable = true;
            }
            else
            {
                Debug.Log($"Тест 2 -");
                // Блюдо не разблокировано или отсутствует
                //dishButtons[i].GetComponentInChildren<Text>().text = "Locked";
                dishButtons[i].interactable = false;
            }
        }
    }

    // Обработчик нажатия на кнопку блюда
    private void OnDishButtonClicked(int dishIndex)
    {
        List<Dish> unlockedDishes = databaseManager.GetUnlockedDishes();
        if (dishIndex < unlockedDishes.Count)
        {
            Dish selectedDish = unlockedDishes[dishIndex];
            if (databaseManager.CanCookDish(selectedDish.DishID))
            {
                databaseManager.CookDish(selectedDish.DishID);
                Debug.Log($"Cooking {selectedDish.DishName}...");
                ShowMainMenu(); // Возвращаемся в главное меню после приготовления
            }
            else
            {
                Debug.LogWarning("Not enough ingredients to cook this dish.");
            }
        }
    }

    // Скрыть все меню (если нужно)
    public void HideAllMenus()
    {
        mainMenuPanel.SetActive(false);
        ingredientsMenuPanel.SetActive(false);
        hireStaffMenuPanel.SetActive(false);
        unlockDishesMenuPanel.SetActive(false);
        cookDishMenuPanel.SetActive(false);
    }
    // Обновить состояние кнопок в меню разблокировки блюд
    public void UpdateUnlockDishMenu()
    {
        List<Dish> allDishes = databaseManager.GetAllDishes();

        for (int i = 0; i < dishButtons.Length; i++)
        {
            if (i < allDishes.Count)
            {
                Dish dish = allDishes[i];
                int dishId = dish.DishID;

                // Проверяем, можно ли разблокировать блюдо
                bool canUnlock = (dishId == 1) || databaseManager.IsDishUnlocked(dishId - 1);

                // Устанавливаем текст кнопки
                //dishButtons[i].GetComponentInChildren<Text>().text = dish.DishName;

                // Делаем кнопку активной, если блюдо можно разблокировать
                dishButtons[i].interactable = canUnlock && !databaseManager.IsDishUnlocked(dishId);

                // Добавляем обработчик нажатия
                dishButtons[i].onClick.RemoveAllListeners();
                dishButtons[i].onClick.AddListener(() => OnUnlockDishButtonClicked(dishId));
            }
            else
            {
                // Блюдо не существует
                //dishButtons[i].GetComponentInChildren<Text>().text = "Locked";
                dishButtons[i].interactable = false;
            }
        }
    }

    private void OnUnlockDishButtonClicked(int dishIndex)
    {
        if (dishIndex == unlockedDishesCount - 1)
        {
            // Разблокируем следующее блюдо
            unlockedDishesCount++;
            Debug.Log($"Блюдо {dishIndex + 1} разблокировано. Теперь доступно {unlockedDishesCount} блюд.");

            // Обновляем состояние кнопок
            InitializeUnlockDishButtons();
        }
        else
        {
            Debug.LogWarning("Нельзя разблокировать это блюдо, пока не разблокировано предыдущее.");
        }
    }
}