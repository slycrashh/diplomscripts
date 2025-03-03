using UnityEngine;
using System.Collections;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject customerPrefab; // ������ �����
    public Transform spawnPoint; // ����� ��������� �����
    public float delayBetweenCustomers = 30f; // �������� ����� ���������� ������

    void Start()
    {
        // ������� ������� ����� �����
        SpawnCustomer();

        // ��������� �������� ��� �������� ������� ����� � ���������
        StartCoroutine(SpawnCustomerWithDelay());
    }

    private IEnumerator SpawnCustomerWithDelay()
    {
        // ���� ��������� ���������� ������
        yield return new WaitForSeconds(delayBetweenCustomers);

        // ������� ������� �����
        SpawnCustomer();
    }

    private void SpawnCustomer()
    {
        if (customerPrefab != null && spawnPoint != null)
        {
            Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("Customer spawned at: " + spawnPoint.position);
        }
        else
        {
            Debug.LogError("Customer prefab or spawn point is not assigned.");
        }
    }
}