using UnityEngine;
using UnityEngine.SceneManagement;

public class DerrotaManagerMenu : MonoBehaviour
{
    public void onSalirPulsado()
    {

        SceneManager.LoadScene("MenuPrincipal");
    }
}
