using UnityEngine;
using UnityEngine.EventSystems;
[RequireComponent(typeof(Collider2D))] // Автоматически добавит коллайдер, если его нет
public class CustomerClickHandler : MonoBehaviour
{
    public CustomerController customerController; // Ссылка на контроллер гостя

    public void OnMouseDown()
    {
        // Открываем меню при клике на гостя
        if (customerController != null && MenuManager.Instance != null)
        {
            Debug.Log("Клик по гостю!"); // Проверка срабатывания
            MenuManager.Instance.ShowCustomerMenu(customerController);
        }
        else
        {
            Debug.LogError("CustomerController или MenuManager не назначены.");
        }
    }
}