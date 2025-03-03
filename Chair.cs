using UnityEngine;

public class Chair : MonoBehaviour
{
    public bool IsAvailable { get; private set; } = true;

    public bool OccupyChair()
    {
        if (IsAvailable)
        {
            IsAvailable = false;
            return true; // Стул успешно занят
        }
        return false; // Стул уже занят
    }

    public void FreeChair()
    {
        IsAvailable = true;
    }
}