using UnityEngine;

public class ActivarPuerta : MonoBehaviour
{

    public GameObject puerta; // Asigna la puerta en el Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bala"))
        {
            // Desactivar la puerta al colisionar con una bala
            puerta.SetActive(false);
        }
    }

}
