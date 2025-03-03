using UnityEngine;
using System.Collections;
using System.Linq;
using static DatabaseManager;
using System.Collections.Generic;

public class CustomerController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public LayerMask chairLayer; // ���� ��� �������
    public Transform returnPosition; // ����� ��������

    private Transform targetChair;
    private Dish selectedDish; // ��������� �����
    private bool isDishReady = false; // ����, �����������, ��� ����� ������
    private bool isLeaving = false; // ����, �����������, ��� ������ ������

    void Start()
    {
        // ���������, �������� �� returnPosition
        if (returnPosition == null)
        {
            Debug.Log("Return Position is not assigned in the Inspector!");
            // ������� ��������� ����� ��������
            GameObject tempReturnPoint = new GameObject("TempReturnPosition");
            tempReturnPoint.transform.position = new Vector3(-10, 0, 0); // ������� ������ ����������
            returnPosition = tempReturnPoint.transform;
        }

        StartCustomerBehavior();
    }

    private void StartCustomerBehavior()
    {
        // ���������� ��������� �����
        isDishReady = false;
        isLeaving = false;
        selectedDish = null;
        targetChair = null;

        // �������� ����� �����
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
                targetChair = null; // ���� ��� �����, ���� ������
                FindNearestFreeChair(); // ��������� �����
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
        // ��������� � �����
        while (Vector3.Distance(transform.position, targetChair.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetChair.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // ��������� �� ����� � "�����"
        Debug.Log("Customer has seated.");
        targetChair.GetComponent<Chair>().OccupyChair();

        // �������� ��������� ���������������� �����
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

        // ���� ���� ���������� �����, ���� ��������� ������� ������������� + 20 ������
        float waitTime = selectedDish.CookTime + 20f; // ����� ������������� + 20 ������
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

        // ����������� ���� � ������������
        Debug.Log("Customer is leaving.");
        targetChair.GetComponent<Chair>().FreeChair();
        Debug.Log("Chair is free now.");

        // ������������ � ��������� �������
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

        // ���� 30 ������ ����� ��������� �������� ���������
        yield return new WaitForSeconds(30f);

        // ������������� ��������� �����
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

    public int GetSelectedDishId()
    {
        return selectedDish != null ? selectedDish.DishID : -1;
    }
}