using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class decksito : MonoBehaviour
{
   public GameObject[] Mazo = new GameObject[24] ;
   public GameObject[] Mano = new GameObject[10];
   private int Cartasig = 0;
   public GameObject PosicionI;
   public GameObject[] ManoInnie = new GameObject[10];
   public GameObject[] ManitoP = new GameObject[10];
   public GameObject[] PosCaC = new GameObject[5];
   public GameObject[] PosAD = new GameObject[5];
   public GameObject[] PosA = new GameObject[5];
   public GameObject[] PosCaCCarta = new GameObject[5];
   public GameObject[] PosADCarta = new GameObject[5];
   public GameObject[] PosACarta = new GameObject[5];
   public GameObject[] PosEsp = new GameObject[3];
   public GameObject[] PosEspCarta = new GameObject[3];
   public GameObject[] PosClima = new GameObject[1];
   public GameObject[] PosClimaCarta = new GameObject[1];
   public gamemanager gamesitomm;
   public int playersito = 1;

   private void Start()
   {
    Barajear();
    Robarcarta(10);
    Mostrarcartas();
    Movercartas();
   }
   public void Barajear()
   {
    for (int i = 0; i < Mazo.Length; i++){
        GameObject Carta = Mazo[i];
        int h = Random.Range (0, Mazo.Length);
        Mazo[i] = Mazo[h];
        Mazo[h] = Carta;
    }
   }

   public void Robarcarta(int cartacantidad){
    for (int i = 0; i < cartacantidad; i++){
        if (Mano[i] == null)
        {
            Mano[i] = Mazo[Cartasig];
            Cartasig++;
        }
    }

   }

   public void Mostrarcartas(){
    for (int i = 0; i < Mano.Length; i++){
        ManoInnie[i] = GameObject.Instantiate(Mano[i], PosicionI.transform.position,PosicionI.transform.rotation);
    }
   }

   private void Movercartas(){
    for (int i = 0; i < ManoInnie.Length; i++){
        ManoInnie[i].transform.position = ManitoP[i].transform.position;
        ManoInnie[i].transform.localScale = ManitoP[i].transform.localScale;
    }
   }

   public void Invocar(GameObject cartitapop){
    if(!gamesitomm.Jugadita && playersito == gamesitomm.playerr){
            for (int i = 0; i < PosCaC.Length; i++){
        
        if (PosCaCCarta[i] == null && cartitapop.GetComponent<General_card>().posicion == "Cuerpo_a_cuerpo"){

                cartitapop.transform.position = PosCaC[i].transform.position;
                cartitapop.transform.localScale = PosCaC[i].transform.localScale;
                PosCaCCarta[i] = cartitapop;
                gamesitomm.Jugadita = true;
                
                if (gamesitomm.playerr == 1){
                    
                    gamesitomm.poderxito1 += cartitapop.GetComponent<Unit_cards>().poder;
                
                }

                if (gamesitomm.playerr == 2){

                    gamesitomm.poderxito2 += cartitapop.GetComponent<Unit_cards>().poder;

                }
                break;
            }
        
    }

    for (int i = 0; i < PosAD.Length; i++){
        if (PosADCarta[i] == null && cartitapop.GetComponent<General_card>().posicion == "A_distancia"){
                cartitapop.transform.position = PosAD[i].transform.position;
                cartitapop.transform.localScale = PosAD[i].transform.localScale;
                PosADCarta[i] = cartitapop;
                gamesitomm.Jugadita = true;

                if (gamesitomm.playerr == 1){
                    
                    gamesitomm.poderxito1 += cartitapop.GetComponent<Unit_cards>().poder;
                
                }

                if (gamesitomm.playerr == 2){

                    gamesitomm.poderxito2 += cartitapop.GetComponent<Unit_cards>().poder;

                }
                break;
            }
    }

    for (int i = 0; i < PosA.Length; i++){
        if (PosACarta[i] == null && cartitapop.GetComponent<General_card>().posicion == "Asedio"){
                cartitapop.transform.position = PosA[i].transform.position;
                cartitapop.transform.localScale = PosA[i].transform.localScale;
                PosACarta[i] = cartitapop;
                gamesitomm.Jugadita = true;

                if (gamesitomm.playerr == 1){
                    
                    gamesitomm.poderxito1 += cartitapop.GetComponent<Unit_cards>().poder;
                
                }

                if (gamesitomm.playerr == 2){

                    gamesitomm.poderxito2 += cartitapop.GetComponent<Unit_cards>().poder;

                }
                break;
            }
    }

    for (int i = 0; i < PosEsp.Length; i++){
        if (PosEspCarta[i] == null && cartitapop.GetComponent<General_card>().posicion == "Especial"){
            cartitapop.transform.position = PosEsp[i].transform.position;
            cartitapop.transform.localScale = PosEsp[i].transform.localScale;
            PosEspCarta[i] = cartitapop; 
            gamesitomm.Jugadita = true;
             break;
            }
    }

    for (int i = 0; i < PosClima.Length; i++){
        if (PosClimaCarta[i] == null && cartitapop.GetComponent<General_card>().tipo == "Clima"){
            cartitapop.transform.position = PosClima[i].transform.position;
             cartitapop.transform.rotation = PosClima[i].transform.rotation;
            cartitapop.transform.localScale = PosClima[i].transform.localScale;
            PosClimaCarta[i] = cartitapop;
            gamesitomm.Jugadita = true;           
            break;
            }
    }

        }
    
   }
}
