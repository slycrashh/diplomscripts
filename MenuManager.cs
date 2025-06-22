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

    public StaffMember[] staffMembers; // ��������� � ����������
    [Header("���� ���������")]
    public Button[] staffButtons; // ������ ������ �����
    public int[] staffIds; // ��������������� ID ���������
    [Header("���������")]
    [SerializeField] private GameObject decorMenuPanel;
    [SerializeField] private Button[] decorButtons;
    public int[] decorIds; 
    [System.Serializable]
    public class DecorButton
    {
        public Button buttonComponent; // ��� Button
        public GameObject activeState; // ������ "�� �������"
        public GameObject purchasedState; // ������ "�������"
        public int decorId; // ������������� ID � DatabaseManager
    }
    // ������ ����
    public GameObject mainMenuPanel;
    public GameObject ingredientsMenuPanel;
    public GameObject hireStaffMenuPanel;
    public GameObject unlockDishesMenuPanel;
    public GameObject cookDishMenuPanel; // ������ ��� ������������� ����
    public GameObject customerMenuPanel; // ����� ������ ��� ���� �����

    // ������ ��� ������������� ����
    public Button[] unlockDishButtons;

    // ������ ��� ����
    public Button[] dishButtons; // ������ ������ ��� ����

    // ������ ��� ������ �����
    public TextMeshProUGUI customerDishText; // ��������� ���� ��� ����������� �������� �����
    public Button serveDishButton; // ������ "������"
    public Text customerInfoText; // ��������� ���� ��� ���������� � �����

    public DatabaseManager databaseManager; // ������ �� DatabaseManager
    private int unlockedDishesCount = 1; // �������� � 1, ��� ��� ������ ����� ��� ��������������
    private int nextAvailableIndex = 0; // �������� � 1, ��� ��� ������ ����� ��� ��������������

    private CustomerController currentCustomer; // ������� ��������� �����
    private int currentUnlocked = 1;
    private void Awake()
    {
        // ���������� ���������
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // ���������� ���������
        }
    }
    void Start()
    {
        HideAllMenus();
        currentUnlocked = 1;
        
        // ���������, ��� DatabaseManager ���������������
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("DatabaseManager �� ���������������.");
            return;
        }

        InitializeDecorMenu();
        nextAvailableIndex = 0; // �������� � ������� ����������
        InitializeHireButtons();
        InitializeDishButtons();
        InitializeUnlockDishButtons();
        
        // ������������� databaseManager
        databaseManager = DatabaseManager.Instance;
        if (databaseManager == null)
        {
            Debug.LogError("DatabaseManager �� ������.");
        }

        // ������������� dishButtons
        if (dishButtons == null || dishButtons.Length == 0)
        {
            Debug.LogError("������ ���� (dishButtons) �� ����������������.");
        }
         if (serveDishButton != null)
        {
            serveDishButton.onClick.RemoveAllListeners();
            serveDishButton.onClick.AddListener(OnServeDishButtonClicked);
        }
        else
        {
            Debug.LogError("������ '������' �� ���������.");
        }
    }

    // ������������� ������ ��� ����
    private void InitializeDishButtons()
    {
        for (int i = 0; i < dishButtons.Length; i++)
        {
            int dishIndex = i; // ��������� ���������� ��� ���������
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

    // �������� ������� ����
    public void ShowMainMenu()
    {
        HideAllMenus();
        mainMenuPanel.SetActive(true);
    }

    // �������� ���� ������� ������������
    public void ShowIngredientsMenu()
    {
        HideAllMenus();
        ingredientsMenuPanel.SetActive(true);
    }

    // �������� ���� ����� ���������
    public void ShowHireStaffMenu()
    {
        HideAllMenus();
        hireStaffMenuPanel.SetActive(true);
    }

    // �������� ���� ������������� ����
    public void ShowUnlockDishesMenu()
    {
        HideAllMenus();
        unlockDishesMenuPanel.SetActive(true);

        // ��������� ��������� ������ �������������
        InitializeUnlockDishButtons();
    }

    // �������� ���� ������������� ����
    public void ShowCookDishMenu()
    {
        HideAllMenus();
        cookDishMenuPanel.SetActive(true);

        UpdateDishMenu(); // ��������� ��������� ������ ��� �������� ����
    }

    // ���������� ������� �� ������ "������"
    private void OnServeDishButtonClicked()
    {
        if (currentCustomer == null)
        {
            Debug.LogWarning("����� �� ������.");
            return;
        }

        Dish orderedDish = currentCustomer.GetSelectedDish();
        if (orderedDish == null)
        {
            Debug.LogWarning("����� �� ������� �����.");
            return;
        }

        // �������� ������� ����� �����
        int currentStock = databaseManager.GetDishStockQuantity(orderedDish.DishID);

        if (currentStock <= 0)
        {
            Debug.LogWarning($"����� {orderedDish.DishName} �����������!");
            // ����� �������� ���������� �������� ����� (��������, ������� �����)
            customerDishText.text = $"����� {orderedDish.DishName} �����������!";
            customerDishText.color = Color.red;
            return;
        }

        if (databaseManager.GetDishStockQuantity(orderedDish.DishID) > 0)
        {
            databaseManager.DecreaseDishStock(orderedDish.DishID);
            currentCustomer.ReceiveDish(); // ������ ����� ������ �����
            HideAllMenus();
        }
    }
    // �������� ���� �����
    public void ShowCustomerMenu(CustomerController customer)
    {
        HideAllMenus();
        customerMenuPanel.SetActive(true);

        currentCustomer = customer;

        // ���������� �������� �����
        if (currentCustomer != null && currentCustomer.GetSelectedDish() != null)
        {
            customerDishText.text = $"����� �������: {currentCustomer.GetSelectedDish().DishName}";
        }
        else
        {
            customerDishText.text = "����� �� ������� �����.";
        }

        // ����������� ������ "������ �����"
        serveDishButton.onClick.RemoveAllListeners();
        serveDishButton.onClick.AddListener(OnServeDishButtonClicked);
    }

    // ������ ��� ����
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

    // �������� ��������� ������ � ���� ������������� ����
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

    // ���������� ������� �� ������ �����
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
                    Debug.Log("��� ������ ������!");
                }
            }
        }
    }
    // ������������� ������ ��� ������������� ����
    private void InitializeUnlockDishButtons()
    {
        for (int i = 0; i < unlockDishButtons.Length; i++)
        {
            // ������ ����� ������ �������� ��� �������������
            if (i == unlockedDishesCount - 1)
            {
                unlockDishButtons[i].interactable = true;
            }
            else
            {
                unlockDishButtons[i].interactable = false;
            }

            // ��������� ���������� �������
            int dishIndex = i; // ��������� ���������� ��� ���������
            unlockDishButtons[i].onClick.RemoveAllListeners();
            unlockDishButtons[i].onClick.AddListener(() => OnUnlockDishButtonClicked(dishIndex));
        }
    }
    // ���������� ������� �� ������ ������������� �����
    private void OnUnlockDishButtonClicked(int dishIndex)
    {
        if (dishIndex == unlockedDishesCount - 1)
        {
            float unlockCost = CalculateDishUnlockCost(dishIndex);

            if (DatabaseManager.Instance.GetMoney() >= unlockCost)
            {
                DatabaseManager.Instance.SpendMoney(unlockCost);
                // ������������ ��������� �����
                unlockedDishesCount++;
                Debug.Log($"����� {dishIndex + 1} ��������������. ������ �������� {unlockedDishesCount} ����.");

                // ��������� ��������� ������
                InitializeUnlockDishButtons();
            }
        }
        else
        {
            Debug.LogWarning("������ �������������� ��� �����, ���� �� �������������� ����������.");
        }
    }

    // ��������/������ ���� ������
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
                    break; // ���������� ������ �������
                }
            }

            // ������ ������ ����������
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

        // ������ ����� ���������� � DatabaseManager ��� �������� �����
        if (DatabaseManager.Instance.GetMoney() >= staff.hireCost)
        {
            //DatabaseManager.Instance.SpendMoney(staff.hireCost); // �������� �����
            staff.isHired = true; // �������� �������� ��� ��������

            // ���� ���������� ���������� ����������
            nextAvailableIndex = -1;
            for (int i = 0; i < staffMembers.Length; i++)
            {
                if (!staffMembers[i].isHired)
                {
                    nextAvailableIndex = i;
                    break;
                }
            }

            InitializeHireButtons(); // ��������� ������
        }
        else
        {
            Debug.Log($"�� ������� �����! �����: {staff.hireCost}");
        }
    }
    

}