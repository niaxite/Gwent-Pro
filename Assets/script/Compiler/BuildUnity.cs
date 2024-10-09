
using System.Collections;
using System.Collections.Generic;
using GwentCompiler;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class BuildUnity : MonoBehaviour
{
    public TMP_InputField Console, Output;
    private LexerUnity lexer;
    private ParserUnity parser;
    private SemanticAnalyzer semanticAnalyzer;
    private SymbolTable symbolTable = new SymbolTable();
    private EffectManager effectManager;
    private CardsManager cardsManager;

    void Start()
    {
        effectManager = gameObject.AddComponent<EffectManager>();
        cardsManager = gameObject.AddComponent<CardsManager>();
    }

    public void Build()
    {
        Output.text = "";
        semanticAnalyzer = new SemanticAnalyzer(symbolTable);

        // Paso 1: Análisis léxico (tokenización)
        Output.text += ("=== Análisis Léxico ===\n");
        lexer = new LexerUnity(Console.text);

        List<Tokens> tokens = lexer.Tokenize();

        Output.text += ("Tokens generados:\n");
        foreach (var token in tokens)
        {
            Output.text += ($"Token: {token.Type}, Value: {token.Value}, Line: {token.Line}, Column: {token.Column} \n");
        }

        // Paso 2: Análisis sintáctico (parsing)
        Output.text += ("\n=== Análisis Sintáctico ===\n");
        parser = new ParserUnity(tokens);
        ProgramNode program = parser.Parse();

        // Paso 3: Análisis semántico
        Output.text += ("\n=== Análisis Semántico ===\n");
        List<string> semanticErrors = semanticAnalyzer.Analyze(program);

        if (semanticErrors.Count == 0)
        {
            Output.text += ("\nCompilación completada con éxito.\n");
            effectManager.GenerateEffectsFromAST(program);
            cardsManager.GenerateCardsFromAST(program);
        }
        else
        {
            Output.text = ("\nCompilación completada con errores.\n");
            foreach (string error in semanticErrors)
            {
                Output.text += error + "\n";
            }
        }
    }
}