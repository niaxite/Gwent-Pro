using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class actions : MonoBehaviour
{
    private decksito Masito;
    private void Start()
    { if (GetComponent<General_card>().faccion == "Hadas_guardianas"){
            Masito = GameObject.FindGameObjectWithTag("HadasGuardianas").GetComponent<decksito>();
        }

      if (GetComponent<General_card>().faccion == "Hadas_contaminadas"){
            Masito = GameObject.FindGameObjectWithTag("HadasContaminadas").GetComponent<decksito>();
        }
        
    }
    private void OnMouseDown()
    {
        Masito.Invocar(gameObject);
       
    }
}
