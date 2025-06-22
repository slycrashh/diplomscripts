using UnityEngine;

public class DecorObject : MonoBehaviour
{
    public int decorID;

    private void Start()
    {
        // ѕровер€ем при старте, куплен ли этот декор
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
        // ѕровер€ем при старте, куплен ли этот декор
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