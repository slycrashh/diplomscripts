using UnityEngine;
using UnityEngine.EventSystems;
[RequireComponent(typeof(Collider2D))] // ������������� ������� ���������, ���� ��� ���
public class CustomerClickHandler : MonoBehaviour
{
    public CustomerController customerController; // ������ �� ���������� �����

    public void OnMouseDown()
    {
        // ��������� ���� ��� ����� �� �����
        if (customerController != null && MenuManager.Instance != null)
        {
            Debug.Log("���� �� �����!"); // �������� ������������
            MenuManager.Instance.ShowCustomerMenu(customerController);
        }
        else
        {
            Debug.LogError("CustomerController ��� MenuManager �� ���������.");
        }
    }
}