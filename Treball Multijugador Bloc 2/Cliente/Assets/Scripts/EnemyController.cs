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

    private float damageCooldown = 1.0f; // 1 segundo entre golpes
    private float nextDamageTime = 0f;

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

    private void OnTriggerStay2D(Collider2D other)
    {
        // Solo el servidor procesa el daño
        if (!enabled) return;

        if (other.CompareTag("Player"))
        {
            // COMPROBACIÓN: ¿Ha pasado suficiente tiempo desde el último golpe?
            if (Time.time >= nextDamageTime)
            {
                string playerName = other.gameObject.name.Replace("(Clone)", "").Trim();
                Vector3 playerPosition = other.transform.position;

                if (GameManager.Instance != null)
                {
                    // Aplicar daño
                    GameManager.Instance.CheckCollisionAndUpdateHealth(playerName, playerPosition);

                    // PROGRAMAR el próximo golpe: Tiempo actual + 1 segundo de espera
                    nextDamageTime = Time.time + damageCooldown;

                    Debug.Log($"Daño aplicado a {playerName}. Próximo daño disponible en: {damageCooldown}s");
                }
            }
        }
    }






}