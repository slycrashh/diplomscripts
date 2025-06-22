using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

public class DatabaseManager : MonoBehaviour
{
    private List<int> unlockedDishes = new List<int>();
    public static DatabaseManager Instance { get; private set; }
    private string connectionString;
    private float money = 10000f; // Переменная для отслеживания денег
    private static readonly object _dbLock = new object(); // Для синхронизации доступа
    private List<Decoration> decorations = new List<Decoration>();

    private void InitializeDecor()
    {
        decorations.Clear(); // Очищаем на всякий случай

        // Добавляем декорации
        decorations.Add(new Decoration
        {
            DecorationID = 1,
            Name = "Картина",
            Price = 500,
            IncomeMultiplier = 1.1f,
            IsPurchased = false,
            PrefabName = "Painting"
        });

        decorations.Add(new Decoration
        {
            DecorationID = 2,
            Name = "Тумба",
            Price = 800,
            IncomeMultiplier = 1.15f,
            IsPurchased = false,
            PrefabName = "Cabinet"
        });

        decorations.Add(new Decoration
        {
            DecorationID = 3,
            Name = "Шкаф",
            Price = 1200,
            IncomeMultiplier = 1.2f,
            IsPurchased = false,
            PrefabName = "Wardrobe"
        });
    }
    private void ResetDecorations()
    {
        foreach (var decor in decorations)
        {
            decor.IsPurchased = false;
        }
    }

