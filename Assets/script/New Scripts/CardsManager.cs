using System.Collections.Generic;
using UnityEngine;
using GwentCompiler;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
public class CardsManager : MonoBehaviour
{
    private Dictionary<string, List<GameObject>> factionDecks = new Dictionary<string, List<GameObject>>();
    private EffectManager effectManager; // Referencia al EffectManager para cargar los efectos
    private string deckDirectory;

    void Awake()
    {
        deckDirectory = Path.Combine(Application.persistentDataPath, "Decks");

        // Asegurarse de que la carpeta exista antes de cargar o guardar
        if (!Directory.Exists(deckDirectory))
        {
            Directory.CreateDirectory(deckDirectory);
        }

        // Referenciar el EffectManager para obtener los efectos
        effectManager = FindObjectOfType<EffectManager>();

        // Cargar los decks existentes al inicio
        LoadDecksFromJson();
    }

    public void GenerateCardsFromAST(ProgramNode ast)
    {
        foreach (var definition in ast.Definitions)
        {
            if (definition is CardDefinitionNode cardDef)
            {
                GameObject card = CreateCard(cardDef);
                AddCardToDeck(cardDef.Faction, card);
            }
        }

        // Guardar los decks actualizados en JSON
        SaveDecksToJson();
    }
    
    private GameObject CreateCard(CardDefinitionNode cardDef)
{
    CardData cardData = new CardData
    {
        Name = cardDef.Name,
        Type = cardDef.Type,
        Faction = cardDef.Faction,
        Power = cardDef.Power,
        Range = new List<string>(cardDef.Range),
        OnActivation = new List<ActivationEffectData>()
    };

    if (cardDef.OnActivation.Any())
    {
        foreach (var activationEffect in cardDef.OnActivation)
        {
            ActivationEffectData effect = new ActivationEffectData();

            string effectName = activationEffect.EffectName;
            SerializableEffect matchingEffect = FindEffectByName(effectName);
            
            if (matchingEffect != null)
            {
                effect.Effect = new EffectData
                {
                    Name = matchingEffect.Name,
                    Parameters = new Dictionary<string, object>(),
                    Action = matchingEffect.Action
                };

                // Procesar los argumentos del efecto
                foreach (var argument in activationEffect.Arguments)
                {
                    string paramName = argument.Key;
                    ExpressionNode paramValue = argument.Value;

                    // Evaluar el valor del parámetro
                    object evaluatedValue = EvaluateExpression(paramValue);

                    // Añadir al diccionario de parámetros del efecto
                    effect.Effect.Parameters[paramName] = evaluatedValue;
                }
            }
            else
            {
                Debug.LogWarning($"Effect {effectName} not found in EffectManager.");
            }

            // Selector handling
            if (activationEffect.Selector != null)
            {
                effect.Selector = new SelectorData
                {
                    Source = activationEffect.Selector.Source,
                    Single = activationEffect.Selector.Single,
                    Predicate = activationEffect.Selector.Predicate != null ? ProcessPredicate(activationEffect.Selector.Predicate) : null
                };
            }

            // PostAction handling
            if (activationEffect.PostAction != null)
            {
                effect.PostAction = new PostActionData
                {
                    Effect = activationEffect.PostAction.EffectName,
                    Selector = activationEffect.PostAction.Selector != null ? new SelectorData
                    {
                        Source = activationEffect.PostAction.Selector.Source,
                        Single = activationEffect.PostAction.Selector.Single,
                        Predicate = activationEffect.Selector.Predicate != null ? ProcessPredicate(activationEffect.Selector.Predicate) : null
                    } : null
                };
            }

            cardData.OnActivation.Add(effect);
        }
    }
    GameObject carta = new GameObject();
    carta.AddComponent<Cards>();
    carta.GetComponent<Cards>().Crear(cardData);
    
    return carta;
}


private PredicateData ProcessPredicate(LambdaExpressionNode predicate)
    {
        var predicateData = new PredicateData();
        
        foreach (var statement in predicate.Body)
        {
            if (statement is BinaryExpressionNode binaryExpression)
            {
                ProcessBinaryExpression(binaryExpression, predicateData);
            }
        }
        
        return predicateData;
    }

private void ProcessBinaryExpression(BinaryExpressionNode expression, PredicateData predicateData)
{
    if (expression.Operator == "&&" || expression.Operator == "||")
    {
        predicateData.Operator = expression.Operator == "&&" ? "AND" : "OR";
        
        if (expression.Left is BinaryExpressionNode leftBinary)
        {
            AddCondition(leftBinary, predicateData);
        }
        
        if (expression.Right is BinaryExpressionNode rightBinary)
        {
            AddCondition(rightBinary, predicateData);
        }
    }
    else
    {
        AddCondition(expression, predicateData);
    }
}

private void AddCondition(BinaryExpressionNode expression, PredicateData predicateData)
{
    string condition = "";
    
    // Process left side
    if (expression.Left is FunctionCallNode leftId)
    {
        condition += leftId.Name;
    }
    
    // Add operator
    condition += $" {expression.Operator} ";
    
    // Process right side
    if (expression.Right is LiteralNode literal)
    {
        // If the literal is a string, wrap it in quotes
        condition += literal.Value is string ? $"{literal.Value}" : literal.Value.ToString();
    }
    else if (expression.Right is IdentifierNode rightId)
    {
        condition += rightId.Name;
    }
    
    predicateData.Conditions.Add(condition);
}


