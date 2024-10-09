using System;
using System.Collections.Generic;
using UnityEngine;

namespace GwentCompiler
{
    public abstract class AstNode { }

    public class ProgramNode : AstNode
    {
        public List<DefinitionNode> Definitions { get; }
        

        public ProgramNode(List<DefinitionNode> definitions)
        {
            Definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        }
    }

    public abstract class DefinitionNode : AstNode { }

    public class EffectDefinitionNode : DefinitionNode
    {
        public string Name { get; }
        public List<ParameterNode> Parameters { get; }
        public LambdaExpressionNode Body { get; }

        public EffectDefinitionNode(string name, List<ParameterNode> parameters, LambdaExpressionNode body)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }
    }
    public class ExpressionStatementNode : StatementNode
    {
        public ExpressionNode Expression { get; }

        public ExpressionStatementNode(ExpressionNode expression)
        {
            Expression = expression;
        }
    }
    public class IfStatementNode : StatementNode
    {
        public ExpressionNode Condition { get; }
        public List<StatementNode> ThenBlock { get; }
        public List<StatementNode>? ElseBlock { get; }

        public IfStatementNode(ExpressionNode condition, List<StatementNode> thenBlock, List<StatementNode>? elseBlock)
        {
            Condition = condition;
            ThenBlock = thenBlock;
            ElseBlock = elseBlock;
        }
    }


    public class CardDefinitionNode : DefinitionNode
    {
        public string Name { get; }
        public string Type { get; }
        public string Faction { get; }
        public int Power { get; }
        public List<string> Range { get; }
        public List<EffectInvocationNode> OnActivation { get; }

        public CardDefinitionNode(string name, string type, string faction, int power, List<string> range, List<EffectInvocationNode> onActivation)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Power = power;
            Range = range ?? throw new ArgumentNullException(nameof(range));
            OnActivation = onActivation ?? throw new ArgumentNullException(nameof(onActivation));
        }
    }

    public class EffectInvocationNode : AstNode
    {
        public string EffectName { get; }
        public Dictionary<string, ExpressionNode> Arguments { get; }
        public SelectorNode? Selector { get; }
        public EffectInvocationNode? PostAction { get; }

        public EffectInvocationNode(string effectName, Dictionary<string, ExpressionNode> arguments, SelectorNode? selector = null, EffectInvocationNode? postAction = null)
        {
            EffectName = effectName ?? throw new ArgumentNullException(nameof(effectName));
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            Selector = selector;
            PostAction = postAction;
        }
    }

  public class SelectorNode : AstNode
{
    public string Source { get; }
    public bool Single { get; }
    public LambdaExpressionNode Predicate { get; }

    public SelectorNode(string source, bool single, LambdaExpressionNode predicate)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Single = single;
        Predicate = predicate;
    }
}

    public class ParameterNode : AstNode
    {
        public string Name { get; }
        public string Type { get; }

        public ParameterNode(string name, string type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }

    public class LambdaExpressionNode : AstNode
    {
        public List<string> Parameters { get; }
        public List<StatementNode> Body { get; }

        public LambdaExpressionNode(List<string> parameters, List<StatementNode> body)
        {
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }
    }

    public abstract class StatementNode : AstNode { }

    public class ForLoopNode : StatementNode
    {
        public string IteratorName { get; }
        public IdentifierNode Collection { get; }
        public List<StatementNode> Body { get; }

        public ForLoopNode(string iteratorName, IdentifierNode collection, List<StatementNode> body)
        {
            IteratorName = iteratorName ?? throw new ArgumentNullException(nameof(iteratorName));
            Collection = collection ?? throw new ArgumentNullException(nameof(collection));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }
    }

    public class WhileLoopNode : StatementNode
    {
        public ExpressionNode Condition { get; }
        public List<StatementNode> Body { get; }

        public WhileLoopNode(ExpressionNode condition, List<StatementNode> body)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }
    }

    public class AssignmentNode : ExpressionNode
    {
        public ExpressionNode Target { get; }
        public ExpressionNode Value { get; }

        public AssignmentNode(ExpressionNode target, ExpressionNode value)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
    public class intAssignamentNode : ExpressionNode
    {
        public string Variable { get; }
        public string Value {get; }
        public string Type { get; }
        public intAssignamentNode(string Var, string value,string type)
        {
            Variable = Var;
            Value = value;
            Type = type;
        } 

    }
    public  class ExpressionNode : StatementNode 
    {
        public string Solution = "";
    }

    public class BinaryExpressionNode : ExpressionNode
    {
        public ExpressionNode Left { get; }
        public string Operator { get; }
        public ExpressionNode Right { get; }

        public BinaryExpressionNode(ExpressionNode left, string @operator, ExpressionNode right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Operator = @operator ?? throw new ArgumentNullException(nameof(@operator));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }
    }

    public class UnaryExpressionNode : ExpressionNode
    {
        public string Operator { get; }
        public ExpressionNode Operand { get; }
        public bool IsPostfix { get; }

        public UnaryExpressionNode(string @operator, ExpressionNode operand, bool isPostfix = false)
        {
            Operator = @operator ?? throw new ArgumentNullException(nameof(@operator));
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
            IsPostfix = isPostfix;
        }
    }

    public class LiteralNode : ExpressionNode
    {
        public object Value { get; }

        public LiteralNode(object value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    public class IdentifierNode : ExpressionNode
    {
        public string Name { get; }

        public IdentifierNode(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }

    public class PropertyAccessNode : ExpressionNode
    {
        public string PropertyName { get; }

        public PropertyAccessNode(string propertyName)
        {
           // Object = @object ?? throw new ArgumentNullException(nameof(@object));
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        }
    }

    public class FunctionCallNode : ExpressionNode
    {
        public string Name { get; }
        public List<ExpressionNode> Arguments { get; }

        public FunctionCallNode(string name, List<ExpressionNode> arguments)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }
    }
    public class IncrementDecrementNode : ExpressionNode
{
    public Tokens Variable { get; }
    public Tokens IsIncrement { get; } // true para ++, false para --

    public IncrementDecrementNode(Tokens variable, Tokens isIncrement)
    {
        Variable = variable ?? throw new ArgumentNullException(nameof(variable));
        IsIncrement = isIncrement;
    }
}

    public class ParserException : Exception
    {
        public Tokens Token { get; }

        public ParserException(string message, Tokens token) : base($"{message} at line {token.Line}, column {token.Column}")
        {
            Token = token;
        }
    }
    
    public class EffectArgumentNode : AstNode
    {
        public string Name { get; }
        public ExpressionNode Value { get; }

        public EffectArgumentNode(string name, ExpressionNode value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}