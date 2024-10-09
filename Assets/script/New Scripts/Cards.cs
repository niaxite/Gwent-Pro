using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cards : MonoBehaviour
{
    public string Name;
    public string Type;
    public string Faction;
    public int Power;
    public List<string> Range;
    public List<ActivationEffectData> OnActivation;

    public void Crear(CardData cardData)
    {
        Name = cardData.Name;
        Type = cardData.Type;
        Power = cardData.Power;
        Faction = cardData.Faction;
        Range = cardData.Range;
        OnActivation = cardData.OnActivation;
    }


}
