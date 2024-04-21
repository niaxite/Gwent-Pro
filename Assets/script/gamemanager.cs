using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class gamemanager : MonoBehaviour
{
    public bool Jugadita = false;
    public int poderxito1 = 0;
    public int poderxito2 = 0;
    public int playerr = 1; 
    public TextMeshProUGUI podercito1;
    public TextMeshProUGUI podercito2;

    public void countt(){
        if (playerr == 1){
            podercito1.text = poderxito1.ToString();
            podercito2.text = poderxito2.ToString();

        }

        if (playerr == 2){
            podercito1.text = poderxito2.ToString();
            podercito2.text = poderxito1.ToString();

        }
    }

    private void Update(){
        countt();

    }
}
