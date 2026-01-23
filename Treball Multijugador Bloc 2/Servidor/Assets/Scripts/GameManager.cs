using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport.Samples;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración de Red")]
    [Tooltip("Velocidad de suavizado: 10 es suave, 25 es muy reactivo.")]
    public float smoothingSpeed = 15f;

    // Diccionario para guardar a dónde debe ir cada personaje (Interpolación)
    private Dictionary<string, Vector3> targetPositions = new Dictionary<string, Vector3>();

    public GameObject proyectilPrefab; // Asigna el prefab del proyectil aquí

    [Header("Objetos Estáticos del Servidor/Host")]
    public GameObject perroPersonaje;
    public GameObject creeperPersonaje;
    public GameObject goombaEnemy;

    // Diccionario para personajes instanciados dinámicamente
    private Dictionary<string, GameObject> instancedCharacters = new Dictionary<string, GameObject>();

    public Dictionary<string, int> playerHealths = new Dictionary<string, int>();
    public int startingHealth = 3;

    public struct CharacterSpawnData
    {
        public string CharacterName;
        public Vector3 Position;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Identifica qué objeto físico corresponde a cada nombre
    private GameObject GetCharacterObject(string characterName)
    {
        if (characterName == "perroP") return perroPersonaje;
        if (characterName == "creeperP") return creeperPersonaje;
        if (characterName == "goombaP") return goombaEnemy;

        // Si no está en los estáticos, buscar en instanciados
        instancedCharacters.TryGetValue(characterName, out GameObject obj);
        return obj;
    }

    // =========================================================================
    // LÓGICA DE MOVIMIENTO SUAVE (LERP)
    // =========================================================================

    void Update()
    {
        // Recorremos todos los personajes que tienen una "meta" de posición
        foreach (var entry in targetPositions)
        {
            string charName = entry.Key;
            Vector3 targetPos = entry.Value;

            // Verificamos que no estemos suavizando a nuestro propio jugador local
            // (Nuestro jugador local se mueve instantáneamente por el LocalPlayerController)
            bool isLocalPlayer = (ClientBehaviour.Instance != null && ClientBehaviour.Instance.perro && charName == "perroP") ||
                                 (ClientBehaviour.Instance != null && ClientBehaviour.Instance.creeper && charName == "creeperP");

            if (!isLocalPlayer)
            {
                GameObject obj = GetCharacterObject(charName);
                if (obj != null && obj.activeSelf)
                {
                    // 1. Suavizado para el Transform (Mundo 2D/3D)
                    obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, Time.deltaTime * smoothingSpeed);

                    // 2. Suavizado para el RectTransform (Si el objeto es UI)
                    RectTransform rect = obj.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, (Vector2)targetPos, Time.deltaTime * smoothingSpeed);
                    }
                }
            }
        }
    }

    // Este método ahora solo actualiza la "META", el Update hará el resto
    public void UpdateRemotePlayerPosition(string characterName, Vector3 position)
    {
        targetPositions[characterName] = position;
    }

    public void UpdateRemoteEnemyPosition(string characterName, Vector3 position)
    {
        // Los enemigos también se suavizan usando la misma lógica
        targetPositions[characterName] = position;
    }

    // =========================================================================
    // SPAWNING Y OTROS (Mantenido de tu versión)
    // =========================================================================

    public void SpawnCharacters(List<CharacterSpawnData> spawnData, string localPlayerName)
    {
        bool isServerHost = string.IsNullOrEmpty(localPlayerName);

        foreach (var data in spawnData)
        {
            GameObject characterObject = GetCharacterObject(data.CharacterName);

            if (characterObject != null)
            {
                characterObject.SetActive(true);
                // En el spawn sí teletransportamos directamente para evitar que el personaje 
                // "viaje" desde el centro del mapa al punto de inicio.
                characterObject.transform.position = data.Position;
                targetPositions[data.CharacterName] = data.Position;

                RectTransform rect = characterObject.GetComponent<RectTransform>();
                if (rect != null) rect.anchoredPosition = data.Position;
            }
        }
    }

    public void UpdateDamage(string characterName)
    {
        GameObject playerObject = GetCharacterObject(characterName);
        if (playerObject == null) return;

        CharacterStats stats = playerObject.GetComponent<CharacterStats>();
        if (stats != null)
        {
            stats.TakeDamage(characterName);
        }
    }

    public void SpawnRemoteProjectile(Vector3 position, float direction)
    {
        GameObject go = Instantiate(proyectilPrefab, position, Quaternion.identity);
        // Ajustar la dirección del proyectil remoto
        go.transform.right = direction > 0 ? Vector2.right : Vector2.left;
    }

    public void CheckCollisionAndUpdateHealth(string name, Vector3 position)
    {
        string cleanName = name.Replace("(Clone)", "").Trim();

        // Solo el servidor debería ejecutar esto
        if (playerHealths.ContainsKey(cleanName))
        {
            playerHealths[cleanName] -= 1;

            // --- ENVIAR AL SERVIDOR ---
            // Debes crear este método en tu ServerBehaviour para enviar un paquete 
            // con el nombre del jugador y su nueva salud a todos los clientes.
            if (ServerBehaviour.Instance != null)
            {
                // Ejemplo: Enviamos un mensaje tipo 'H' (Health)
                ServerBehaviour.Instance.BroadcastHealthUpdate(cleanName);
            }
            // ---------------------------

            // Actualizar visualmente para el host/servidor
            GameObject playerObj = GameObject.Find(name);
            if (playerObj != null) ActualizarCorazonesVisuales(playerObj, playerHealths[cleanName]);
        }
    }

    private void ActualizarCorazonesVisuales(GameObject player, int saludRestante)
    {
        // Buscamos el contenedor llamado "Hearts"
        Transform heartsContainer = player.transform.Find("Hearts");

        if (heartsContainer != null)
        {
            // Desactivamos el corazón correspondiente a la vida que acaba de perder.
            // Si saludRestante es 2, desactivamos el tercer corazón (índice 2).
            if (saludRestante < heartsContainer.childCount)
            {
                // Desactivamos el objeto "Triangle"
                heartsContainer.GetChild(saludRestante).gameObject.SetActive(false);
            }
        }
    }

    void PlayerDied(string name)
    {
        Debug.Log($"{name} ha muerto.");

        // Si eres el servidor, envía un mensaje de fin de juego
        if (ServerBehaviour.Instance != null)
        {
            // Puedes usar un código nuevo como 'K' (GameOver/Kick)
            ServerBehaviour.Instance.BroadcastGameOver();
        }
    }
}