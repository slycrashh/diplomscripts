using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static DatabaseManager;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    [System.Serializable]
    public class StaffMember
    {
        public int staffId;
        public float hireCost;
        public bool isHired;
    }

    public StaffMember[] staffMembers; // Заполняем в инспекторе
    [Header("Найм персонала")]
    public Button[] staffButtons; // Массив кнопок найма
    public int[] staffIds; // Соответствующие ID персонала
    [Header("Декорации")]
    [SerializeField] private GameObject decorMenuPanel;
    [SerializeField] private Button[] decorButtons;
    public int[] decorIds; 
    [System.Serializable]
    public class DecorButton
    {
        public Button buttonComponent; // Сам Button
        public GameObject activeState; // Группа "Не куплено"
        public GameObject purchasedState; // Группа "Куплено"
        public int decorId; // Соответствует ID в DatabaseManager
    }
    // Панели меню
    public GameObject mainMenuPanel;
    public GameObject ingredientsMenuPanel;
    public GameObject hireStaffMenuPanel;
    public GameObject unlockDishesMenuPanel;
    public GameObject cookDishMenuPanel; // Панель для приготовления блюд
    public GameObject customerMenuPanel; // Новая панель для меню гостя

    // Кнопки для разблокировки блюд
    public Button[] unlockDishButtons;

    // Кнопки для блюд
    public Button[] dishButtons; // Массив кнопок для блюд

    // Кнопка для выдачи блюда
    public TextMeshProUGUI customerDishText; // Текстовое поле для отображения названия блюда
    public Button serveDishButton; // Кнопка "Выдать"
    public Text customerInfoText; // Текстовое поле для информации о госте

    public DatabaseManager databaseManager; // Ссылка на DatabaseManager
    private int unlockedDishesCount = 1; // Начинаем с 1, так как первое блюдо уже разблокировано
    private int nextAvailableIndex = 0; // Начинаем с 1, так как первое блюдо уже разблокировано

    private CustomerController currentCustomer; // Текущий выбранный гость
    private int currentUnlocked = 1;
    private void Awake()
    {
        // Реализация синглтона
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Уничтожаем дубликаты
        }
    }
    void Start()
    {
        HideAllMenus();
        currentUnlocked = 1;
        
        // Убедитесь, что DatabaseManager инициализирован
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("DatabaseManager не инициализирован.");
            return;
        }

        InitializeDecorMenu();
        nextAvailableIndex = 0; // Начинаем с первого сотрудника
        InitializeHireButtons();
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
         if (serveDishButton != null)
        {
            serveDishButton.onClick.RemoveAllListeners();
            serveDishButton.onClick.AddListener(OnServeDishButtonClicked);
        }
        else
        {
            Debug.LogError("Кнопка 'Выдать' не назначена.");
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

    
    private float CalculateDishUnlockCost(int dishIndex)
    {
       if(dishIndex == 0)
        {
            return 499f;
        }
        if (dishIndex == 1)
        {
            return 1099f;
        }
        if (dishIndex == 2)
        {
            return 299f;
        }
        return 1000000f;
    }

    // Показать главное меню
    public void ShowMainMenu()
    {
        HideAllMenus();
        mainMenuPanel.SetActive(true);
    }

    // Показать меню покупки ингредиентов
    public void ShowIngredientsMenu()
    {
        HideAllMenus();
        ingredientsMenuPanel.SetActive(true);
    }

    // Показать меню найма персонала
    public void ShowHireStaffMenu()
    {
        HideAllMenus();
        hireStaffMenuPanel.SetActive(true);
    }

    // Показать меню разблокировки блюд
    public void ShowUnlockDishesMenu()
    {
        HideAllMenus();
        unlockDishesMenuPanel.SetActive(true);

        // Обновляем состояние кнопок разблокировки
        InitializeUnlockDishButtons();
    }

    // Показать меню приготовления блюд
    public void ShowCookDishMenu()
    {
        HideAllMenus();
        cookDishMenuPanel.SetActive(true);

        UpdateDishMenu(); // Обновляем состояние кнопок при открытии меню
    }

    // Обработчик нажатия на кнопку "Выдать"
    private void OnServeDishButtonClicked()
    {
        if (currentCustomer == null)
        {
            Debug.LogWarning("Гость не выбран.");
            return;
        }

        Dish orderedDish = currentCustomer.GetSelectedDish();
        if (orderedDish == null)
        {
            Debug.LogWarning("Гость не заказал блюдо.");
            return;
        }

        // Получаем текущий запас блюда
        int currentStock = databaseManager.GetDishStockQuantity(orderedDish.DishID);

        if (currentStock <= 0)
        {
            Debug.LogWarning($"Блюдо {orderedDish.DishName} закончилось!");
            // Можно добавить визуальную обратную связь (например, красный текст)
            customerDishText.text = $"Блюдо {orderedDish.DishName} закончилось!";
            customerDishText.color = Color.red;
            return;
        }

        if (databaseManager.GetDishStockQuantity(orderedDish.DishID) > 0)
        {
            databaseManager.DecreaseDishStock(orderedDish.DishID);
            currentCustomer.ReceiveDish(); // Только здесь выдаем блюдо
            HideAllMenus();
        }
    }
    // Показать меню гостя
    public void ShowCustomerMenu(CustomerController customer)
    {
        HideAllMenus();
        customerMenuPanel.SetActive(true);

        currentCustomer = customer;

        // Отображаем название блюда
        if (currentCustomer != null && currentCustomer.GetSelectedDish() != null)
        {
            customerDishText.text = $"Гость заказал: {currentCustomer.GetSelectedDish().DishName}";
        }
        else
        {
            customerDishText.text = "Гость не заказал блюдо.";
        }

        // Настраиваем кнопку "Выдать блюдо"
        serveDishButton.onClick.RemoveAllListeners();
        serveDishButton.onClick.AddListener(OnServeDishButtonClicked);
    }

    // Скрыть все меню
    public void HideAllMenus()
    {
        mainMenuPanel.SetActive(false);
        ingredientsMenuPanel.SetActive(false);
        hireStaffMenuPanel.SetActive(false);
        unlockDishesMenuPanel.SetActive(false);
        cookDishMenuPanel.SetActive(false);
        customerMenuPanel.SetActive(false);
        decorMenuPanel.SetActive(false);
    }

    // Обновить состояние кнопок в меню приготовления блюд
    public void UpdateDishMenu()
    {
        List<Dish> unlockedDishes = databaseManager.GetUnlockedDishes();
        for (int i = 0; i < dishButtons.Length; i++)
        {
            if (i < unlockedDishes.Count)
            {
                Dish dish = unlockedDishes[i];
                dishButtons[i].interactable = true;
            }
            else
            {
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
                if (CookingManager.Instance.StartCooking(
                    selectedDish.DishID,
                    selectedDish.DishName,
                    selectedDish.CookTime))
                {
                    databaseManager.DeductIngredientsForDish(selectedDish.DishID);
                }
                else
                {
                    Debug.Log("Все повара заняты!");
                }
            }
        }
    }
    // Инициализация кнопок для разблокировки блюд
    private void InitializeUnlockDishButtons()
    {
        for (int i = 0; i < unlockDishButtons.Length; i++)
        {
            // Первое блюдо всегда доступно для разблокировки
            if (i == unlockedDishesCount - 1)
            {
                unlockDishButtons[i].interactable = true;
            }
            else
            {
                unlockDishButtons[i].interactable = false;
            }

            // Добавляем обработчик нажатия
            int dishIndex = i; // Локальная переменная для замыкания
            unlockDishButtons[i].onClick.RemoveAllListeners();
            unlockDishButtons[i].onClick.AddListener(() => OnUnlockDishButtonClicked(dishIndex));
        }
    }
    // Обработчик нажатия на кнопку разблокировки блюда
    private void OnUnlockDishButtonClicked(int dishIndex)
    {
        if (dishIndex == unlockedDishesCount - 1)
        {
            float unlockCost = CalculateDishUnlockCost(dishIndex);

            if (DatabaseManager.Instance.GetMoney() >= unlockCost)
            {
                DatabaseManager.Instance.SpendMoney(unlockCost);
                // Разблокируем следующее блюдо
                unlockedDishesCount++;
                Debug.Log($"Блюдо {dishIndex + 1} разблокировано. Теперь доступно {unlockedDishesCount} блюд.");

                // Обновляем состояние кнопок
                InitializeUnlockDishButtons();
            }
        }
        else
        {
            Debug.LogWarning("Нельзя разблокировать это блюдо, пока не разблокировано предыдущее.");
        }
    }

    // Показать/скрыть меню декора
    public void ShowDecorMenu()
    {
        HideAllMenus();
        decorMenuPanel.SetActive(true);
        InitializeDecorMenu();
    }
    public void InitializeDecorMenu()
    {
        for (int i = 0; i < decorButtons.Length; i++)
        {
            int decorId = decorIds[i];
            bool isPurchased = DatabaseManager.Instance.IsDecorPurchased(decorId);

            decorButtons[i].onClick.RemoveAllListeners();
            decorButtons[i].interactable = !isPurchased;

            if (!isPurchased)
            {
                decorButtons[i].onClick.AddListener(() => BuyDecoration(decorId));
            }
        }
    }

    private void BuyDecoration(int decorId)
    {
        if (DatabaseManager.Instance.BuyDecoration(decorId))
        {
            DecorObject[] allDecor = FindObjectsOfType<DecorObject>(true);
            foreach (var decor in allDecor)
            {
                if (decor.decorID == decorId)
                {
                    decor.ShowDecor();
                    break; // Достаточно одного объекта
                }
            }

            // Делаем кнопку неактивной
            for (int i = 0; i < decorIds.Length; i++)
            {
                if (decorIds[i] == decorId)
                {
                    decorButtons[i].interactable = false;
                    break;
                }
            }
        }
    }
    private void InitializeHireButtons()
    {
        for (int i = 0; i < staffButtons.Length; i++)
        {
            bool isInteractable = (i == nextAvailableIndex && !staffMembers[i].isHired);
            staffButtons[i].interactable = isInteractable;

            if (isInteractable)
            {
                int index = i;
                staffButtons[i].onClick.RemoveAllListeners();
                staffButtons[i].onClick.AddListener(() => OnHireClicked(index));
            }
        }
    }
    private void OnHireClicked(int staffIndex)
    {
        StaffMember staff = staffMembers[staffIndex];

        // Только здесь обращаемся к DatabaseManager для проверки денег
        if (DatabaseManager.Instance.GetMoney() >= staff.hireCost)
        {
            //DatabaseManager.Instance.SpendMoney(staff.hireCost); // Списание денег
            staff.isHired = true; // Локально отмечаем как нанятого

            // Ищем следующего доступного сотрудника
            nextAvailableIndex = -1;
            for (int i = 0; i < staffMembers.Length; i++)
            {
                if (!staffMembers[i].isHired)
                {
                    nextAvailableIndex = i;
                    break;
                }
            }

            InitializeHireButtons(); // Обновляем кнопки
        }
        else
        {
            Debug.Log($"Не хватает денег! Нужно: {staff.hireCost}");
        }
    }
    

}