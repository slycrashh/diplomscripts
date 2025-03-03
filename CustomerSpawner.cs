using UnityEngine;
using System.Collections;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject customerPrefab; // Префаб гостя
    public Transform spawnPoint; // Точка появления гостя
    public float delayBetweenCustomers = 30f; // Задержка между появлением гостей

    void Start()
    {
        // Создаем первого гостя сразу
        SpawnCustomer();

        // Запускаем корутину для создания второго гостя с задержкой
        StartCoroutine(SpawnCustomerWithDelay());
    }

    private IEnumerator SpawnCustomerWithDelay()
    {
        // Ждем указанное количество секунд
        yield return new WaitForSeconds(delayBetweenCustomers);

        // Создаем второго гостя
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