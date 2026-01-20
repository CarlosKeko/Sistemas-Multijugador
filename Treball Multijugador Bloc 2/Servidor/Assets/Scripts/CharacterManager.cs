using TMPro;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Samples;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    public GameObject perro;

    public GameObject creeper;
    private void Awake()
    {


        // Opcional: si quieres que sobreviva entre escenas
        // DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        Debug.Log("Instanciando CharacterManger");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        float posicionXPerro = ClientBehaviour.Instance.posXPerro;
        float posicionYPerro = ClientBehaviour.Instance.posYPerro;
        float posicionXCreeper = ClientBehaviour.Instance.posXCreeper;
        float posicionYCreeper = ClientBehaviour.Instance.posYCreeper;

        //Debug.Log(posicionX);
        //Debug.Log(posicionY);
        perro.gameObject.SetActive(true);
        perro.transform.position = new Vector3(posicionXPerro, posicionYPerro, 0);
        creeper.gameObject.SetActive(true);
        creeper.transform.position = new Vector3(posicionXCreeper, posicionYCreeper, 0);

        //Debug.Log(posicionXPerro + " , " + posicionYPerro);

        if (ClientBehaviour.Instance.perro == true) {

            Debug.Log("Perro visto");


            if (perro != null)
            {

            }
        }
        if (ClientBehaviour.Instance.creeper == true)
        {
            if (creeper != null)
            {

            }
        }
    }
    private void Update()
    {
        
    }

    public void actualizarPosicion(string nombrePersonaje, Vector3 posicion)
    {
        Debug.Log("Entrando en actuaizar posicion");
        if (nombrePersonaje == "perroP")
        {
            perro.transform.position = posicion;

        }else if (nombrePersonaje == "creeperP")
        {
            creeper.transform.position = posicion;

        }
    }
}
