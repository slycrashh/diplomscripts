using UnityEngine;
using System.Collections;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject customerPrefab; // ������ �����
    public Transform spawnPoint; // ����� ��������� �����

    void Start()
    {
        // ������� ������� ����� �����
        SpawnCustomer();

        // ������� ������� ����� ����� ������
        StartCoroutine(SpawnSecondCustomer());
    }

    private IEnumerator SpawnSecondCustomer()
    {
        // ���� 60 ������
        yield return new WaitForSeconds(60f);

        // ������� ������� �����
        SpawnCustomer();

        // ��������� �������� ��� ���������� �������� ����� ������ ������
        StartCoroutine(SpawnCustomerEveryMinute());
    }

    private IEnumerator SpawnCustomerEveryMinute()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f); // ���� 60 ������
            SpawnCustomer(); // ������� ������ �����
        }
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