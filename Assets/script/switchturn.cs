using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switchturn : MonoBehaviour
{
    public Camera camara1;
    public Camera camara2;
    public gamemanager gamesitomm;
    public void TurnSwitch()
    {
        if (camara1.isActiveAndEnabled)
        {
            camara1.enabled = false;
            camara2.enabled = true;
            gamesitomm.playerr = 2;
        }
        else
        {
            camara1.enabled = true;
            camara2.enabled = false;
            gamesitomm.playerr = 1;
        }
        gamesitomm.Jugadita = false;
    }
}
