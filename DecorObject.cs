using UnityEngine;

public class DecorObject : MonoBehaviour
{
    public int decorID;

    private void Start()
    {
        // ��������� ��� ������, ������ �� ���� �����
        if (DatabaseManager.Instance.IsDecorPurchased(decorID))
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ShowDecor()
    {
        gameObject.SetActive(true);
    }
    private void Update()
    {
        // ��������� ��� ������, ������ �� ���� �����
        if (DatabaseManager.Instance.IsDecorPurchased(decorID))
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}