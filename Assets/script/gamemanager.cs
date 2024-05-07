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
    public int ronda1, ronda2 = 0;
    public bool jugadorcito1end,jugadorcito2end = false;
    public TextMeshProUGUI podercito1;
    public TextMeshProUGUI podercito2;
    public decksito decksito1, decksito2;
    public GameObject game1, game2,texto;
    public void Vaciar()
    {
        if (decksito1.PosClimaCarta[0] != null)
        {
            Destroy(decksito1.PosClimaCarta[0]);
            decksito1.PosClimaCarta = null;
        }
        if (decksito2.PosClimaCarta[0] != null)
        {
            Destroy(decksito2.PosClimaCarta[0]);
            decksito2.PosClimaCarta = null;
        }
        for (int i = 0; i < 5; i++)
        {
            if (decksito1.PosCaCCarta[i] != null)
            {
                Destroy(decksito1.PosCaCCarta[i]);
                decksito1.PosCaCCarta[i] = null;
            }
            if (decksito1.PosADCarta[i] != null)
            {
                Destroy(decksito1.PosADCarta[i]);
                decksito1.PosADCarta[i] = null;
            }
            if (decksito1.PosACarta[i] != null)
            {
                Destroy(decksito1.PosACarta[i]);
                decksito1.PosACarta[i] = null;
            }
            if (decksito2.PosCaCCarta[i] != null)
            {
                Destroy(decksito2.PosCaCCarta[i]);
                decksito2.PosCaCCarta[i] = null;
            }
            if (decksito2.PosADCarta[i] != null)
            {
                Destroy(decksito2.PosADCarta[i]);
                decksito2.PosADCarta[i] = null;
            }
            if (decksito2.PosACarta[i] != null)
            {
                Destroy(decksito2.PosACarta[i]);
                decksito2.PosACarta[i] = null;
            }
        }
        for(int i = 0; i < 3; i++)
        {
            if (decksito1.PosEspCarta[i] != null)
            {
                Destroy(decksito1.PosEspCarta[i]);
                decksito1.PosEspCarta[i] = null;
            }
            if (decksito2.PosEspCarta[i] != null)
            {
                Destroy(decksito2.PosEspCarta[i]);
                decksito2.PosEspCarta[i] = null;
            }
        }
    }

    public bool END_GAME()
    {
        
        if (ronda1 == 2)
        {
            GameObject.FindGameObjectWithTag("text").GetComponent<TextMeshProUGUI>().text = "Player 1 Win the Game";
            return true;
        }
        if(ronda2 == 2)
        {
            GameObject.FindGameObjectWithTag("text").GetComponent<TextMeshProUGUI>().text = "Player 2 Win the Game";
            return true;
        }
        return false;
    }    
    public void ContadorsitoRondas()
    {
        if(jugadorcito1end && jugadorcito2end)
        {
            Vaciar();
            texto = Instantiate(game1, game2.transform.position, game2.transform.rotation);

            if (poderxito1 > poderxito2 && ronda1 != 2 && ronda2 != 2)
            {
                ronda1++;
                GameObject.FindGameObjectWithTag("text").GetComponent<TextMeshProUGUI>().text = "Player 1 Win";
            }
            if (poderxito1 < poderxito2 && ronda1 != 2 && ronda2 != 2)
            {
                ronda2++;
                GameObject.FindGameObjectWithTag("text").GetComponent<TextMeshProUGUI>().text = "Player 2 Win";
            }
            if(poderxito2 == poderxito1 && ronda1 != 2 && ronda2 != 2)
            {
                GameObject.FindGameObjectWithTag("text").GetComponent<TextMeshProUGUI>().text = "Empate";
                ronda1++;
                ronda2++;
            }

            if (!END_GAME())
            {
                jugadorcito1end = false;
                jugadorcito2end = false;
                Jugadita = false;
                poderxito1 = 0;
                poderxito2 = 0;
                decksito1.Robarcarta(2);
                decksito1.Mostrarcartas();
                decksito2.Robarcarta(2);
                decksito2.Mostrarcartas();
                Destroy(texto, 1.5f);
            }

        }
    }
    public void Contadorsito(){
        ContadorsitoRondas();
        if (playerr == 1){
            podercito1.text = "Podersito: " + poderxito1.ToString();
            podercito2.text = "Podersito: " + poderxito2.ToString();

        }

        if (playerr == 2){
            podercito1.text = "Podersito: " + poderxito2.ToString();
            podercito2.text ="Podersito: " + poderxito1.ToString();

        }
    }

    private void Update()
    {
        if(ronda2 != 2 && ronda1 != 2)
        {
            Contadorsito();
        }
    }
        
}
