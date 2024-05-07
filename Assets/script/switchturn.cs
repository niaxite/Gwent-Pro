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
            if(!gamesitomm.Jugadita)
            {
                gamesitomm.jugadorcito1end = true;
            }
            if(!gamesitomm.jugadorcito2end)
            {
                camara1.enabled = false;
                camara2.enabled = true;
                gamesitomm.playerr = 2;
            }
            
        }
        else
        {
            if (!gamesitomm.Jugadita)
            {
                gamesitomm.jugadorcito2end = true;
            }
            if (!gamesitomm.jugadorcito1end)
            {
                camara1.enabled = true;
                camara2.enabled = false;
                gamesitomm.playerr = 1;
                
            }
            
        }
        if(!gamesitomm.jugadorcito2end || !gamesitomm.jugadorcito1end)
        {
            gamesitomm.Jugadita = false;
        }
        else
        {
            gamesitomm.Jugadita = true;
        }
    }
}
