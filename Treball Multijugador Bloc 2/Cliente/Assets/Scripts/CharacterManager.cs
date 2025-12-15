using TMPro;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Samples;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{

    public Transform perro;

    public Transform creeper;

    private void Start()
    {
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
}
