using UnityEngine;
using Unity.Networking.Transport.Samples; // Asegúrate de que el namespace coincida

public class ActivarPuerta : MonoBehaviour
{
    public GameObject puerta;
    public string puertaID; // Dale un nombre único en el inspector (ej: "PuertaNivel1")

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bala"))
        {
            // Solo el cliente que detecta la colisión envía el mensaje
            if (ClientBehaviour.Instance != null)
            {
                ClientBehaviour.Instance.SendTriggerObject(puertaID);
            }
        }

        if (other.CompareTag("Player"))
        {
            print("Jugador activó la puerta: " + puertaID);
            if (ClientBehaviour.Instance != null)
            {
                ClientBehaviour.Instance.SendTriggerObject(puertaID);
            }
        }
    }

    public void DesactivarPuerta()
    {
        if (puerta != null) puerta.SetActive(false);
    }
}