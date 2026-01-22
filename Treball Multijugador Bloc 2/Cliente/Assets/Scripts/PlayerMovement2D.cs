using System;
using TMPro;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Samples;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement2D : MonoBehaviour
{
    public float speed = 100f;      // Velocidad horizontal
    public float jumpForce = 100f;  // Fuerza del salto
    Rigidbody2D rb;
    bool isGrounded = false;      // Para saber si est� tocando el suelo

    public Camera camara;


    // Solo activar en el pj que hara doble salto
    public bool soloDobleSalto;
    public bool doubleJump;

    // Solo activar en el pj que disparara
    public bool disparar;

    public ProjectilBehaviour ProjectilPrefab;
    public Transform LaunchOffset;

    // Variables de sincronización de red
    private RectTransform rectTransform; // Para leer/escribir la posición UI
    private Vector3 lastPositionSent;
    public float positionUpdateThreshold = 0.5f; // Umbral más alto para física

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Obtenemos el Rigidbody2D del personaje
    }

    private void Start()
    {
        string nombreObjeto = gameObject.name;
        Debug.Log("NOMBRE PERSONAJE: " + gameObject.name );
        if (ClientBehaviour.Instance.perro && nombreObjeto != "perroP") {
            enabled = false;
            return;

        }

        if (ClientBehaviour.Instance.creeper && nombreObjeto != "creeperP")
        {
            enabled = false;
            return;
        }
        // Solo activar para testeo
        enabled = true;

        if (enabled)
        {
            if (camara == null) camara = Camera.main;

            var follow = camara.GetComponent<CameraFollow2D>();
            if (follow != null)
            {
                follow.SetTarget(transform);
            }
            else
            {
                Debug.LogWarning("La cámara no tiene CameraFollow2D.");
            }
        }
    }

    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(inputX * speed, rb.linearVelocity.y);



        // --- LÓGICA DE GIRO ---
        if (inputX > 0)
        {
            // Mirar a la derecha
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (inputX < 0)
        {
            // Mirar a la izquierda (escala negativa en X)
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // Lógica de reseteo al tocar el suelo
        if (isGrounded)
        {
            // Si estamos en el suelo, habilitamos la posibilidad de doble salto 
            // solo si el personaje tiene esa habilidad activa.
            if (soloDobleSalto)
            {
                doubleJump = true;
            }
            else
            {
                doubleJump = false;
            }
        }

        // Lógica de Salto
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                // Salto normal
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else if (doubleJump)
            {
                // Doble salto (solo entrará aquí si doubleJump es true)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                doubleJump = false; // Consumimos el doble salto
            }
        }

        if (Input.GetButtonDown("Fire1") && disparar)
        {
            // Disparar un proyectil
            print("Disparar");
            
        }
        if (Input.GetButtonDown("Fire1") && disparar)
        {
            // 1. Instancia local (lo que ya tenías)
            ProjectilBehaviour nuevoProyectil = Instantiate(ProjectilPrefab, LaunchOffset.position, LaunchOffset.rotation);
            float direccion = transform.localScale.x > 0 ? 1f : -1f;
            nuevoProyectil.transform.right = direccion > 0 ? Vector2.right : Vector2.left;

            // 2. Notificar al servidor
            if (ClientBehaviour.Instance != null)
            {
                ClientBehaviour.Instance.SendShoot(LaunchOffset.position, direccion);
            }
        }
    }

    void FixedUpdate()
    {
        // Sincronización de posición con el servidor
        if (ClientBehaviour.Instance != null)
        {
            Vector3 currentPosition = transform.position;

            // Comprobar si la posición ha cambiado lo suficiente
            if ((currentPosition - lastPositionSent).sqrMagnitude > positionUpdateThreshold * positionUpdateThreshold)
            {
                // Enviar la nueva posición al servidor
                ClientBehaviour.Instance.SendMovementUpdate(new Vector3(currentPosition.x, currentPosition.y, 0));

                // Actualizar la última posición enviada
                lastPositionSent = currentPosition;

                // --- DEBUG LOG ---
                //Debug.Log($"CLIENTE ENVÍA [M]: Posición {lastPositionSent.x:F2}, {lastPositionSent.y:F2}");
            }
        }
    }

    public void takeDamage()
    {
        Debug.Log("Entra en takeDamage");
    }

    // Detectar cu�ndo toca el suelo
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //print(collision.gameObject.name + " HOLITA");
        // Si chocamos con un objeto con tag "Ground", consideramos que estamos en el suelo
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        // 'collision.gameObject' es el otro objeto con el que colisionaste
        // 'collision.gameObject.tag' obtiene la etiqueta de ese objeto
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("¡Colisión con un player! (Etiqueta: " + collision.gameObject.name + ")");
            // Aquí va la lógica para cuando tu personaje choca con un amigo:
            // - Reproducir sonido
            // - Dar un bonus
            // - Cambiar de estado, etc.
            // Ejemplo: Destroy(collision.gameObject); // Si quieres destruir al amigo
        }
        
        if (collision.collider.CompareTag("Enemigo"))
        {
            Debug.Log("¡Colisión con un enemigo!");
            // Lógica para enemigos
        }
    }



    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

}
