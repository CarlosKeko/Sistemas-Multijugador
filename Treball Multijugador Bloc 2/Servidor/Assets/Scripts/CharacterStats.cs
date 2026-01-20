using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public string characterName;    // "Perro", "Creeper", etc.
    [SerializeField] int maxHealth = 3;
    public int currentHealth;
    [SerializeField] GameObject[] hearts;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(string nombre)
    {
        if (characterName != nombre) return;
        currentHealth -= 1;
        Debug.Log($"{characterName} recibe 1 de daño. Vida actual: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            //Die();
        }
        ActualizarCorazones();
    }

    void ActualizarCorazones()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(i < currentHealth);
        }
    }

    private void Die()
    {
        Debug.Log($"{characterName} ha muerto.");
        // Aquí haces lo que toque:
        // - Desactivar el objeto
        // - Enviar mensaje al servidor
        // - Cambiar de escena, etc.
        gameObject.SetActive(false);
    }
}
