using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport.Samples; // Necesario para la struct CharacterSpawnData

public class GameManager : MonoBehaviour
{
    // 1. Singleton: Acceso estático a la única instancia
    public static GameManager Instance;

    private Dictionary<string, GameObject> activeCharacters = new Dictionary<string, GameObject>();

    [Header("Personajes")]
    public GameObject perroPersonaje;
    public GameObject creeperPersonaje;


    public struct CharacterSpawnData
    {
        public string CharacterName;
        public Vector3 Position;
    }


    // Necesitas una variable para el estado de vida, por ejemplo:
    private Dictionary<string, int> playerHealth = new Dictionary<string, int>();
    public float collisionRadius = 300; // Ajustar según el tamaño de tus objetos UI

    public RectTransform enemyReact;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    private void Start()
    {
        // Llama al servidor para que sepa que ya puede procesar mensajes
        if (ServerBehaviour.Instance != null)
        {
            ServerBehaviour.Instance.NotifyGameSceneReady();
        }
    }


    public void SpawnCharacters(List<CharacterSpawnData> spawnData, string localPlayerName)
    {
        Debug.Log($"Iniciando spawning de {spawnData.Count} personajes. Local player: {localPlayerName}");

        // Determinar si estamos ejecutando en el servidor host
        bool isServerHost = string.IsNullOrEmpty(localPlayerName);

        foreach (var data in spawnData)
        {
            GameObject characterObject = null;
            string charName = data.CharacterName;

            if (isServerHost)
            {

                string objectName = charName.ToLower() + "Personaje"; // "perroPersonaje", "creeperPersonaje"

                // 2. Buscar el objeto estático en la escena (Lento en el inicio, pero solo una vez)
                characterObject = GameObject.Find(objectName);


                if (characterObject != null)
                {
                    // 3. Activar el objeto que por defecto está desactivado
                    characterObject.SetActive(true);
                    // 4. Establecer la posición inicial
                    characterObject.transform.position = data.Position;
                    Debug.Log($"SERVER HOST: Activado objeto estático '{charName}' en {data.Position}");
                }
                else
                {
                    Debug.LogError($"SERVER HOST ERROR: No se encontró el objeto estático '{objectName}'.");
                }
            }
 
            

            // Almacenar el objeto (estático o instanciado) en el diccionario
            if (characterObject != null)
            {
                // Usamos el nombre base (ej. "Perro") como clave
                activeCharacters[charName] = characterObject;
            }
        }
    }

    // Fragmento de GameManager.cs (Método UpdateRemotePlayerPosition MODIFICADO)

    public void UpdateRemotePlayerPosition(string characterName, Vector3 position)
    {
        // Obtiene la referencia al GameObject estático (perroPersonaje o creeperPersonaje)
        GameObject playerObject = GetHostCharacterObject(characterName);

        // 2. Lógica de Cliente: Si no es Host, buscar en el diccionario (instanciado)
        if (playerObject == null)
        {
            activeCharacters.TryGetValue(characterName, out playerObject);
        }
    
        // --- ACTUALIZACIÓN CRÍTICA ---
        if (playerObject != null && playerObject.activeSelf)
        {
            // ¡YA NO USAMOS GetComponent<RectTransform>()!
            // Aplicamos la posición directamente al Transform (World Space)
            playerObject.transform.position = position;
            Debug.Log($"HOST/CLIENTE UPDATED [World]: {characterName} a {position.x:F2}, {position.y:F2}");
        }
        else
        {
            Debug.LogWarning($"UpdateRemotePlayerPosition: Objeto '{characterName}' no encontrado/activo.");
        }
    }

    // Este método es nuevo y nos permite mapear el nombre al objeto estático de forma segura
    private GameObject GetHostCharacterObject(string characterName)
    {
        // Es fundamental que los nombres ("Perro", "Creeper") coincidan con los de ServerBehaviour.cs
        if (characterName == "Perro")
        {
            return perroPersonaje;
        }
        else if (characterName == "Creeper")
        {
            return creeperPersonaje;
        }
        return null;
    }




    public void CheckCollisionAndUpdateHealth(string playerName, Vector3 playerPosition)
    {

        Vector3 enemyPosition = Vector3.zero;

        if (enemyReact != null)
            {
                enemyPosition = new Vector3(enemyReact.anchoredPosition.x, enemyReact.anchoredPosition.y, 0);
            }


        float distanceX = Mathf.Abs(playerPosition.x - enemyPosition.x);


        print("Distancia x:"+ distanceX);

        // 3. Verificar si hay colisión
        if (distanceX <= collisionRadius)
        {
            print("Colision");

            // Aplicar daño
            playerHealth[playerName] -= 1;

            Debug.Log($"SERVIDOR: ¡COLISIÓN! {playerName} golpeado. Vida restante: {playerHealth[playerName]}");

            // 4. NOTIFICAR A TODOS LOS CLIENTES SOBRE EL CAMBIO DE VIDA
            ServerBehaviour.Instance.BroadcastHealthUpdate(playerName, playerHealth[playerName]);
        }
    }

}