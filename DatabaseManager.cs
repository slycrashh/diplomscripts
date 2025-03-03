using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour
{
    private List<int> unlockedDishes = new List<int>();
    public static DatabaseManager Instance { get; private set; }
    private string connectionString;
    private float money = 10000f; // ���������� ��� ������������ �����
    private static readonly object _dbLock = new object(); // ��� ������������� �������

    void Start()
    {
        string dbPath = Application.persistentDataPath + "/RestaurantSimulator12345.db";
        connectionString = $"URI=file:{dbPath}";

        CreateDatabase();
        ClearDatabase();
        InitializeData();
    }
    private void Awake()
    {
        // ���������� ���������
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ��������� ������ ����� �������
        }
        else
        {
            Destroy(gameObject); // ���������� ���������
        }
        unlockedDishes.Add(1); // ID ������� �����
    }
    /// <summary>
    /// �������� ���� ������ � ������
    /// </summary>
    private void CreateDatabase()
    {
        lock (_dbLock)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var commands = new[]
                        {
                            "CREATE TABLE IF NOT EXISTS Dishes (DishID INTEGER PRIMARY KEY AUTOINCREMENT, DishName TEXT NOT NULL, Price REAL NOT NULL, CookTime INTEGER NOT NULL, IsUnlocked BOOLEAN NOT NULL DEFAULT 1)",
                            "CREATE TABLE IF NOT EXISTS Ingredients (IngredientID INTEGER PRIMARY KEY AUTOINCREMENT, IngredientName TEXT NOT NULL, Cost REAL NOT NULL)",
                            "CREATE TABLE IF NOT EXISTS DishIngredients (DishID INTEGER NOT NULL, IngredientID INTEGER NOT NULL, Quantity INTEGER NOT NULL, PRIMARY KEY (DishID, IngredientID), FOREIGN KEY (DishID) REFERENCES Dishes(DishID), FOREIGN KEY (IngredientID) REFERENCES Ingredients(IngredientID))",
                            "CREATE TABLE IF NOT EXISTS Employees (EmployeeID INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Role TEXT NOT NULL, SpeedModifier REAL NOT NULL, HireCost REAL NOT NULL, IsHired BOOLEAN NOT NULL DEFAULT 0)",
                            "CREATE TABLE IF NOT EXISTS Inventory (IngredientID INTEGER PRIMARY KEY, Quantity INTEGER NOT NULL DEFAULT 0, FOREIGN KEY (IngredientID) REFERENCES Ingredients(IngredientID))",
                            "CREATE TABLE IF NOT EXISTS DishStock (DishID INTEGER PRIMARY KEY, StockQuantity INTEGER NOT NULL DEFAULT 0, FOREIGN KEY (DishID) REFERENCES Dishes(DishID))"
                        };

                        foreach (var commandText in commands)
                        {
                            using (var cmd = new SqliteCommand(commandText, connection, transaction))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Debug.LogError($"������ ��� �������� ���� ������: {ex.Message}");
                    }
                }
                connection.Close();
            }
        }
    }

    /// <summary>
    /// ������ ������� ������
    /// </summary>
    private void ClearDatabase()
    {
        lock (_dbLock)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var tablesToClear = new[]
                        {
                            "DELETE FROM DishStock",
                            "DELETE FROM Inventory",
                            "DELETE FROM DishIngredients",
                            "DELETE FROM Dishes",
                            "DELETE FROM Ingredients",
                            "DELETE FROM Employees"
                        };

                        foreach (var commandText in tablesToClear)
                        {
                            using (var cmd = new SqliteCommand(commandText, connection, transaction))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }

                        using (var resetCmd = new SqliteCommand("DELETE FROM sqlite_sequence", connection, transaction))
                        {
                            resetCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Debug.LogError($"������ ��� ������� ���� ������: {ex.Message}");
                    }
                }
                connection.Close();
            }
        }
    }

    /// <summary>
    /// ������������� ������
    /// </summary>
    private void InitializeData()
    {
        lock (_dbLock)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // ���������� ���������� ��������� ����
                        AddDish(connection, transaction, "�����", 10.99f, 1, true);
                        AddDish(connection, transaction, "����� ������", 8.99f, 120, true);
                        AddDish(connection, transaction, "���������", 5.49f, 60, true);

                        // ���������� ����, ������� ����������� �� ������
                        AddDish(connection, transaction, "�����", 15.99f, 600, false);
                        AddDish(connection, transaction, "�����", 25.99f, 900, false);
                        AddDish(connection, transaction, "������", 7.99f, 180, false);

                        // ���������� ������������
                        AddIngredient(connection, transaction, "�����", 0.50f);
                        AddIngredient(connection, transaction, "���", 1.00f);
                        AddIngredient(connection, transaction, "����", 0.20f);
                        AddIngredient(connection, transaction, "������", 2.00f);
                        AddIngredient(connection, transaction, "��������", 3.50f);
                        AddIngredient(connection, transaction, "�������", 1.50f);

                        // ���������� ������ ����� ������� � �������������
                        AddDishIngredient(connection, transaction, 1, 1, 2); // �����: 2 ������
                        AddDishIngredient(connection, transaction, 1, 2, 1); // �����: 1 ���
                        AddDishIngredient(connection, transaction, 2, 2, 1); // ����� ������: 1 ���
                        AddDishIngredient(connection, transaction, 2, 4, 1); // ����� ������: 1 ������
                        AddDishIngredient(connection, transaction, 3, 3, 1); // ���������: 1 ����
                        AddDishIngredient(connection, transaction, 4, 1, 3); // �����: 3 ������
                        AddDishIngredient(connection, transaction, 4, 2, 2); // �����: 2 ����
                        AddDishIngredient(connection, transaction, 5, 5, 1); // �����: 1 ��������
                        AddDishIngredient(connection, transaction, 6, 6, 1); // ������: 1 �������

                        // ���������� ����������� � ��������� ��������� �����
                        AddEmployee(connection, transaction, "�����", "�����", 1.2f, 30f, 0); // IsHired = 0
                        AddEmployee(connection, transaction, "���", "��������", 1.0f, 20f, 0); // IsHired = 0
                        AddEmployee(connection, transaction, "�����", "�����������", 1.5f, 50f, 0); // IsHired = 0

                        // ������������� ���������
                        InitializeInventory(connection, transaction);

                        // ������������� ������ ����
                        InitializeDishStock(connection, transaction);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Debug.LogError($"������ ��� ������������� ������: {ex.Message}");
                    }
                }
                connection.Close();
            }
        }
    }

    /// <summary>
    /// ���������� ������ �����
    /// </summary>
    private void AddDish(SqliteConnection connection, SqliteTransaction transaction, string dishName, float price, int cookTime, bool isUnlocked)
    {
        using (var cmd = new SqliteCommand("INSERT INTO Dishes (DishName, Price, CookTime, IsUnlocked) VALUES (@dishName, @price, @cookTime, @isUnlocked)", connection, transaction))
        {
            cmd.Parameters.AddWithValue("@dishName", dishName);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.Parameters.AddWithValue("@cookTime", cookTime);
            cmd.Parameters.AddWithValue("@isUnlocked", isUnlocked ? 1 : 0);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// ���������� ������ �����������
    /// </summary>
    private void AddIngredient(SqliteConnection connection, SqliteTransaction transaction, string ingredientName, float cost)
    {
        using (var cmd = new SqliteCommand("INSERT INTO Ingredients (IngredientName, Cost) VALUES (@ingredientName, @cost)", connection, transaction))
        {
            cmd.Parameters.AddWithValue("@ingredientName", ingredientName);
            cmd.Parameters.AddWithValue("@cost", cost);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// ���������� ����� ����� ������ � ������������
    /// </summary>
    private void AddDishIngredient(SqliteConnection connection, SqliteTransaction transaction, int dishId, int ingredientId, int quantity)
    {
        using (var cmd = new SqliteCommand("INSERT INTO DishIngredients (DishID, IngredientID, Quantity) VALUES (@dishId, @ingredientId, @quantity)", connection, transaction))
        {
            cmd.Parameters.AddWithValue("@dishId", dishId);
            cmd.Parameters.AddWithValue("@ingredientId", ingredientId);
            cmd.Parameters.AddWithValue("@quantity", quantity);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// ���������� ���������� � ��������� ��������� �����
    /// </summary>
    private void AddEmployee(SqliteConnection connection, SqliteTransaction transaction, string name, string role, float speedModifier, float hireCost, int isHired)
    {
        using (var cmd = new SqliteCommand("INSERT INTO Employees (Name, Role, SpeedModifier, HireCost, IsHired) VALUES (@name, @role, @speedModifier, @hireCost, @isHired)", connection, transaction))
        {
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@role", role);
            cmd.Parameters.AddWithValue("@speedModifier", speedModifier);
            cmd.Parameters.AddWithValue("@hireCost", hireCost);
            cmd.Parameters.AddWithValue("@isHired", isHired);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// ������������� ���������
    /// </summary>
    private void InitializeInventory(SqliteConnection connection, SqliteTransaction transaction)
    {
        using (var cmd = new SqliteCommand("SELECT IngredientID FROM Ingredients", connection, transaction))
        {
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int ingredientId = reader.GetInt32(0);
                    AddIngredientToInventory(connection, transaction, ingredientId, 0); // ��������� ���������� = 0
                }
            }
        }
    }

    /// <summary>
    /// ������������� ������ ����
    /// </summary>
    private void InitializeDishStock(SqliteConnection connection, SqliteTransaction transaction)
    {
        using (var cmd = new SqliteCommand("SELECT DishID FROM Dishes", connection, transaction))
        {
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int dishId = reader.GetInt32(0);
                    AddDishToStock(connection, transaction, dishId, 0); // ��������� ���������� = 0
                }
            }
        }
    }

    /// <summary>
    /// ���������� ����������� � ���������
    /// </summary>
    private void AddIngredientToInventory(SqliteConnection connection, SqliteTransaction transaction, int ingredientId, int quantity)
    {
        using (var cmd = new SqliteCommand("INSERT INTO Inventory (IngredientID, Quantity) VALUES (@ingredientId, @quantity)", connection, transaction))
        {
            cmd.Parameters.AddWithValue("@ingredientId", ingredientId);
            cmd.Parameters.AddWithValue("@quantity", quantity);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// ���������� ����� � DishStock
    /// </summary>
    private void AddDishToStock(SqliteConnection connection, SqliteTransaction transaction, int dishId, int quantity)
    {
        using (var cmd = new SqliteCommand("INSERT INTO DishStock (DishID, StockQuantity) VALUES (@dishId, @quantity)", connection, transaction))
        {
            cmd.Parameters.AddWithValue("@dishId", dishId);
            cmd.Parameters.AddWithValue("@quantity", quantity);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// ������� ����������� (���������� ������ ����� 1)
    /// </summary>
    public void BuyIngredient(int ingredientId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            // �������� ��������� �����������
            using (var cmd = new SqliteCommand("SELECT Cost FROM Ingredients WHERE IngredientID = @ingredientId", connection))
            {
                cmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    float cost = Convert.ToSingle(result);

                    // ���������, ���������� �� �����
                    if (SpendMoney(cost))
                    {
                        // ��������� ���������� � ���������
                        UpdateIngredientQuantity(ingredientId, GetIngredientQuantity(ingredientId) + 1);
                        Debug.Log($"������ 1 ������� ����������� � ID {ingredientId}. ������� �����: {money}");
                    }
                    else
                    {
                        Debug.LogWarning("������������ ����� ��� ������� �����������.");
                    }
                }
                else
                {
                    Debug.LogError($"���������� � ID {ingredientId} �� ������.");
                }
            }

            connection.Close();
        }
    }

    /// <summary>
    /// �������� ����� (��������� = Price + 100)
    /// </summary>
    public void UnlockDish(int dishId)
    {
        // ���������, �������������� �� ���������� �����
        if (dishId > 1 && !IsDishUnlocked(dishId - 1))
        {
            Debug.LogWarning($"������ �������������� ����� � ID {dishId}, ���� �� �������������� ����� � ID {dishId - 1}.");
            return;
        }

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            // �������� ���� �����
            using (var cmd = new SqliteCommand("SELECT Price FROM Dishes WHERE DishID = @dishId", connection))
            {
                cmd.Parameters.AddWithValue("@dishId", dishId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    float unlockCost = Convert.ToSingle(result) + 100f; // ��������� = Price + 100

                    // ���������, ���������� �� �����
                    if (SpendMoney(unlockCost))
                    {
                        // ������������ �����
                        using (var updateCmd = new SqliteCommand("UPDATE Dishes SET IsUnlocked = 1 WHERE DishID = @dishId", connection))
                        {
                            updateCmd.Parameters.AddWithValue("@dishId", dishId);
                            int rowsAffected = updateCmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                Debug.Log($"����� � ID {dishId} ������� ��������������!");
                            }
                            else
                            {
                                Debug.LogError($"����� � ID {dishId} �� ���� �������������� (��������, ID �� ������).");
                            }
                        }

                        // ���������� MenuManager � ������������� �����
                        MenuManager menuManager = FindObjectOfType<MenuManager>();
                        if (menuManager != null)
                        {
                            menuManager.UpdateDishMenu();
                        }
                        else
                        {
                            Debug.LogError("MenuManager �� ������ �� �����.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("������������ ����� ��� ������������� �����.");
                    }
                }
                else
                {
                    Debug.LogError($"����� � ID {dishId} �� �������.");
                }
            }

            connection.Close();
        }
    }


    /// <summary>
    /// �������� ��� �����
    /// </summary>
    public List<Dish> GetAllDishes()
    {
        List<Dish> allDishes = new List<Dish>();

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("SELECT DishID, DishName, Price, CookTime, IsUnlocked FROM Dishes", connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        allDishes.Add(new Dish
                        {
                            DishID = reader.GetInt32(0),
                            DishName = reader.GetString(1),
                            Price = reader.GetFloat(2),
                            CookTime = reader.GetInt32(3),
                            IsUnlocked = reader.GetBoolean(4)
                        });
                    }
                }
            }
            connection.Close();
        }

        return allDishes;
    }

    /// <summary>
    /// ��������, �������������� �� �����
    /// </summary>
    public bool IsDishUnlocked(int dishId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            using (var cmd = new SqliteCommand("SELECT IsUnlocked FROM Dishes WHERE DishID = @dishId", connection))
            {
                cmd.Parameters.AddWithValue("@dishId", dishId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result) == 1;
                }
            }

            connection.Close();
        }

        return false;
    }

    /// <summary>
    /// ���� ����������
    /// </summary>
    public void HireEmployee(int employeeId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            // �������� ��������� ����� ����������
            using (var cmd = new SqliteCommand("SELECT HireCost FROM Employees WHERE EmployeeID = @employeeId", connection))
            {
                cmd.Parameters.AddWithValue("@employeeId", employeeId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    float hireCost = Convert.ToSingle(result);

                    // ���������, ���������� �� �����
                    if (SpendMoney(hireCost))
                    {
                        // �������� ����������
                        using (var updateCmd = new SqliteCommand("UPDATE Employees SET IsHired = 1 WHERE EmployeeID = @employeeId", connection))
                        {
                            updateCmd.Parameters.AddWithValue("@employeeId", employeeId);
                            updateCmd.ExecuteNonQuery();
                        }

                        Debug.Log($"��������� � ID {employeeId} ������� �����!");
                    }
                    else
                    {
                        Debug.LogWarning("������������ ����� ��� ����� ����������.");
                    }
                }
                else
                {
                    Debug.LogError($"��������� � ID {employeeId} �� ������.");
                }
            }

            connection.Close();
        }
    }

    /// <summary>
    /// ������������� �����
    /// </summary>
    public void CookDish(int dishId)
    {
        if (CanCookDish(dishId))
        {
            DeductIngredientsForDish(dishId); // �������� �����������
            IncreaseDishStock(dishId); // ����������� ����� �����
            Debug.Log($"����� � ID {dishId} ������� ������������. ������� �����: {GetDishStockQuantity(dishId)}");

            // ���������� ������� � ���������� �����
            NotifyCustomerDishReady(dishId);
        }
        else
        {
            Debug.LogWarning($"������������ ������������ ��� ������������� ����� � ID {dishId}");
        }
    }

    /// <summary>
    /// ����������� ������� � ���������� �����
    /// </summary>
    private void NotifyCustomerDishReady(int dishId)
    {
        var customers = FindObjectsOfType<CustomerController>();
        foreach (var customer in customers)
        {
            if (customer.GetSelectedDishId() == dishId)
            {
                customer.OnDishReady();
                break; // ���������� ������ ������ �����
            }
        }
    }

    /// <summary>
    /// ��������� �������� ���������� �����
    /// </summary>
    public float GetMoney()
    {
        return money;
    }

    /// <summary>
    /// ���������� �����
    /// </summary>
    public void AddMoney(float amount)
    {
        money += amount;
        Debug.Log($"��������� {amount} �����. ������� ������: {money}");
    }

    /// <summary>
    /// ����� �����
    /// </summary>
    /// <returns>True, ���� ����� ����������, ����� False</returns>
    public bool SpendMoney(float amount)
    {
        if (money >= amount)
        {
            money -= amount;
            Debug.Log($"��������� {amount} �����. ������� ������: {money}");
            return true;
        }
        else
        {
            Debug.LogWarning("������������ �����!");
            return false;
        }
    }

    /// <summary>
    /// �������� ������� ������������ ��� ������������� �����
    /// </summary>
    public bool CanCookDish(int dishId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            using (var cmd = new SqliteCommand(@"
                SELECT COUNT(*)
                FROM DishIngredients DI
                JOIN Inventory Inv ON DI.IngredientID = Inv.IngredientID
                WHERE DI.DishID = @dishId AND Inv.Quantity < DI.Quantity", connection))
            {
                cmd.Parameters.AddWithValue("@dishId", dishId);
                int missingIngredientsCount = Convert.ToInt32(cmd.ExecuteScalar());
                return missingIngredientsCount == 0;
            }
        }
    }

    /// <summary>
    /// ��������� ������������ ��� ������������� �����
    /// </summary>
    private void DeductIngredientsForDish(int dishId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            // �������� ������ ������������ � �� ���������� ��� ������� �����
            using (var cmd = new SqliteCommand(@"
            SELECT IngredientID, Quantity
            FROM DishIngredients
            WHERE DishID = @dishId", connection))
            {
                cmd.Parameters.AddWithValue("@dishId", dishId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int ingredientId = reader.GetInt32(0);
                        int quantity = reader.GetInt32(1);

                        // ��������� ���������� ����������� � ���������
                        using (var updateCmd = new SqliteCommand(@"
                        UPDATE Inventory
                        SET Quantity = Quantity - @quantity
                        WHERE IngredientID = @ingredientId", connection))
                        {
                            updateCmd.Parameters.AddWithValue("@quantity", quantity);
                            updateCmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            connection.Close();
        }
    }

    /// <summary>
    /// ���������� ���������� ����� � DishStock
    /// </summary>
    private void IncreaseDishStock(int dishId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("UPDATE DishStock SET StockQuantity = StockQuantity + 1 WHERE DishID = @dishId", connection))
            {
                cmd.Parameters.AddWithValue("@dishId", dishId);
                cmd.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    /// <summary>
    /// ���������� ���������� ����� � DishStock
    /// </summary>
    public void DecreaseDishStock(int dishId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("UPDATE DishStock SET StockQuantity = MAX(StockQuantity - 1, 0) WHERE DishID = @dishId", connection))
            {
                cmd.Parameters.AddWithValue("@dishId", dishId);
                cmd.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    /// <summary>
    /// ��������� ���������� ����������� � ���������
    /// </summary>
    private int GetIngredientQuantity(int ingredientId)
    {
        int quantity = 0;

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("SELECT Quantity FROM Inventory WHERE IngredientID = @ingredientId", connection))
            {
                cmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    quantity = Convert.ToInt32(result);
                }
            }
            connection.Close();
        }

        return quantity;
    }

    /// <summary>
    /// ���������� ���������� ����������� � ���������
    /// </summary>
    private void UpdateIngredientQuantity(int ingredientId, int newQuantity)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("UPDATE Inventory SET Quantity = @newQuantity WHERE IngredientID = @ingredientId", connection))
            {
                cmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                cmd.Parameters.AddWithValue("@newQuantity", newQuantity);
                cmd.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    /// <summary>
    /// ���������� ����������� � ���������
    /// </summary>
    private void AddIngredientToInventory(int ingredientId, int quantity)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("INSERT INTO Inventory (IngredientID, Quantity) VALUES (@ingredientId, @quantity)", connection))
            {
                cmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    /// <summary>
    /// ���������� ����� � DishStock
    /// </summary>
    private void AddDishToStock(int dishId, int quantity)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("INSERT INTO DishStock (DishID, StockQuantity) VALUES (@dishId, @quantity)", connection))
            {
                cmd.Parameters.AddWithValue("@dishId", dishId);
                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    /// <summary>
    /// ��������� �������� ������ �����
    /// </summary>
    public int GetDishStockQuantity(int dishId)
    {
        int stockQuantity = 0;

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("SELECT StockQuantity FROM DishStock WHERE DishID = @dishId", connection))
            {
                cmd.Parameters.AddWithValue("@dishId", dishId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    stockQuantity = Convert.ToInt32(result);
                }
            }
            connection.Close();
        }

        return stockQuantity;
    }
    public List<Dish> GetUnlockedDishes()
    {
        List<Dish> unlockedDishes = new List<Dish>();

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("SELECT DishID, DishName, Price, CookTime FROM Dishes WHERE IsUnlocked = 1", connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        unlockedDishes.Add(new Dish
                        {
                            DishID = reader.GetInt32(0),
                            DishName = reader.GetString(1),
                            Price = reader.GetFloat(2),
                            CookTime = reader.GetInt32(3)
                        });
                    }
                }
            }
            connection.Close();
        }

        Debug.Log($"� ���� ������ ������� {unlockedDishes.Count} ���������������� ����.");
        return unlockedDishes;
    }
    public class Dish
    {
        public int DishID { get; set; }
        public string DishName { get; set; }
        public float Price { get; set; }
        public int CookTime { get; set; }
        public bool IsUnlocked { get; set; }
    }
}