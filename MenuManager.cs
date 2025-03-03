using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static DatabaseManager;

public class MenuManager : MonoBehaviour
{
    // ������ ����
    public GameObject mainMenuPanel;
    public GameObject ingredientsMenuPanel;
    public GameObject hireStaffMenuPanel;
    public GameObject unlockDishesMenuPanel;
    public GameObject cookDishMenuPanel; // ����� ������ ��� ������������� ����
                                         // ������ ��� ������������� ����
    public Button[] unlockDishButtons;
    // ������ ��� ����
    public Button[] dishButtons; // ������ ������ ��� ����
    public DatabaseManager databaseManager; // ������ �� DatabaseManager
    private int unlockedDishesCount = 1; // �������� � 1, ��� ��� ������ ����� ��� ��������������
    void Start()
    {
        // ���������, ��� DatabaseManager ���������������
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("DatabaseManager �� ���������������.");
            return;
        }

        HideAllMenus();
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
    private void InitializeUnlockDishButtons()
    {
        for (int i = 0; i < unlockDishButtons.Length; i++)
        {
            // ������ ����� ������ �������� ��� �������������
            if (i == 0)
            {
                unlockDishButtons[i].interactable = true;
            }
            else
            {
                // ��������� ����� �������� ������ ���� ���������� ����� ��������������
                unlockDishButtons[i].interactable = i < unlockedDishesCount;
            }

            // ��������� ���������� �������
            int dishIndex = i; // ��������� ���������� ��� ���������
            unlockDishButtons[i].onClick.RemoveAllListeners();
            unlockDishButtons[i].onClick.AddListener(() => OnUnlockDishButtonClicked(dishIndex));
        }
    }

    // �������� ������� ����
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        ingredientsMenuPanel.SetActive(false);
        hireStaffMenuPanel.SetActive(false);
        unlockDishesMenuPanel.SetActive(false);
        cookDishMenuPanel.SetActive(false);
    }

    // �������� ���� ������� ������������
    public void ShowIngredientsMenu()
    {
        mainMenuPanel.SetActive(false);
        ingredientsMenuPanel.SetActive(true);
        hireStaffMenuPanel.SetActive(false);
        unlockDishesMenuPanel.SetActive(false);
        cookDishMenuPanel.SetActive(false);
    }

    // �������� ���� ����� ���������
    public void ShowHireStaffMenu()
    {
        mainMenuPanel.SetActive(false);
        ingredientsMenuPanel.SetActive(false);
        hireStaffMenuPanel.SetActive(true);
        unlockDishesMenuPanel.SetActive(false);
        cookDishMenuPanel.SetActive(false);
    }

    // �������� ���� ������������� ����
    public void ShowUnlockDishesMenu()
    {
        mainMenuPanel.SetActive(false);
        ingredientsMenuPanel.SetActive(false);
        hireStaffMenuPanel.SetActive(false);
        unlockDishesMenuPanel.SetActive(true);
        cookDishMenuPanel.SetActive(false);

        // ��������� ��������� ������ �������������
        InitializeUnlockDishButtons();
    }

    // �������� ���� ������������� ����
    public void ShowCookDishMenu()
    {
        mainMenuPanel.SetActive(false);
        ingredientsMenuPanel.SetActive(false);
        hireStaffMenuPanel.SetActive(false);
        unlockDishesMenuPanel.SetActive(false);
        cookDishMenuPanel.SetActive(true);

        UpdateDishMenu(); // ��������� ��������� ������ ��� �������� ����
    }

    // �������� ��������� ������ � ���� ������������� ����
    // �������� ��������� ������ � ���� ������������� ����
    public void UpdateDishMenu()
    {
        List<Dish> unlockedDishes = databaseManager.GetUnlockedDishes();
        Debug.Log($"���� 1");
        for (int i = 0; i < dishButtons.Length; i++)
        {
            if (i < unlockedDishes.Count)
            {
                Debug.Log($"���� 2 +");
                // ����� ��������������
                Dish dish = unlockedDishes[i];
                //dishButtons[i].GetComponentInChildren<Text>().text = dish.DishName;
                dishButtons[i].interactable = true;
            }
            else
            {
                Debug.Log($"���� 2 -");
                // ����� �� �������������� ��� �����������
                //dishButtons[i].GetComponentInChildren<Text>().text = "Locked";
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
                databaseManager.CookDish(selectedDish.DishID);
                Debug.Log($"Cooking {selectedDish.DishName}...");
                ShowMainMenu(); // ������������ � ������� ���� ����� �������������
            }
            else
            {
                Debug.LogWarning("Not enough ingredients to cook this dish.");
            }
        }
    }

    // ������ ��� ���� (���� �����)
    public void HideAllMenus()
    {
        mainMenuPanel.SetActive(false);
        ingredientsMenuPanel.SetActive(false);
        hireStaffMenuPanel.SetActive(false);
        unlockDishesMenuPanel.SetActive(false);
        cookDishMenuPanel.SetActive(false);
    }
    // �������� ��������� ������ � ���� ������������� ����
    public void UpdateUnlockDishMenu()
    {
        List<Dish> allDishes = databaseManager.GetAllDishes();

        for (int i = 0; i < dishButtons.Length; i++)
        {
            if (i < allDishes.Count)
            {
                Dish dish = allDishes[i];
                int dishId = dish.DishID;

                // ���������, ����� �� �������������� �����
                bool canUnlock = (dishId == 1) || databaseManager.IsDishUnlocked(dishId - 1);

                // ������������� ����� ������
                //dishButtons[i].GetComponentInChildren<Text>().text = dish.DishName;

                // ������ ������ ��������, ���� ����� ����� ��������������
                dishButtons[i].interactable = canUnlock && !databaseManager.IsDishUnlocked(dishId);

                // ��������� ���������� �������
                dishButtons[i].onClick.RemoveAllListeners();
                dishButtons[i].onClick.AddListener(() => OnUnlockDishButtonClicked(dishId));
            }
            else
            {
                // ����� �� ����������
                //dishButtons[i].GetComponentInChildren<Text>().text = "Locked";
                dishButtons[i].interactable = false;
            }
        }
    }

    private void OnUnlockDishButtonClicked(int dishIndex)
    {
        if (dishIndex == unlockedDishesCount - 1)
        {
            // ������������ ��������� �����
            unlockedDishesCount++;
            Debug.Log($"����� {dishIndex + 1} ��������������. ������ �������� {unlockedDishesCount} ����.");

            // ��������� ��������� ������
            InitializeUnlockDishButtons();
        }
        else
        {
            Debug.LogWarning("������ �������������� ��� �����, ���� �� �������������� ����������.");
        }
    }
}