using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKey("escape"))  // ���� ������ ������� Esc (Escape)
        {
            Application.Quit();    // ������� ����������
        }
    }
    public void ExitGame()
    {
        
            Application.Quit();    // ������� ����������
        
    }
}