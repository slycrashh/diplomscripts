using UnityEngine;
using System.Collections;
using System.Linq;
using static DatabaseManager;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CustomerController : MonoBehaviour
{
    public GameObject orderUIPrefab; // ������ UI ��� ������
    public Vector2 uiOffset = new Vector2(0, 50f); // �������� � ��������

    private GameObject orderUIInstance;
    private RectTransform uiTransform;
    private Canvas mainCanvas;
    public float moveSpeed = 2f;
    public LayerMask chairLayer; // ���� ��� �������
    public Transform returnPosition; // ����� ��������
    private bool hasOrderExpired = false;
    private Transform targetChair;
    private Dish selectedDish; // ��������� �����
    private bool isDishReady = false; // ����, �����������, ��� ����� ������
    private bool isLeaving = false; // ����, �����������, ��� ������ ������
    private float timeRemaining; // ������ ��������
    private bool isWaiting = true;
    private float returnTime = 120f; // ����� ����������� (2 ������ ��� ������� �����)
    private int completedOrders = 0; // ���������� ����������� �������

    void Start()
    {
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("�������� Canvas �� ������ � �����!");
            enabled = false;
            return;
        }
        // ������������� UI
        orderUIInstance = Instantiate(orderUIPrefab, MenuManager.Instance.transform);
        uiTransform = orderUIInstance.GetComponent<RectTransform>();
        orderUIInstance.SetActive(false);

        // ������������� ������� ��������
        if (returnPosition == null)
        {
            Debug.Log("Return Position is not assigned in the Inspector!");
            GameObject tempReturnPoint = new GameObject("TempReturnPosition");
            tempReturnPoint.transform.position = new Vector3(-10, 0, 0);
            returnPosition = tempReturnPoint.transform;
        }

        StartCustomerBehavior(); // ������ ��������� ���������
    }
    void UpdateUIPosition()
    {
        if (orderUIInstance == null || mainCanvas == null) return;

        // ������������ ������� ���������� � ��������
        Vector3 worldPos = transform.position + new Vector3(0, 1.5f, 0);
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // ��� Screen Space - Overlay
        if (mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            uiTransform.position = screenPos + uiOffset;
        }
        // ��� Screen Space - Camera
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

        // �������� � �����
        while (Vector3.Distance(transform.position, targetChair.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetChair.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log("Customer has seated.");
        targetChair.GetComponent<Chair>().OccupyChair();

        // �������� ��������� ���������������� �����
        List<Dish> unlockedDishes = DatabaseManager.Instance.GetUnlockedDishes();
        if (unlockedDishes.Count > 0)
        {
            int randomIndex = Random.Range(0, unlockedDishes.Count);
            selectedDish = unlockedDishes[randomIndex];
            float waitTime = selectedDish.CookTime + 20f;
            Debug.Log($"Customer ordered: {selectedDish.DishName} (Waiting: {waitTime} seconds)");

            // ������� UI ������� � �������� Canvas
            if (orderUIPrefab != null)
            {
                // ������� �������� Canvas
                Canvas mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas != null)
                {
                    // ������� ��������� UI
                    orderUIInstance = Instantiate(orderUIPrefab, mainCanvas.transform);
                    orderUIInstance.transform.SetSiblingIndex(0);
                    uiTransform = orderUIInstance.GetComponent<RectTransform>();

                    // ����������� ��������� ���������
                    uiTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    uiTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    uiTransform.pivot = new Vector2(0.5f, 0);

                    // ������� �������� UI
                    TextMeshProUGUI dishText = orderUIInstance.transform.Find("DishName").GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI timerText = orderUIInstance.transform.Find("Timer").GetComponent<TextMeshProUGUI>();
                    Button serveButton = orderUIInstance.transform.Find("ServeButton").GetComponent<Button>();

                    // ����������� ����������
                    dishText.text = selectedDish.DishName;
                    serveButton.onClick.AddListener(() => {
                        if (!isDishReady && DatabaseManager.Instance.GetDishStockQuantity(selectedDish.DishID) > 0)
                        {
                            DatabaseManager.Instance.DecreaseDishStock(selectedDish.DishID);
                            isDishReady = true;
                        }
                    });

                    orderUIInstance.SetActive(true);
                    Debug.Log("UI ������� ������� ������ � �����������");
                }
                else
                {
                    Debug.LogError("�������� Canvas �� ������ � �����!");
                }
            }

            // �������� � ��������
            float timer = waitTime;
            while (timer > 0 && !isDishReady)
            {
                timer -= Time.deltaTime;

                // ��������� ������� UI
                if (orderUIInstance != null)
                {
                    // ������������ ������� ���������� � ��������
                    Vector3 worldPos = transform.position + new Vector3(0, 1.5f, 0);
                    Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                    uiTransform.position = screenPos;

                    // ��������� ������
                    TextMeshProUGUI timerText = orderUIInstance.transform.Find("Timer").GetComponent<TextMeshProUGUI>();
                    timerText.text = $"{Mathf.CeilToInt(timer)} ���";
                }

                yield return null;
            }

            // ���������� UI
            if (orderUIInstance != null)
            {
                Destroy(orderUIInstance);
                orderUIInstance = null;
            }

            // ���� ����� �����
            if (timer <= 0 && !isDishReady)
            {
                Debug.Log("����� �����! ����� ������ ��� �����.");
                isDishReady = true;
            }
        }
        else
        {
            Debug.LogError("No unlocked dishes available.");
            yield break;
        }

        // ������ �����
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
    /// ����������, ����� ����� ������
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
    /// �������� ��������� �����
    /// </summary>
    public Dish GetSelectedDish()
    {
        return selectedDish;
    }

    /// <summary>
    /// �������� ID ���������� �����
    /// </summary>
    public int GetSelectedDishId()
    {
        return selectedDish != null ? selectedDish.DishID : -1;
    }
    public void ServeDish(Dish dish)
    {
        selectedDish = dish;
        isDishReady = true;
        Debug.Log($"����� {dish.DishName} ������ �����.");
    }
    public void ReceiveDish()
    {
        if (!isDishReady)
        {
            isDishReady = true;
            Debug.Log("����� �������� ����� ����");
        }
    }
}