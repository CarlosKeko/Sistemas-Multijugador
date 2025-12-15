using UnityEngine;
using Unity.Networking.Transport.Samples;
using System; // Para acceder a ServerBehaviour

public class EnemyController : MonoBehaviour
{
    // Rango de movimiento en coordenadas del Canvas (anchoredPosition)
    [Header("Movement Settings (Canvas Coordinates)")]
    public float moveSpeed = 1f;
    public float rangeX = 3f;


    private Vector3 startPosition; // Posición de inicio en World Space


    private float sendRate = 0.1f; // Enviar 10 veces por segundo (10 Hz)
    private float nextSendTime = 0f;

    void Awake()
    {
        // Solo necesitamos este script si somos el servidor (o el Host)
        /*
        if (ServerBehaviour.Instance == null)
        {
            enabled = false;
            return;
        }
        */
        enabled = true;

        startPosition = transform.position;

    }

    void Update()
    {

        if (!enabled) return;

        // 1. Lógica de Movimiento (Ejemplo de movimiento sinusoidal simple de ida y vuelta)

        // Calculamos la nueva posición X basada en el tiempo
        // Math.Sin(Time.time) oscila entre -1 y 1.
        float newX = startPosition.x + Mathf.Sin(Time.time * moveSpeed) * rangeX;

        Vector3 newPosition = new Vector3(newX, startPosition.y, startPosition.z);
        transform.position = newPosition; // <-- ¡APLICACIÓN DIRECTA AL TRANSFORM!

        if (Time.time >= nextSendTime)
        {
            // Usamos el nombre de función corregido
            // Asumo que el nombre es BroadcastEnemyPosition
            // ServerBehaviour.Instance.UpdateRemoteEnemyPositioni(newPosition);

            nextSendTime = Time.time + sendRate; // Programar el próximo envío
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        print("Colisison");

        // Solo el servidor debe procesar esta lógica
        if (!enabled) return;

        if (other.CompareTag("Player"))
        {
            // 1. Obtener el nombre del objeto para el log
            string playerName = other.gameObject.name;

            // 2. Ejecutar la lógica de Colisión/Daño

            // La posición exacta del jugador es la posición del objeto
            Vector3 playerPosition = other.transform.position;

            // LLAMADA AL GAMEMANAGER (Igual que en el mensaje 'M')
            if (GameManager.Instance != null)
            {
                // Nota: Aquí estamos usando la posición y el nombre del Transform que COLISIONÓ.
                // Si el objeto se llama "perroP(Clone)", necesitarás ajustarlo si usas la normalización de nombres.

                GameManager.Instance.CheckCollisionAndUpdateHealth(playerName, playerPosition);
            }
        }
    }




}