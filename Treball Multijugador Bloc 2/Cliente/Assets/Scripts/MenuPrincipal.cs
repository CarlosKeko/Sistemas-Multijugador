using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class MenuPrincipal : MonoBehaviour
{
    public void Jugar()
    {
       
        SceneManager.LoadScene("EscogerServidor");
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit(); // Cierra la aplicación
    }
}