    private object EvaluateExpression(ExpressionNode expression)
    {
        switch (expression)
        {
            case LiteralNode literal:
                return literal.Value;
            case IdentifierNode identifier:
                // Aquí podrías buscar el valor de la variable si es necesario
                return identifier.Name;
            case BinaryExpressionNode binary:
                return EvaluateBinaryExpression(binary);
            // Añadir más casos según sea necesario para otros tipos de expresiones
            default:
                Debug.LogWarning($"Unsupported expression type: {expression.GetType().Name}");
                return null;
        }
    }

    private object EvaluateBinaryExpression(BinaryExpressionNode binary)
    {
        var left = EvaluateExpression(binary.Left);
        var right = EvaluateExpression(binary.Right);

        switch (binary.Operator)
        {
            case "+":
                return AddValues(left, right);
            case "-":
                return SubtractValues(left, right);
            // Añadir más operadores según sea necesario
            default:
                Debug.LogWarning($"Unsupported binary operator: {binary.Operator}");
                return null;
        }
    }

    private object AddValues(object left, object right)
    {
        if (left is int l && right is int r)
            return l + r;
        if (left is string || right is string)
            return left.ToString() + right.ToString();
        // Añadir más casos según sea necesario
        return null;
    }

    private object SubtractValues(object left, object right)
    {
        if (left is int l && right is int r)
            return l - r;
        // Añadir más casos según sea necesario
        return null;
    }

    private SerializableEffect FindEffectByName(string effectName)
    {
        // Buscar el efecto por nombre en el EffectManager
        List<SerializableEffect> effects = effectManager.GetEffects();
        return effects.Find(e => e.Name == effectName);
    }

