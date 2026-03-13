using System.Text.RegularExpressions;
using Puchitto.Server.Scripting.Expressions;

namespace Puchitto.Server.Scripting;

/// <summary>
/// A single MiniAntics script.
/// </summary>
/// <remarks>
/// This is for now a 1:1 rewrite from the puchitto client. It definitely can be done better.
/// </remarks>
public class MiniAnticsScript
{
    private static readonly Regex TokenizerRegex = new("""("[^"\\]*(?:\\[\S\s][^"\\]*)*"|\(|\)|\n|\s+|[^\s()]+)""", RegexOptions.Compiled);

    private IMiniAnticsAtom Atom { get; }
    
    public MiniAnticsScript(string script)
    {
        var tokens = Tokenize(script);
        Atom = Parse(tokens);
    }

    /// <summary>
    /// Runs the script.
    /// </summary>
    /// <param name="environment">The environment to run under.</param>
    /// <returns>The last returned value.</returns>
    public object? Run(MiniAnticsEnvironment environment)
    {
        return Atom.Evaluate(environment);
    }

    /// <summary>
    /// Parses a MiniAntics token stream.
    /// </summary>
    /// <param name="tokens">The tokens.</param>
    /// <returns>The parsed expression.</returns>
    private IMiniAnticsAtom Parse(Queue<string> tokens)
    {
        if (tokens.Count < 1)
        {
            throw new Exception("Empty MiniAntics script!"); // TODO
        }

        var token = tokens.Dequeue();

        if (token == "(")
        {
            var expressions = new List<IMiniAnticsAtom>();
            while (tokens.Peek() != ")")
            {
                expressions.Add(Parse(tokens));
            }

            tokens.Dequeue();
            return new Expression(expressions);
        }
        
        // Check if this is a number
        var maybeNumber = token.All(char.IsDigit);
        if (maybeNumber && float.TryParse(token, out var numberValue))
        {
            return new Number(numberValue);
        }
        
        // String?
        if (token[0] == '"')
        {
            // Check if the final token is also a "
            if (token[^1] != '"')
            {
                throw new Exception("Unterminated string."); // TODO
            }

            return new Text(token.Trim('"'));
        }

        return new Symbol(token);
    }
    
    /// <summary>
    /// Tokes a script and tokenizes it into individual tokens.
    /// </summary>
    /// <param name="script">The script.</param>
    /// <returns>A list of tokens.</returns>
    private Queue<string> Tokenize(string script)
    {
        // TODO: This is a horrible tokenizer.
        script = script.Trim();

        var tokens = new Queue<string>();
        foreach (var token in TokenizerRegex.Split(script))
        {
            var trimmedToken = token.Trim();
            if (trimmedToken.Length < 1)
                continue;
            
            tokens.Enqueue(trimmedToken);
        }

        return tokens;
    }
}