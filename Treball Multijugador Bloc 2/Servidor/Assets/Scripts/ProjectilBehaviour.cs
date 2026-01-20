using UnityEngine;

public class ProjectilBehaviour : MonoBehaviour
{



    public float speed = 10f; // Velocidad del proyectil
    public float lifeTime = 5f; // Tiempo de vida del proyectil en segundos

    private float timer;

    void Start()
    {
        timer = 0f;
    }

    void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;

        // Incrementar el temporizador
        timer += Time.deltaTime;

        // Destruir el proyectil después de su tiempo de vida
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }

}
