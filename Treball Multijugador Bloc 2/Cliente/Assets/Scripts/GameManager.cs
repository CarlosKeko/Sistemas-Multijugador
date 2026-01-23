using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport.Samples;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración de Red")]
    [Tooltip("Velocidad de suavizado: 10 es suave, 25 es muy reactivo.")]
    public float smoothingSpeed = 15f;

    // Diccionario para guardar a dónde debe ir cada personaje (Interpolación)
    private Dictionary<string, Vector3> targetPositions = new Dictionary<string, Vector3>();

    [Header("Objetos Estáticos del Servidor/Host")]
    public GameObject perroPersonaje;
    public GameObject creeperPersonaje;
    public GameObject goombaEnemy;

    public Dictionary<string, int> playerHealths = new Dictionary<string, int>();
    public int startingHealth = 3;


    public GameObject proyectilPrefab; // Asigna el prefab del proyectil aquí

    // Diccionario para personajes instanciados dinámicamente (si los hubiera)
    private Dictionary<string, GameObject> instancedCharacters = new Dictionary<string, GameObject>();

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

    // Dentro de GameManager.cs
    public void UpdateDamage(string playerName)
    {
        // Buscamos el objeto en la escena (puede llamarse "perroP" o "perroP(Clone)")
        GameObject playerObj = GameObject.Find(playerName);


        if (playerObj != null)
        {
            // Necesitamos saber cuánta vida le queda a este personaje específico
            // Podemos usar el mismo diccionario que el servidor o simplemente
            // contar cuántos corazones tiene activos actualmente.

            Transform heartsContainer = playerObj.transform.Find("Hearts");
            if (heartsContainer != null)
            {
                // Buscamos el último corazón activo y lo apagamos
                for (int i = heartsContainer.childCount - 1; i >= 0; i--)
                {
                    GameObject corazón = heartsContainer.GetChild(i).gameObject;
                    if (corazón.activeSelf)
                    {
                        corazón.SetActive(false);
                        Debug.Log($"Corazón {i} quitado a {playerName}");
                        break; // Solo quitamos uno por mensaje
                    }
                }
            }
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
        // 1. Normalizar el nombre para encontrar el objeto en la escena
        string cleanName = name.Replace("(Clone)", "").Trim();
        GameObject playerObj = GameObject.Find(name); // Buscamos el objeto original por su nombre completo

        if (playerObj == null) return;

        // 2. Inicializar salud si es necesario
        if (!playerHealths.ContainsKey(cleanName))
        {
            playerHealths.Add(cleanName, 3); // Empezamos con 3 vidas
        }

        // 3. Si aún tiene vida, restamos 1 y quitamos un corazón visual
        if (playerHealths[cleanName] > 0)
        {
            playerHealths[cleanName] -= 1;
            ActualizarCorazonesVisuales(playerObj, playerHealths[cleanName]);

            Debug.Log($"A {cleanName} le quedan {playerHealths[cleanName]} vidas.");

            if (playerHealths[cleanName] <= 0)
            {
                PlayerDied(cleanName);
            }
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
        // Lógica de respawn o fin de partida
        SceneManager.LoadScene("MenuDerrota");
    }

    public void OnClickVolverAlMenu()
    {
        if (ClientBehaviour.Instance != null)
        {
            ClientBehaviour.Instance.DisconnectAndRestart();
        }
    }


}