    private void AddCardToDeck(string faction, GameObject cardObject)
    {
        if (!factionDecks.ContainsKey(faction))
        {
            factionDecks[faction] = new List<GameObject>();
        }
        factionDecks[faction].Add(cardObject);
    }

private void SaveDecksToJson()
    {
        foreach (var factionDeck in factionDecks)
        {
            string faction = factionDeck.Key;
            List<GameObject> deck = factionDeck.Value;
            CardDataList cardDataList = new CardDataList();
            cardDataList.cards = new List<CardData>();

            string filePath = Path.Combine(Application.dataPath, "Resources", "Decks", $"{faction}.json");

            foreach (var cardObject in deck)
            {
                Cards card = cardObject.GetComponent<Cards>();
                CardData cardData = new CardData
                {
                    Name = card.Name,
                    Type = card.Type,
                    Faction = card.Faction,
                    Power = card.Power,
                    Range = card.Range,
                    OnActivation = card.OnActivation?.Select(ae => new ActivationEffectData
                    {
                        Effect = ae.Effect != null ? new EffectData
                        {
                            Name = ae.Effect.Name,
                            Parameters = ae.Effect.Parameters,
                            Action = ae.Effect.Action
                        } : null,
                        Selector = ae.Selector != null ? new SelectorData
                        {
                            Source = ae.Selector.Source,
                            Single = ae.Selector.Single,
                            Predicate = ae.Selector.Predicate
                        } : null,
                        PostAction = ae.PostAction != null ? new PostActionData
                        {
                            Effect = ae.PostAction.Effect,
                            Selector = ae.PostAction.Selector != null ? new SelectorData
                            {
                                Source = ae.PostAction.Selector.Source,
                                Single = ae.PostAction.Selector.Single,
                                Predicate = ae.PostAction.Selector.Predicate
                            } : null
                        } : null
                    }).ToList()
                };
                cardDataList.cards.Add(cardData);
            }

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
            string json = JsonConvert.SerializeObject(cardDataList, jsonSettings);
            File.WriteAllText(filePath, json);
        }
    }


   public void LoadDecksFromJson()
    {
        factionDecks.Clear();

        string[] jsonFiles = Directory.GetFiles(Path.Combine(Application.dataPath, "Resources", "Decks"), "*.json");

        var jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        foreach (string filePath in jsonFiles)
        {
            string faction = Path.GetFileNameWithoutExtension(filePath);
            string json = File.ReadAllText(filePath);
            CardDataList cardDataList = JsonConvert.DeserializeObject<CardDataList>(json, jsonSettings);

            List<GameObject> deck = new List<GameObject>();
            foreach (CardData cardData in cardDataList.cards)
            {
                GameObject cardObject = new GameObject();
                Cards card = cardObject.AddComponent<Cards>();
                card.Name = cardData.Name;
                card.Type = cardData.Type;
                card.Faction = cardData.Faction;
                card.Power = cardData.Power;
                card.Range = cardData.Range;

                if (cardData.OnActivation != null && cardData.OnActivation.Count > 0)
                {
                    card.OnActivation = cardData.OnActivation.Select(ae => new ActivationEffectData
                    {
                        Effect = ae.Effect != null ? new EffectData
                        {
                            Name = ae.Effect.Name,
                            Parameters = ae.Effect.Parameters,
                            Action = ae.Effect.Action
                        } : null,
                        Selector = ae.Selector != null ? new SelectorData
                        {
                            Source = ae.Selector.Source,
                            Single = ae.Selector.Single,
                            Predicate = ae.Selector.Predicate
                        } : null,
                        PostAction = ae.PostAction != null ? new PostActionData
                        {
                            Effect = ae.PostAction.Effect,
                            Selector = ae.PostAction.Selector != null ? new SelectorData
                            {
                                Source = ae.PostAction.Selector.Source,
                                Single = ae.PostAction.Selector.Single,
                                Predicate = ae.PostAction.Selector.Predicate
                            } : null
                        } : null
                    }).ToList();
                }

                deck.Add(cardObject);
            }

            factionDecks[faction] = deck;
        }
    }
}

[System.Serializable]
public class CardData
{
    public string Name;
    public string Type;
    public string Faction;
    public int Power;
    public List<string> Range;
    public List<ActivationEffectData> OnActivation;
}

[System.Serializable]
public class CardDataList
{
    public List<CardData> cards;
}

[System.Serializable]
public class ActivationEffectData
{
    public EffectData Effect;
    public SelectorData Selector;
    public PostActionData PostAction;
}

[System.Serializable]
public class EffectData
{
    public string Name;
    public Dictionary<string, object> Parameters;
    public string Action;
}

[System.Serializable]
public class SelectorData
{
    public string Source;
    public bool Single;
    public PredicateData Predicate;  // Serialized as string
}

[System.Serializable]
public class PostActionData
{
    public string Effect;
    public SelectorData Selector;
}

public class PredicateData
{
    public List<string> Conditions { get; set; } = new List<string>();
    public string Operator { get; set; } // Can be "AND" or "OR"
}
