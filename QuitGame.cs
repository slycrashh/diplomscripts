using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKey("escape"))  // если нажата клавиша Esc (Escape)
        {
            Application.Quit();    // закрыть приложение
        }
    }
    public void ExitGame()
    {
        
            Application.Quit();    // закрыть приложение
        
    }
}