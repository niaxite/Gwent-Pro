using System;
using System.Collections.Generic;
using System.Linq;

namespace GwentCompiler
{
    public enum SymbolType
    {
        Effect,
        Card,
        Variable,
        Parameter
    }

    public class Symbol
    {
        public string Name { get; }
        public SymbolType Type { get; }
        public string DataType { get; }
        public AstNode Definition { get; }

        public Symbol(string name, SymbolType type, string dataType, AstNode definition)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }
    }

    public class Scope
    {
        public Dictionary<string, Symbol> Symbols { get; } = new Dictionary<string, Symbol>();
        public Scope? Parent { get; }
        public List<Scope> Children { get; } = new List<Scope>();

        public Scope(Scope? parent = null)
        {
            Parent = parent;
            parent?.Children.Add(this);
        }

        public bool DeclareSymbol(Symbol symbol)
        {
            if (Symbols.ContainsKey(symbol.Name))
            {
                return false;
            }

            Symbols[symbol.Name] = symbol;
            return true;
        }

        public Symbol? LookupSymbol(string name)
        {
            if (Symbols.TryGetValue(name, out Symbol? symbol))
            {
                return symbol;
            }

            return Parent?.LookupSymbol(name);
        }
    }

    public class SymbolTable
    {
        private Scope currentScope;
        private readonly Scope globalScope;
        private readonly List<string> errors = new List<string>();

        public SymbolTable()
        {
            globalScope = new Scope();
            currentScope = globalScope;
        }

        public void EnterScope()
        {
            currentScope = new Scope(currentScope);
        }

        public void ExitScope()
        {
            if (currentScope.Parent != null)
            {
                currentScope = currentScope.Parent;
            }
        }

        public bool DeclareSymbol(string name, SymbolType type, string dataType, AstNode definition)
        {
            var symbol = new Symbol(name, type, dataType, definition);
            
            if (!currentScope.DeclareSymbol(symbol))
            {
                errors.Add($"Error: Symbol '{name}' already defined in current scope");
                return false;
            }

            return true;
        }

        public Symbol? LookupSymbol(string name)
        {
            return currentScope.LookupSymbol(name);
        }

        public void DeclareEffect(EffectDefinitionNode effectNode)
        {
            if (!DeclareSymbol(effectNode.Name, SymbolType.Effect, "Effect", effectNode))
            {
                return;
            }

            // Enter a new scope for the effect's parameters
            EnterScope();
            
            foreach (var param in effectNode.Parameters)
            {
                DeclareSymbol(param.Name, SymbolType.Parameter, param.Type, param);
            }

            // The scope will be exited when processing the effect body is complete
        }

        public void DeclareCard(CardDefinitionNode cardNode)
        {
            DeclareSymbol(cardNode.Name, SymbolType.Card, cardNode.Type, cardNode);
        }

        public List<string> GetErrors()
        {
            return errors.ToList();
        }

        public void Reset()
        {
            while (currentScope != globalScope)
            {
                ExitScope();
            }
            globalScope.Symbols.Clear();
            errors.Clear();
        }
    }
}