using UnityEngine;

public class Chair : MonoBehaviour
{
    public bool IsAvailable { get; private set; } = true;

    public bool OccupyChair()
    {
        if (IsAvailable)
        {
            IsAvailable = false;
            return true; // ���� ������� �����
        }
        return false; // ���� ��� �����
    }

    public void FreeChair()
    {
        IsAvailable = true;
    }
}