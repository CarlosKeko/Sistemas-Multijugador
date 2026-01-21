using UnityEngine;
using Unity.Networking.Transport.Samples;

public class LocalPlayerController : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 lastPositionSent;

    public float movementSpeed = 5.0f;

    [Header("Network Settings")]
    [Tooltip("Distancia mínima para enviar (ahora mucho menor)")]
    public float positionUpdateThreshold = 0.01f;
    [Tooltip("Veces por segundo que enviamos la posición (30Hz es ideal)")]
    public float sendRate = 0.033f;

    private float lastSendTime;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("LocalPlayerController requiere un RectTransform.");
            enabled = false;
            return;
        }
        lastPositionSent = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // 1. Manejo de Input y Movimiento
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(x, y) * movementSpeed * Time.deltaTime;

        if (movement.sqrMagnitude > 0)
        {
            rectTransform.anchoredPosition += movement;
        }

        // 2. Lógica de Envío Optimizada
        // Enviamos si: Ha pasado el tiempo suficiente Y (el jugador se ha movido algo)
        if (Time.time - lastSendTime > sendRate)
        {
            float distanceMovedSqr = (rectTransform.anchoredPosition - lastPositionSent).sqrMagnitude;

            if (distanceMovedSqr > positionUpdateThreshold * positionUpdateThreshold)
            {
                if (ClientBehaviour.Instance != null)
                {
                    // Enviamos la posición actual
                    Vector3 posToSend = new Vector3(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y, 0);
                    ClientBehaviour.Instance.SendMovementUpdate(posToSend);

                    // Actualizamos marcadores
                    lastPositionSent = rectTransform.anchoredPosition;
                    lastSendTime = Time.time;

                    // Debug.Log($"Enviando a {sendRate}s: {posToSend}");
                }
            }
        }
    }
}