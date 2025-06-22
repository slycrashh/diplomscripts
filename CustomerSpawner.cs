using UnityEngine;
using System.Collections;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject customerPrefab; // Префаб гостя
    public Transform spawnPoint; // Точка появления гостя

    void Start()
    {
        // Создаем первого гостя сразу
        SpawnCustomer();

        // Создаем второго гостя через минуту
        StartCoroutine(SpawnSecondCustomer());
    }

    private IEnumerator SpawnSecondCustomer()
    {
        // Ждем 60 секунд
        yield return new WaitForSeconds(60f);

        // Создаем второго гостя
        SpawnCustomer();

        // Запускаем корутину для повторного создания гостя каждую минуту
        StartCoroutine(SpawnCustomerEveryMinute());
    }

    private IEnumerator SpawnCustomerEveryMinute()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f); // Ждем 60 секунд
            SpawnCustomer(); // Создаем нового гостя
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