    void Start()
    {
        string dbPath = Application.persistentDataPath + "/RestaurantSimulator12345.db";
        connectionString = $"URI=file:{dbPath}";

        CreateDatabase();
        ClearDatabase();
        InitializeData();
        ResetDecorations();
    }
    private void Awake()
    {
        // Реализация синглтона
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Сохраняем объект между сценами
            InitializeDecor();
        }
        else
        {
            Destroy(gameObject); // Уничтожаем дубликаты
        }
        unlockedDishes.Add(1); // ID первого блюда
    }
    /// <summary>
    /// Создание базы данных и таблиц
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
                        Debug.LogError($"Ошибка при создании базы данных: {ex.Message}");
                    }
                }
                connection.Close();
            }
        }
    }

    /// <summary>
    /// Полная очистка данных
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
                        Debug.LogError($"Ошибка при очистке базы данных: {ex.Message}");
                    }
                }
                connection.Close();
            }
        }
    }

    /// <summary>
    /// Инициализация данных
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
                        // Добавление изначально доступных блюд
                        AddDish(connection, transaction, "Паста", 1099f, 30, true);
                        AddDish(connection, transaction, "Салат Цезарь", 899f, 45, true);
                        AddDish(connection, transaction, "Бутерброд", 549f, 60, true);

                        // Добавление блюд, которые открываются за деньги
                        AddDish(connection, transaction, "Пицца", 999f, 90, false);
                        AddDish(connection, transaction, "Стейк", 1599f, 100, false);
                        AddDish(connection, transaction, "Десерт", 799f, 125, false);

                        // Добавление ингредиентов
                        AddIngredient(connection, transaction, "Томат", 50f);
                        AddIngredient(connection, transaction, "Сыр", 100f);
                        AddIngredient(connection, transaction, "Хлеб", 20f);
                        AddIngredient(connection, transaction, "Курица", 200f);
                        AddIngredient(connection, transaction, "Говядина", 350f);
                        AddIngredient(connection, transaction, "Шоколад", 150f);

                        // Добавление связей между блюдами и ингредиентами
                        AddDishIngredient(connection, transaction, 1, 1, 2); // Паста: 2 Томата
                        AddDishIngredient(connection, transaction, 1, 2, 1); // Паста: 1 Сыр
                        AddDishIngredient(connection, transaction, 2, 2, 1); // Салат Цезарь: 1 Сыр
                        AddDishIngredient(connection, transaction, 2, 4, 1); // Салат Цезарь: 1 Курица
                        AddDishIngredient(connection, transaction, 3, 3, 1); // Бутерброд: 1 Хлеб
                        AddDishIngredient(connection, transaction, 4, 1, 3); // Пицца: 3 Томата
                        AddDishIngredient(connection, transaction, 4, 2, 2); // Пицца: 2 Сыра
                        AddDishIngredient(connection, transaction, 5, 5, 1); // Стейк: 1 Говядина
                        AddDishIngredient(connection, transaction, 6, 6, 1); // Десерт: 1 Шоколад

                        // Добавление сотрудников с указанием стоимости найма
                        AddEmployee(connection, transaction, "Алиса", "Повар", 1.2f, 100f, 0); // IsHired = 0
                        AddEmployee(connection, transaction, "Боб", "Официант", 1.0f, 200f, 0); // IsHired = 0
                        AddEmployee(connection, transaction, "Кэрол", "Управляющий", 1.5f, 500f, 0); // IsHired = 0

                        // Инициализация инвентаря
                        InitializeInventory(connection, transaction);

                        // Инициализация запаса блюд
                        InitializeDishStock(connection, transaction);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Debug.LogError($"Ошибка при инициализации данных: {ex.Message}");
                    }
                }
                connection.Close();
            }
        }
    }

    /// <summary>
    /// Добавление нового блюда
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
    /// Добавление нового ингредиента
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
    /// Добавление связи между блюдом и ингредиентом
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
    /// Добавление сотрудника с указанием стоимости найма
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
    /// Инициализация инвентаря
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
                    AddIngredientToInventory(connection, transaction, ingredientId, 0); // Начальное количество = 0
                }
            }
        }
    }

    /// <summary>
    /// Инициализация запаса блюд
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
                    AddDishToStock(connection, transaction, dishId, 0); // Начальное количество = 0
                }
            }
        }
    }

    /// <summary>
    /// Добавление ингредиента в инвентарь
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
    /// Добавление блюда в DishStock
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
    /// Покупка ингредиента (количество всегда равно 1)
    /// </summary>
    public void BuyIngredient(int ingredientId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            // Получаем стоимость ингредиента
            using (var cmd = new SqliteCommand("SELECT Cost FROM Ingredients WHERE IngredientID = @ingredientId", connection))
            {
                cmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    float cost = Convert.ToSingle(result);

                    // Проверяем, достаточно ли денег
                    if (SpendMoney(cost))
                    {
                        // Добавляем ингредиент в инвентарь
                        UpdateIngredientQuantity(ingredientId, GetIngredientQuantity(ingredientId) + 1);
                        Debug.Log($"Куплен 1 единица ингредиента с ID {ingredientId}. Остаток денег: {money}");
                    }
                    else
                    {
                        Debug.LogWarning("Недостаточно денег для покупки ингредиента.");
                    }
                }
                else
                {
                    Debug.LogError($"Ингредиент с ID {ingredientId} не найден.");
                }
            }

            connection.Close();
        }
    }
    /// <summary>
    /// Проверяет, можно ли потратить указанную сумму
    /// </summary>
    public bool CanSpendMoney(float amount)
    {
        return money >= amount;
    }
    /// <summary>
    /// Получает информацию о сотруднике (для проверки найма)
    /// </summary>
    public bool IsEmployeeHired(int employeeId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("SELECT IsHired FROM Employees WHERE EmployeeID = @employeeId", connection))
            {
                cmd.Parameters.AddWithValue("@employeeId", employeeId);
                object result = cmd.ExecuteScalar();
                return result != null && Convert.ToInt32(result) == 1;
            }
        }
    }
    public float GetEmployeeHireCost(int employeeId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var cmd = new SqliteCommand("SELECT HireCost FROM Employees WHERE EmployeeID = @employeeId", connection))
            {
                cmd.Parameters.AddWithValue("@employeeId", employeeId);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToSingle(result) : 0f;
            }
        }
    }
    /// <summary>
    /// Открытие блюда (стоимость = Price + 100)
    /// </summary>
    public void UnlockDish(int dishId)
    {
        // Проверяем, разблокировано ли предыдущее блюдо
        if (dishId > 1 && !IsDishUnlocked(dishId - 1))
        {
            Debug.LogWarning($"Нельзя разблокировать блюдо с ID {dishId}, пока не разблокировано блюдо с ID {dishId - 1}.");
            return;
        }

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            // Получаем цену блюда
            using (var cmd = new SqliteCommand("SELECT Price FROM Dishes WHERE DishID = @dishId", connection))
            {
                cmd.Parameters.AddWithValue("@dishId", dishId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    float unlockCost = Convert.ToSingle(result) - 400; 

                    // Проверяем, достаточно ли денег
                    if (true)
                    {
                        // Разблокируем блюдо
                        using (var updateCmd = new SqliteCommand("UPDATE Dishes SET IsUnlocked = 1 WHERE DishID = @dishId", connection))
                        {
                            updateCmd.Parameters.AddWithValue("@dishId", dishId);
                            int rowsAffected = updateCmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                Debug.Log($"Блюдо с ID {dishId} успешно разблокировано!");
                            }
                            else
                            {
                                Debug.LogError($"Блюдо с ID {dishId} не было разблокировано (возможно, ID не найден).");
                            }
                        }

                        // Уведомляем MenuManager о разблокировке блюда
                        MenuManager menuManager = FindObjectOfType<MenuManager>();
                        if (menuManager != null)
                        {
                            menuManager.UpdateDishMenu();
                        }
                        else
                        {
                            Debug.LogError("MenuManager не найден на сцене.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Недостаточно денег для разблокировки блюда.");
                    }
                }
                else
                {
                    Debug.LogError($"Блюдо с ID {dishId} не найдено.");
                }
            }

            connection.Close();
        }
    }


    /// <summary>
    /// Получить все блюда
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
    /// Проверка, разблокировано ли блюдо
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
    /// Найм сотрудника
    /// </summary>
    public void HireEmployee(int employeeId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            // Получаем стоимость найма сотрудника
            using (var cmd = new SqliteCommand("SELECT HireCost FROM Employees WHERE EmployeeID = @employeeId", connection))
            {
                cmd.Parameters.AddWithValue("@employeeId", employeeId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    float hireCost = Convert.ToSingle(result);

                    // Проверяем, достаточно ли денег
                    if (SpendMoney(hireCost))
                    {
                        // Нанимаем сотрудника
                        using (var updateCmd = new SqliteCommand("UPDATE Employees SET IsHired = 1 WHERE EmployeeID = @employeeId", connection))
                        {
                            updateCmd.Parameters.AddWithValue("@employeeId", employeeId);
                            updateCmd.ExecuteNonQuery();
                        }
                        Debug.Log($"Сотрудник с ID {employeeId} успешно нанят!");
                        UpdateSpeedBonusDisplay(); // Обновляем отображение бонуса
                        CookingManager.Instance.UpdateCookingCapacity();
                    }
                    else
                    {
                        Debug.LogWarning("Недостаточно денег для найма сотрудника.");
                    }
                }
                else
                {
                    Debug.LogError($"Сотрудник с ID {employeeId} не найден.");
                }
            }

            connection.Close();
        }
    }
    private void UpdateSpeedBonusDisplay()
    {
        float bonus = (1f - DatabaseManager.Instance.GetCookingSpeedMultiplier()) * 100f;
        
    }
    

    /// <summary>
    /// Приготовление блюда
    /// </summary>
    public void CookDish(int dishId)
    {
        if (CanCookDish(dishId))
        {
            DeductIngredientsForDish(dishId); // Вычитаем ингредиенты
            IncreaseDishStock(dishId); // Увеличиваем запас блюда
            Debug.Log($"Блюдо с ID {dishId} успешно приготовлено. Текущий запас: {GetDishStockQuantity(dishId)}");
        }
        else
        {
            Debug.LogWarning($"Недостаточно ингредиентов для приготовления блюда с ID {dishId}");
        }
    }

    /// <summary>
    /// Уведомление клиента о готовности блюда
    /// </summary>
    private void NotifyCustomerDishReady(int dishId)
    {
        var customers = FindObjectsOfType<CustomerController>();
        foreach (var customer in customers)
        {
            if (customer.GetSelectedDishId() == dishId)
            {
                customer.OnDishReady();
                break; // Уведомляем только одного гостя
            }
        }
    }

    /// <summary>
    /// Получение текущего количества денег
    /// </summary>
    public float GetMoney()
    {
        return money;
    }

    /// <summary>
    /// Добавление денег
    /// </summary>
    public void AddMoney(float amount)
    {
        MoneyUI moneyUI = FindObjectOfType<MoneyUI>();
        float multiplier = GetIncomeMultiplier();
        float finalAmount = amount * multiplier;
        moneyUI.ShowMoneyChange(finalAmount);
        money += finalAmount;
        Debug.Log($"Добавлено {finalAmount} (база: {amount}, множитель: x{multiplier})");
    }

    /// <summary>
    /// Траты денег
    /// </summary>
    /// <returns>True, если денег достаточно, иначе False</returns>
    public bool SpendMoney(float amount)
    {
        if (money >= amount)
        {
            money -= amount;
            Debug.Log($"Потрачено {amount} денег. Текущие деньги: {money}");
            return true;
        }
        else
        {
            Debug.LogWarning("Недостаточно денег!");
            return false;
        }
    }

    /// <summary>
    /// Проверка наличия ингредиентов для приготовления блюда
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
    /// Вычитание ингредиентов для приготовления блюда
    /// </summary>
    public void DeductIngredientsForDish(int dishId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            // Получаем список ингредиентов и их количество для данного блюда
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

                        // Обновляем количество ингредиента в инвентаре
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
    /// Увеличение количества блюда в DishStock
    /// </summary>
    public void IncreaseDishStock(int dishId)
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
    /// Уменьшение количества блюда в DishStock
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
    /// Получение количества ингредиента в инвентаре
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
    /// Обновление количества ингредиента в инвентаре
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
    /// Добавление ингредиента в инвентарь
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
    /// Добавление блюда в DishStock
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
    /// Получение текущего запаса блюда
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

        Debug.Log($"В базе данных найдено {unlockedDishes.Count} разблокированных блюд.");
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
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public float SpeedModifier { get; set; } // Множитель скорости (например, 0.8 = на 20% быстрее)
        public bool IsHired { get; set; }
    }

    public float GetCookingSpeedBonus()
    {
        float bonus = 0f; // Начальный бонус

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Employees WHERE IsHired = 1";
            int hiredCount = Convert.ToInt32(command.ExecuteScalar());

            // Формула: 10% ускорения за каждого нанятого
            bonus = hiredCount * 0.1f;
        }

        return Mathf.Min(bonus, 0.5f); // Максимум 50% ускорения
    }
    // Добавляем в DatabaseManager.cs
    public float GetCookingSpeedMultiplier()
    {
        // Каждый нанятый сотрудник дает 10% ускорения (макс. 50%)
        int hiredCount = GetHiredEmployeesCount();
        return 1f - Mathf.Min(hiredCount * 0.2f, 1f); // Например: 0.9 при 10% ускорении
    }

    public int GetHiredEmployeesCount()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Employees WHERE IsHired = 1";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
    public int GetHiredChefsCount()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Employees WHERE IsHired = 1 AND Role = 'Повар'";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
    public bool IsDecorPurchased(int decorId)
    {
        Decoration decor = decorations.Find(d => d.DecorationID == decorId);
        return decor != null && decor.IsPurchased;
    }

    // Обновляем метод покупки:
    public bool BuyDecoration(int decorationId)
    {
        Decoration decor = decorations.Find(d => d.DecorationID == decorationId);
        if (decor == null || decor.IsPurchased) return false;

        if (SpendMoney(decor.Price))
        {
            decor.IsPurchased = true;
            return true;
        }
        return false;
    }

    private void FindAndShowDecor(int decorId)
    {
        DecorObject[] allDecor = FindObjectsOfType<DecorObject>(true); // Ищем даже скрытые
        foreach (var decor in allDecor)
        {
            if (decor.decorID == decorId)
            {
                decor.ShowDecor();
                break;
            }
        }
    }
    public float GetIncomeMultiplier()
    {
        float multiplier = 1f;
        foreach (var decor in decorations)
        {
            if (decor.IsPurchased)
            {
                multiplier *= decor.IncomeMultiplier;
            }
        }
        return multiplier;
    }
    private void LoadDecorations()
    {
        foreach (var decor in decorations)
        {
            decor.IsPurchased = PlayerPrefs.GetInt($"Decor_{decor.DecorationID}", 0) == 1;
            if (decor.IsPurchased)
            {
                GameObject decorObj = GameObject.Find(decor.PrefabName);
                if (decorObj != null) decorObj.SetActive(true);
            }
        }
    }
}
[System.Serializable]
public class Decoration
{
    public int DecorationID;
    public string Name;
    public float Price;
    public float IncomeMultiplier;
    public bool IsPurchased;
    public string PrefabName; // Имя префаба/объекта на сцене
}