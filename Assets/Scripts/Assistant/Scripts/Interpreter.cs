using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

using Assistant;
using ClassicUO.Utility.Logging;
using System.Threading.Tasks;
using ClassicUO.Game.UI.Controls;

namespace UOScript
{
    internal class RunTimeError
    {
        internal RunTimeError(ASTNode node, string error)
        {
            if (node != null)
            {
                Log.Error($"Error in Script: {node} - with Error: {error} - line: {node.LineNumber}");
                UOSObjects.Player?.SendMessage(MsgLevel.Error, $"{error} ({node.Lexeme}) - line: {node.LineNumber}");
            }
            else
            {
                Log.Error($"{error}");
                UOSObjects.Player?.SendMessage(MsgLevel.Error, $"{error}");
            }
        }
    }

    internal static class TypeConverter
    {
        internal static int ToInt(string token)
        {
            int val;

            if (token.StartsWith("0x"))
            {
                if (int.TryParse(token.Substring(2), NumberStyles.HexNumber, Interpreter.Culture, out val))
                    return val;
            }
            else if (int.TryParse(token, out val))
                return val;

            new RunTimeError(null, "Cannot convert argument to int");
            return 0;
        }

        internal static uint ToUInt(string token)
        {
            uint val;

            if (token.StartsWith("0x"))
            {
                if (uint.TryParse(token.Substring(2), NumberStyles.HexNumber, Interpreter.Culture, out val))
                    return val;
            }
            else if (uint.TryParse(token, out val))
                return val;

            new RunTimeError(null, "Cannot convert argument to uint");
            return 0;
        }

        internal static ushort ToUShort(string token)
        {
            ushort val;

            if (token.StartsWith("0x"))
            {
                if (ushort.TryParse(token.Substring(2), NumberStyles.HexNumber, Interpreter.Culture, out val))
                    return val;
            }
            else if (ushort.TryParse(token, out val))
                return val;

            new RunTimeError(null, "Cannot convert argument to ushort");
            return 0;
        }

        internal static double ToDouble(string token)
        {
            if (double.TryParse(token, out double val))
                return val;

            new RunTimeError(null, "Cannot convert argument to double");
            return 0.0;
        }

        internal static bool ToBool(string token)
        {
            if (bool.TryParse(token, out bool val))
                return val;

            new RunTimeError(null, "Cannot convert argument to bool");
            return false;
        }
    }

    internal class Scope
    {
        private Dictionary<string, Argument> _namespace = new Dictionary<string, Argument>();

        internal readonly ASTNode StartNode;
        internal readonly Scope Parent;

        internal Scope(Scope parent, ASTNode start)
        {
            Parent = parent;
            StartNode = start;
        }

        internal Argument GetVar(string name)
        {
            Argument arg;

            if (_namespace.TryGetValue(name, out arg))
                return arg;

            return null;
        }

        internal void SetVar(string name, Argument val)
        {
            _namespace[name] = val;
        }

        internal void ClearVar(string name)
        {
            _namespace.Remove(name);
        }
    }

    internal class Argument
    {
        private ASTNode _node;
        private Script _script;

        internal Argument(Script script, ASTNode node)
        {
            _node = node;
            _script = script;
        }

        internal Layer AsLayer()
        {
            if (_node.Lexeme == null)
            {
                new RunTimeError(_node, "Cannot convert argument to layer");
                return Layer.Invalid;
            }

            var arg = _script.Lookup(_node.Lexeme);
            if (arg != null)
                return arg.AsLayer();
            int val;
            if (_node.Lexeme.StartsWith("0x"))
            {
                int.TryParse(_node.Lexeme.Substring(2), NumberStyles.HexNumber, Interpreter.Culture, out val);
            }
            else 
                int.TryParse(_node.Lexeme, out val);
            Layer lay = (Layer)val;
            if (lay > Layer.Invalid && lay <= Layer.LastUserValid)
                return lay;
            return Layer.Invalid;
        }

        // Treat the argument as an integer
        internal int AsInt(bool throwerror = true)
        {
            if (_node.Lexeme == null)
            {
                new RunTimeError(_node, "Cannot convert argument to int");
                return 0;
            }

            // Try to resolve it as a scoped variable first
            var arg = _script.Lookup(_node.Lexeme);
            if (arg != null)
                return arg.AsInt(throwerror);

            int val;

            if (_node.Lexeme.StartsWith("0x"))
            {
                if (int.TryParse(_node.Lexeme.Substring(2), NumberStyles.HexNumber, Interpreter.Culture, out val))
                    return val;
            }
            else if (int.TryParse(_node.Lexeme, out val))
                return val;
            if(throwerror)
                new RunTimeError(_node, "Cannot convert argument to int");
            return 0;
        }

        // Treat the argument as an unsigned integer
        internal uint AsUInt(bool throwerror = true)
        {
            if (_node.Lexeme == null)
            {
                new RunTimeError(_node, "Cannot convert argument to uint");
                return 0;
            }

            // Try to resolve it as a scoped variable first
            var arg = _script.Lookup(_node.Lexeme);
            if (arg != null)
                return arg.AsUInt(throwerror);

            uint val;

            if (_node.Lexeme.StartsWith("0x"))
            {
                if (uint.TryParse(_node.Lexeme.Substring(2), NumberStyles.HexNumber, Interpreter.Culture, out val))
                    return val;
            }
            else if (uint.TryParse(_node.Lexeme, out val))
                return val;
            if(throwerror)
                new RunTimeError(_node, "Cannot convert argument to uint");
            return 0;
        }

        internal ushort AsUShort(bool throwerror = true)
        {
            if (_node.Lexeme == null)
            {
                new RunTimeError(_node, "Cannot convert argument to ushort");
                return 0;
            }

            // Try to resolve it as a scoped variable first
            var arg = _script.Lookup(_node.Lexeme.EndsWith("[]") ? _node.Lexeme.Substring(0, _node.Lexeme.Length - 2) : _node.Lexeme);
            if (arg != null)
                return arg.AsUShort(throwerror);

            ushort val;

            if (_node.Lexeme.StartsWith("0x"))
            {
                if (ushort.TryParse(_node.Lexeme.Substring(2), NumberStyles.HexNumber, Interpreter.Culture, out val))
                    return val;
            }
            else if (ushort.TryParse(_node.Lexeme, out val))
                return val;
            if(throwerror)
                new RunTimeError(_node, "Cannot convert argument to ushort");
            return 0;
        }

        // Treat the argument as a serial or an alias. Aliases will
        // be automatically resolved to serial numbers.
        internal uint AsSerial(bool throwerror = true)
        {
            if (_node.Lexeme == null)
            {
                new RunTimeError(_node, "Cannot convert argument to serial");
                return 0;
            }

            // Try to resolve it as a scoped variable first
            var arg = _script.Lookup(_node.Lexeme.EndsWith("[]") ? _node.Lexeme.Substring(0, _node.Lexeme.Length - 2) : _node.Lexeme);
            if (arg != null)
                return arg.AsSerial(throwerror);

            // Resolve it as a global alias next
            uint serial = Interpreter.GetAlias(_node.Lexeme);
            if (serial != uint.MaxValue)
                return serial;

            return AsUInt(throwerror);
        }

        // Treat the argument as a string
        internal string AsString()
        {
            if (_node.Lexeme == null)
            {
                new RunTimeError(_node, "Cannot convert argument to string");
                return string.Empty;
            }

            // Try to resolve it as a scoped variable first
            var arg = _script.Lookup(_node.Lexeme);
            if (arg != null)
                return arg.AsString();

            return _node.Lexeme;
        }

        internal bool AsBool()
        {
            if (_node.Lexeme == null)
            {
                new RunTimeError(_node, "Cannot convert argument to bool");
                return false;
            }

            bool val;

            if (bool.TryParse(_node.Lexeme, out val))
                return val;

            new RunTimeError(_node, "Cannot convert argument to bool");
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Argument arg))
                return false;

            return Equals(arg);
        }

        public bool Equals(Argument other)
        {
            if (other == null)
                return false;

            return (other._node.Lexeme == _node.Lexeme);
        }

        public override int GetHashCode()
        {
            if (_node.Lexeme == null)
            {
                new RunTimeError(_node, "Cannot convert argument to string");
                return 0;
            }
            return _node.Lexeme.GetHashCode();
        }
    }

    internal class Script
    {
        private ASTNode _statement;

        private Scope _scope;

        internal Argument Lookup(string name)
        {
            var scope = _scope;
            Argument result = null;

            while (scope != null)
            {
                result = scope.GetVar(name);
                if (result != null)
                    return result;

                scope = scope.Parent;
            }

            return result;
        }

        private void PushScope(ASTNode node)
        {
            _scope = new Scope(_scope, node);
        }

        private void PopScope()
        {
            _scope = _scope.Parent;
        }

        private Argument[] ConstructArguments(ref ASTNode node)
        {
            List<Argument> args = new List<Argument>();

            node = node.Next();

            while (node != null)
            {
                switch (node.Type)
                {
                    case ASTNodeType.AND:
                    case ASTNodeType.OR:
                    case ASTNodeType.EQUAL:
                    case ASTNodeType.NOT_EQUAL:
                    case ASTNodeType.LESS_THAN:
                    case ASTNodeType.LESS_THAN_OR_EQUAL:
                    case ASTNodeType.GREATER_THAN:
                    case ASTNodeType.GREATER_THAN_OR_EQUAL:
                        return args.ToArray();
                }

                args.Add(new Argument(this, node));

                node = node.Next();
            }

            return args.ToArray();
        }

        internal string ScriptName { get; }
        internal int LineNumber => _statement?.LineNumber ?? 0;
        // For now, the scripts execute directly from the
        // abstract syntax tree. This is relatively simple.
        // A more robust approach would be to "compile" the
        // scripts to a bytecode. That would allow more errors
        // to be caught with better error messages, as well as
        // make the scripts execute more quickly.
        internal Script(ASTNode root, string scriptname)
        {
            ScriptName = scriptname;
            // Set current to the first statement
            _statement = root.FirstChild();

            // Create a default scope
            _scope = new Scope(null, _statement);
        }

        internal bool ExecuteNext()
        {
            if (_statement == null)
                return false;

            if (_statement.Type != ASTNodeType.STATEMENT)
            {
                new RunTimeError(_statement, "Invalid script");
                return false;
            }

            var node = _statement.FirstChild();

            if (node == null)
            {
                new RunTimeError(_statement, "Invalid statement");
                return false;
            }

            int depth;

            switch (node.Type)
            {
                case ASTNodeType.IF:
                {
                    PushScope(node);

                    var expr = node.FirstChild();
                    var result = EvaluateExpression(ref expr);

                    // Advance to next statement
                    Advance();

                    // Evaluated true. Jump right into execution.
                    if (result)
                        break;

                    // The expression evaluated false, so keep advancing until
                    // we hit an elseif, else, or endif statement that matches
                    // and try again.
                    depth = 0;

                    while (_statement != null)
                    {
                        node = _statement.FirstChild();

                        if (node.Type == ASTNodeType.IF)
                        {
                            depth++;
                        }
                        else if (node.Type == ASTNodeType.ELSEIF)
                        {
                            if (depth == 0)
                            {
                                expr = node.FirstChild();
                                result = EvaluateExpression(ref expr);

                                // Evaluated true. Jump right into execution
                                if (result)
                                {
                                    Advance();
                                    break;
                                }
                            }
                        }
                        else if (node.Type == ASTNodeType.ELSE)
                        {
                            if (depth == 0)
                            {
                                // Jump into the else clause
                                Advance();
                                break;
                            }
                        }
                        else if (node.Type == ASTNodeType.ENDIF)
                        {
                            if (depth == 0)
                                break;

                            depth--;
                        }

                        Advance();
                    }

                    if (_statement == null)
                    {
                        new RunTimeError(node, "If with no matching endif");
                        return false;
                    }

                    break;
                }
                case ASTNodeType.ELSEIF:
                    // If we hit the elseif statement during normal advancing, skip over it. The only way
                    // to execute an elseif clause is to jump directly in from an if statement.
                    depth = 0;

                    while (_statement != null)
                    {
                        node = _statement.FirstChild();

                        if (node.Type == ASTNodeType.IF)
                        {
                            depth++;
                        }
                        else if (node.Type == ASTNodeType.ENDIF)
                        {
                            if (depth == 0)
                                break;

                            depth--;
                        }

                        Advance();
                    }

                    if (_statement == null)
                    {
                        new RunTimeError(node, "If with no matching endif");
                        return false;
                    }

                    break;
                case ASTNodeType.ENDIF:
                    PopScope();
                    Advance();
                    break;
                case ASTNodeType.ELSE:
                    // If we hit the else statement during normal advancing, skip over it. The only way
                    // to execute an else clause is to jump directly in from an if statement.
                    depth = 0;

                    while (_statement != null)
                    {
                        node = _statement.FirstChild();

                        if (node.Type == ASTNodeType.IF)
                        {
                            depth++;
                        }
                        else if (node.Type == ASTNodeType.ENDIF)
                        {
                            if (depth == 0)
                                break;

                            depth--;
                        }

                        Advance();
                    }

                    if (_statement == null)
                    {
                        new RunTimeError(node, "If with no matching endif");
                        return false;
                    }

                    break;
                case ASTNodeType.WHILE:
                {
                    // When we first enter the loop, push a new scope
                    if (_scope.StartNode != node)
                    {
                        PushScope(node);
                    }

                    var expr = node.FirstChild();
                    var result = EvaluateExpression(ref expr);

                    // Advance to next statement
                    Advance();

                    // The expression evaluated false, so keep advancing until
                    // we hit an endwhile statement.
                    if (!result)
                    {
                        depth = 0;

                        while (_statement != null)
                        {
                            node = _statement.FirstChild();

                            if (node.Type == ASTNodeType.WHILE)
                            {
                                depth++;
                            }
                            else if (node.Type == ASTNodeType.ENDWHILE)
                            {
                                if (depth == 0)
                                {
                                    PopScope();
                                    // Go one past the endwhile so the loop doesn't repeat
                                    Advance();
                                    break;
                                }

                                depth--;
                            }

                            Advance();
                        }
                    }
                    break;
                }
                case ASTNodeType.ENDWHILE:
                    // Walk backward to the while statement
                    _statement = _statement.Prev();

                    depth = 0;

                    while (_statement != null)
                    {
                        node = _statement.FirstChild();

                        if (node.Type == ASTNodeType.ENDWHILE)
                        {
                            depth++;
                        }
                        else if (node.Type == ASTNodeType.WHILE)
                        {
                            if (depth == 0)
                                break;

                            depth--;
                        }

                        _statement = _statement.Prev();
                    }

                    if (_statement == null)
                    {
                        new RunTimeError(node, "Unexpected endwhile");
                        return false;
                    }

                    break;
                case ASTNodeType.FORINLIST:
                {
                    // The iterator variable's name is the hash code of the for loop's ASTNode.
                    ASTNode nnode = node.FirstChild();
                    var varName = nnode.Lexeme;
                    int idx = node.GetHashCode();
                    var iterName = idx.ToString();
                    var oterName = ((ulong)idx << 20).ToString();

                    // When we first enter the loop, push a new scope
                    if (_scope.StartNode != node)
                    {
                        PushScope(node);
                        // Create a dummy argument that acts as our iterator object
                        idx = Utility.ToInt32(nnode.Next().Lexeme, 0);
                        var iter = new ASTNode(ASTNodeType.INTEGER, idx.ToString(), node, 0);
                        _scope.SetVar(iterName, new Argument(this, iter));
                        iter = new ASTNode(ASTNodeType.INTEGER, nnode.Next().Next().Lexeme, node, 0);
                        _scope.SetVar(oterName, new Argument(this, iter));
                        // Make the user-chosen variable have the value for the front of the list
                        var arg = Interpreter.GetListValue(varName, idx);

                        if (arg != null)
                            _scope.SetVar(varName, arg);
                        else
                            _scope.ClearVar(varName);
                    }
                    else
                    {
                        // Increment the iterator argument
                        idx = _scope.GetVar(iterName).AsInt() + 1;
                        
                        var iter = new ASTNode(ASTNodeType.INTEGER, idx.ToString(), node, 0);
                        _scope.SetVar(iterName, new Argument(this, iter));

                        // Update the user-chosen variable
                        var arg = Interpreter.GetListValue(varName, idx);

                        if (arg != null)
                            _scope.SetVar(varName, arg);
                        else
                            _scope.ClearVar(varName);
                    }

                    // Check loop condition
                    int end = _scope.GetVar(oterName).AsInt();
                    var i = _scope.GetVar(varName);

                    if (i != null && idx < end)
                    {
                        // enter the loop
                        Advance();
                    }
                    else
                    {
                        // Walk until the end of the loop
                        Advance();

                        depth = 0;

                        while (_statement != null)
                        {
                            node = _statement.FirstChild();

                            if (node.Type == ASTNodeType.FOR ||
                                node.Type == ASTNodeType.FORINLIST)
                            {
                                depth++;
                            }
                            else if (node.Type == ASTNodeType.ENDFOR)
                            {
                                if (depth == 0)
                                {
                                    PopScope();
                                    // Go one past the end so the loop doesn't repeat
                                    Advance();
                                    break;
                                }

                                depth--;
                            }

                            Advance();
                        }
                    }
                    break;
                }
                case ASTNodeType.FOR:
                {
                    // The iterator variable's name is the hash code of the for loop's ASTNode.
                    var iterName = node.GetHashCode().ToString();

                    // When we first enter the loop, push a new scope
                    if (_scope.StartNode != node)
                    {
                        PushScope(node);

                        // Grab the arguments
                        var max = node.FirstChild();

                        if (max.Type != ASTNodeType.INTEGER)
                        {
                            new RunTimeError(max, "Invalid for loop syntax");
                            return false;
                        }

                        // Create a dummy argument that acts as our loop variable
                        ASTNode iter = new ASTNode(ASTNodeType.INTEGER, "0", node, 0);

                        _scope.SetVar(iterName, new Argument(this, iter));
                    }
                    else
                    {
                        // Increment the iterator argument
                        var arg = _scope.GetVar(iterName);

                        var iter = new ASTNode(ASTNodeType.INTEGER, (arg.AsUInt() + 1).ToString(), node, 0);

                        _scope.SetVar(iterName, new Argument(this, iter));
                    }

                    // Check loop condition
                    var i = _scope.GetVar(iterName);

                    // Grab the max value to iterate to
                    node = node.FirstChild();
                    var end = new Argument(this, node);

                    if (i.AsUInt() < end.AsUInt())
                    {
                        // enter the loop
                        Advance();
                    }
                    else
                    {
                        // Walk until the end of the loop
                        Advance();

                        depth = 0;

                        while (_statement != null)
                        {
                            node = _statement.FirstChild();

                            if (node.Type == ASTNodeType.FOR ||
                                node.Type == ASTNodeType.FOREACH)
                            {
                                depth++;
                            }
                            else if (node.Type == ASTNodeType.ENDFOR)
                            {
                                if (depth == 0)
                                {
                                    PopScope();
                                    // Go one past the end so the loop doesn't repeat
                                    Advance();
                                    break;
                                }

                                depth--;
                            }

                            Advance();
                        }
                    }
                }
                break;
                case ASTNodeType.FOREACH:
                {
                    // foreach VAR in LIST
                    // The iterator's name is the hash code of the for loop's ASTNode.
                    var varName = node.FirstChild().Lexeme;
                    var iterName = node.GetHashCode().ToString();

                    // When we first enter the loop, push a new scope
                    if (_scope.StartNode != node)
                    {
                        PushScope(node);

                        // Create a dummy argument that acts as our iterator object
                        var iter = new ASTNode(ASTNodeType.INTEGER, "0", node, 0);
                        _scope.SetVar(iterName, new Argument(this, iter));

                        // Make the user-chosen variable have the value for the front of the list
                        var arg = Interpreter.GetListValue(varName, 0);

                        if (arg != null)
                            _scope.SetVar(varName, arg);
                        else
                            _scope.ClearVar(varName);
                    }
                    else
                    {
                        // Increment the iterator argument
                        var idx = _scope.GetVar(iterName).AsInt() + 1;
                        var iter = new ASTNode(ASTNodeType.INTEGER, idx.ToString(), node, 0);
                        _scope.SetVar(iterName, new Argument(this, iter));

                        // Update the user-chosen variable
                        var arg = Interpreter.GetListValue(varName, idx);

                        if (arg != null)
                            _scope.SetVar(varName, arg);
                        else
                            _scope.ClearVar(varName);
                    }

                    // Check loop condition
                    var i = _scope.GetVar(varName);

                    if (i != null)
                    {
                        // enter the loop
                        Advance();
                    }
                    else
                    {
                        // Walk until the end of the loop
                        Advance();

                        depth = 0;

                        while (_statement != null)
                        {
                            node = _statement.FirstChild();

                            if (node.Type == ASTNodeType.FOR ||
                                node.Type == ASTNodeType.FOREACH)
                            {
                                depth++;
                            }
                            else if (node.Type == ASTNodeType.ENDFOR)
                            {
                                if (depth == 0)
                                {
                                    PopScope();
                                    // Go one past the end so the loop doesn't repeat
                                    Advance();
                                    break;
                                }

                                depth--;
                            }

                            Advance();
                        }
                    }
                    break;
                }
                case ASTNodeType.ENDFOR:
                    // Walk backward to the for statement
                    _statement = _statement.Prev();

                    while (_statement != null)
                    {
                        node = _statement.FirstChild();

                        if (node.Type == ASTNodeType.FOR ||
                            node.Type == ASTNodeType.FORINLIST ||
                            node.Type == ASTNodeType.FOREACH)
                        {
                            break;
                        }

                        _statement = _statement.Prev();
                    }

                    if (_statement == null)
                    {
                        new RunTimeError(node, "Unexpected endfor");
                        return false;
                    }

                    break;
                case ASTNodeType.BREAK:
                    // Walk until the end of the loop
                    Advance();

                    depth = 0;

                    while (_statement != null)
                    {
                        node = _statement.FirstChild();

                        if (node.Type == ASTNodeType.WHILE ||
                            node.Type == ASTNodeType.FOR ||
                            node.Type == ASTNodeType.FORINLIST ||
                            node.Type == ASTNodeType.FOREACH)
                        {
                            depth++;
                        }
                        else if (node.Type == ASTNodeType.ENDWHILE ||
                            node.Type == ASTNodeType.ENDFOR)
                        {
                            if (depth == 0)
                            {
                                PopScope();

                                // Go one past the end so the loop doesn't repeat
                                Advance();
                                break;
                            }

                            depth--;
                        }

                        Advance();
                    }

                    PopScope();
                    break;
                case ASTNodeType.CONTINUE:
                    // Walk backward to the loop statement
                    _statement = _statement.Prev();

                    depth = 0;

                    while (_statement != null)
                    {
                        node = _statement.FirstChild();

                        if (node.Type == ASTNodeType.ENDWHILE ||
                            node.Type == ASTNodeType.ENDFOR)
                        {
                            depth++;
                        }
                        else if (node.Type == ASTNodeType.WHILE ||
                                 node.Type == ASTNodeType.FOR ||
                                 node.Type == ASTNodeType.FORINLIST ||
                                 node.Type == ASTNodeType.FOREACH)
                        {
                            if (depth == 0)
                                break;

                            depth--;
                        }

                        _statement = _statement.Prev();
                    }

                    if (_statement == null)
                    {
                        new RunTimeError(node, "Unexpected continue");
                        return false;
                    }
                    break;
                case ASTNodeType.STOP:
                    _statement = null;
                    break;
                case ASTNodeType.REPLAY:
                    _statement = _statement.Parent.FirstChild();
                    break;
                case ASTNodeType.QUIET:
                case ASTNodeType.FORCE:
                case ASTNodeType.COMMAND:
                    if (ExecuteCommand(node))
                        Advance();

                    break;
            }

            return (_statement != null) ? true : false;
        }

        internal void Advance()
        {
            Interpreter.ClearTimeout();
            if(_statement != null)
                _statement = _statement.Next(); 
        }

        private ASTNode EvaluateModifiers(ASTNode node, out bool quiet, out bool force, out bool not)
        {
            quiet = false;
            force = false;
            not = false;

            while (true)
            {
                switch (node.Type)
                {
                    case ASTNodeType.QUIET:
                        quiet = true;
                        break;
                    case ASTNodeType.FORCE:
                        force = true;
                        break;
                    case ASTNodeType.NOT:
                        not = true;
                        break;
                    default:
                        return node;
                }

                node = node.Next();
            }
        }

        private bool ExecuteCommand(ASTNode node)
        {
            node = EvaluateModifiers(node, out bool quiet, out bool force, out _);

            var handler = Interpreter.GetCommandHandler(node.Lexeme);

            if (handler == null)
            {
                new RunTimeError(node, "Unknown command");
                return true;
            }

            var cont = handler(node.Lexeme, ConstructArguments(ref node), quiet, force);

            if (node != null)
            {
                new RunTimeError(node, "Command did not consume all available arguments");
                return true;
            }

            return cont;
        }

        private bool CompareOperands(ASTNodeType op, IComparable lhs, IComparable rhs)
        {
            if (lhs.GetType() != rhs.GetType())
            {
                // Different types. Try to convert one to match the other.

                if (rhs is double)
                {
                    // Special case for rhs doubles because we don't want to lose precision.
                    lhs = (double)lhs;
                }
                else if (rhs is bool)
                {
                    // Special case for rhs bools because we want to down-convert the lhs.
                    var tmp = Convert.ChangeType(lhs, typeof(bool));
                    lhs = (IComparable)tmp;
                }
                else
                {
                    var tmp = Convert.ChangeType(rhs, lhs.GetType());
                    rhs = (IComparable)tmp;
                }
            }

            try
            {
                // Evaluate the whole expression
                switch (op)
                {
                    case ASTNodeType.EQUAL:
                        return lhs.CompareTo(rhs) == 0;
                    case ASTNodeType.NOT_EQUAL:
                        return lhs.CompareTo(rhs) != 0;
                    case ASTNodeType.LESS_THAN:
                        return lhs.CompareTo(rhs) < 0;
                    case ASTNodeType.LESS_THAN_OR_EQUAL:
                        return lhs.CompareTo(rhs) <= 0;
                    case ASTNodeType.GREATER_THAN:
                        return lhs.CompareTo(rhs) > 0;
                    case ASTNodeType.GREATER_THAN_OR_EQUAL:
                        return lhs.CompareTo(rhs) >= 0;
                }
            }
            catch (ArgumentException e)
            {
                new RunTimeError(null, e.Message);
                return false;
            }

            new RunTimeError(null, "Unknown operator in expression");
            return false;

        }

        private bool EvaluateExpression(ref ASTNode expr)
        {
            if (expr == null || (expr.Type != ASTNodeType.UNARY_EXPRESSION && expr.Type != ASTNodeType.BINARY_EXPRESSION && expr.Type != ASTNodeType.LOGICAL_EXPRESSION))
            {
                new RunTimeError(expr, "No expression following control statement");
                return false;
            }

            var node = expr.FirstChild();

            if (node == null)
            {
                new RunTimeError(expr, "Empty expression following control statement");
                return false;
            }

            switch (expr.Type)
            {
                case ASTNodeType.UNARY_EXPRESSION:
                    return EvaluateUnaryExpression(ref node);
                case ASTNodeType.BINARY_EXPRESSION:
                    return EvaluateBinaryExpression(ref node);
            }

            bool lhs = EvaluateExpression(ref node);

            node = node.Next();

            while (node != null)
            {
                // Capture the operator
                var op = node.Type;
                node = node.Next();

                if (node == null)
                {
                    new RunTimeError(node, "Invalid logical expression");
                    return false;
                }

                bool rhs;

                var e = node.FirstChild();

                switch (node.Type)
                {
                    case ASTNodeType.UNARY_EXPRESSION:
                        rhs = EvaluateUnaryExpression(ref e);
                        break;
                    case ASTNodeType.BINARY_EXPRESSION:
                        rhs = EvaluateBinaryExpression(ref e);
                        break;
                    default:
                    {
                        new RunTimeError(node, "Nested logical expressions are not possible");
                        return false;
                    }
                }

                switch (op)
                {
                    case ASTNodeType.AND:
                        lhs = lhs && rhs;
                        break;
                    case ASTNodeType.OR:
                        lhs = lhs || rhs;
                        break;
                    default:
                    {
                        new RunTimeError(node, "Invalid logical operator");
                        return false;
                    }
                }

                node = node.Next();
            }

            return lhs;
        }

        private bool EvaluateUnaryExpression(ref ASTNode node)
        {
            node = EvaluateModifiers(node, out bool quiet, out _, out bool not);

            var handler = Interpreter.GetExpressionHandler(node.Lexeme);

            if (handler == null)
            {
                new RunTimeError(node, "Unknown expression");
                return false;
            }

            var result = handler(node.Lexeme, ConstructArguments(ref node), quiet);

            if (not)
                return CompareOperands(ASTNodeType.EQUAL, result, false);
            else
                return CompareOperands(ASTNodeType.EQUAL, result, true);
        }

        private bool EvaluateBinaryExpression(ref ASTNode node)
        {
            // Evaluate the left hand side
            var lhs = EvaluateBinaryOperand(ref node);

            // Capture the operator
            var op = node.Type;
            node = node.Next();

            // Evaluate the right hand side
            var rhs = EvaluateBinaryOperand(ref node);

            return CompareOperands(op, lhs, rhs);
        }

        private IComparable EvaluateBinaryOperand(ref ASTNode node)
        {
            IComparable val;

            node = EvaluateModifiers(node, out bool quiet, out _, out _);
            if(node == null)
            {
                new RunTimeError(node, "NULL node found in expression");
                return 0;
            }
            switch (node.Type)
            {
                case ASTNodeType.INTEGER:
                    // to facilitate comparisons, convert ints to doubles
                    val = TypeConverter.ToInt(node.Lexeme);
                    break;
                case ASTNodeType.SERIAL:
                    val = TypeConverter.ToUInt(node.Lexeme);
                    break;
                case ASTNodeType.STRING:
                    val = node.Lexeme;
                    break;
                case ASTNodeType.DOUBLE:
                    val = TypeConverter.ToDouble(node.Lexeme);
                    break;
                case ASTNodeType.OPERAND:
                {
                    // This might be a registered keyword, so do a lookup
                    var handler = Interpreter.GetExpressionHandler(node.Lexeme);

                    if (handler == null)
                    {
                        // It's just a string
                        val = node.Lexeme;
                    }
                    else
                    {
                        val = handler(node.Lexeme, ConstructArguments(ref node), quiet);
                    }
                    break;
                }
                default:
                    new RunTimeError(node, "Invalid type found in expression");
                    val = 0;
                    break;
            }

            return val;
        }
    }

    internal static class Interpreter
    {
        // Aliases only hold serial numbers
        private static Dictionary<string, uint> _aliases = new Dictionary<string, uint>();

        // Lists
        private static Dictionary<string, List<Argument>> _lists = new Dictionary<string, List<Argument>>();

        // Timers
        private static Dictionary<string, DateTime> _timers = new Dictionary<string, DateTime>();

        // Expressions
        internal delegate IComparable ExpressionHandler(string expression, Argument[] args, bool quiet);
        internal delegate T ExpressionHandler<T>(string expression, Argument[] args, bool quiet) where T : IComparable;

        private static Dictionary<string, ExpressionHandler> _exprHandlers = new Dictionary<string, ExpressionHandler>();

        internal delegate bool CommandHandler(string command, Argument[] args, bool quiet, bool force);

        private static Dictionary<string, CommandHandler> _commandHandlers = new Dictionary<string, CommandHandler>();

        private static HashSet<string> _exempts = new HashSet<string>()
        {
            
            "if",
            "elseif",
            "else",
            "endif",
            "while",
            "endwhile",
            "to",
            "for",
            "foreach",
            "endfor",
            "break",
            "continue",
            "stop",
            "replay",
            "in",
            "not",
            "and",
            "or",
            "waitforjournal",
            "waitforcontext",
            "waitfortarget",
            "waitforgump",
            "waitforprompt",
            "waitforproperties",
            "waitforcontents",
            "promptalias",
            "pause",
            "walk",
            "turn",
            "run"
        };

        private static Dictionary<string, string> _commandHelpers { get; } = new Dictionary<string, string>();
        private static Dictionary<string, string> _expressionHelpers { get; } = new Dictionary<string, string>();

        internal static ushort ReferenceColor(string s)
        {
            if (_exempts.Contains(s))
                return ScriptTextBox.BLUE_HUE;//blue
            if (_commandHelpers.ContainsKey(s))
                return ScriptTextBox.RED_HUE;//red
            if (_expressionHelpers.ContainsKey(s))
                return ScriptTextBox.YELLOW_HUE;//yellow
            return 0;
        }

        internal static List<string> CmdArgs = new List<string>();
        internal static void GetCmdArgs(string s)
        {
            CmdArgs.AddRange(_commandHelpers.Where(key => key.Key.StartsWith(s)).Select(pv => pv.Value));
            CmdArgs.AddRange(_expressionHelpers.Where(key => key.Key.StartsWith(s)).Select(pv => pv.Value));
        }

        internal delegate uint AliasHandler(string alias);

        private static Dictionary<string, AliasHandler> _aliasHandlers = new Dictionary<string, AliasHandler>();

        private static Script _activeScript = null;
        internal static Script ActiveScript => _activeScript;

        private enum ExecutionState
        {
            RUNNING,
            PAUSED,
            TIMING_OUT
        };

        internal delegate bool TimeoutCallback();

        private static ExecutionState _executionState = ExecutionState.RUNNING;
        private static long _pauseTimeout = long.MaxValue;
        private static TimeoutCallback _timeoutCallback = null;

        internal static CultureInfo Culture;

        static Interpreter()
        {
            Culture = XmlFileParser.Culture;
        }

        internal static void RegisterExpressionHandler<T>(string keyword, ExpressionHandler<T> handler, string helper) where T : IComparable
        {
            _exprHandlers[keyword] = (expression, args, quiet) => handler(expression, args, quiet);
            if(!string.IsNullOrEmpty(helper))
                _expressionHelpers[keyword] = helper;
        }

        internal static string GetCmdHelper(string keyword)
        {
            if(_commandHelpers.TryGetValue(keyword, out string s))
                return s;
            return string.Empty;
        }

        internal static ExpressionHandler GetExpressionHandler(string keyword)
        {
            _exprHandlers.TryGetValue(keyword, out var expression);

            return expression;
        }

        internal static void RegisterCommandHandler(string keyword, CommandHandler handler, string helper)
        {
            _commandHandlers.Add(keyword, handler);
            if (!string.IsNullOrEmpty(helper))
                _commandHelpers[keyword] = helper;
        }

        internal static CommandHandler GetCommandHandler(string keyword)
        {
            _commandHandlers.TryGetValue(keyword, out CommandHandler handler);

            return handler;
        }

        internal static void RegisterAliasHandler(string keyword, AliasHandler handler)
        {
            _aliasHandlers[keyword] = handler;
        }

        internal static void UnregisterAliasHandler(string keyword)
        {
            _aliasHandlers.Remove(keyword);
        }

        internal static uint GetAlias(string alias)
        {
            // If a handler is explicitly registered, call that.
            if (_aliasHandlers.TryGetValue(alias, out AliasHandler handler))
                return handler(alias);

            uint value;
            if (_aliases.TryGetValue(alias, out value))
                return value;

            return uint.MaxValue;
        }

        internal static void SetAlias(string alias, uint serial)
        {
            _aliases[alias] = serial;
        }

        internal static void CreateList(string name)
        {
            if (_lists.ContainsKey(name))
                return;

            _lists[name] = new List<Argument>();
        }

        internal static void DestroyList(string name)
        {
            _lists.Remove(name);
        }

        internal static void ClearList(string name)
        {
            if (!_lists.ContainsKey(name))
                return;

            _lists[name].Clear();
        }

        internal static bool ListExists(string name)
        {
            return _lists.ContainsKey(name);
        }

        internal static bool ListContains(string name, Argument arg)
        {
            if (!_lists.ContainsKey(name))
            {
                new RunTimeError(null, "List does not exist");
                return false;
            }

            return _lists[name].Contains(arg);
        }

        internal static int ListLength(string name)
        {
            if (!_lists.ContainsKey(name))
            {
                new RunTimeError(null, "List does not exist");
                return 0;
            }
            else
                return _lists[name].Count;
        }

        internal static void PushList(string name, Argument arg, bool front, bool unique)
        {
            if (!_lists.ContainsKey(name))
            {
                new RunTimeError(null, "List does not exist");
                return;
            }

            if (unique && _lists[name].Contains(arg))
                return;

            if (front)
                _lists[name].Insert(0, arg);
            else
                _lists[name].Add(arg);
        }

        internal static bool PopList(string name, Argument arg)
        {
            if (!_lists.ContainsKey(name))
            {
                new RunTimeError(null, "List does not exist");
                return true;
            }

            return _lists[name].Remove(arg);
        }

        internal static bool PopList(string name, bool front)
        {
            if (!_lists.ContainsKey(name))
            {
                new RunTimeError(null, "List does not exist");
                return true;
            }

            var idx = front ? 0 : _lists[name].Count - 1;

            _lists[name].RemoveAt(idx);

            return _lists[name].Count > 0;
        }

        internal static Argument GetListValue(string name, int idx)
        {
            if (!_lists.ContainsKey(name))
            {
                new RunTimeError(null, "List does not exist");
                return null;
            }

            var list = _lists[name];

            if (idx < list.Count)
                return list[idx];

            return null;
        }

        internal static void CreateTimer(string name)
        {
            _timers[name] = DateTime.UtcNow;
        }

        internal static TimeSpan GetTimer(string name)
        {
            if (!_timers.TryGetValue(name, out DateTime timestamp))
                new RunTimeError(null, "Timer does not exist");

            TimeSpan elapsed = DateTime.UtcNow - timestamp;

            return elapsed;
        }

        internal static void SetTimer(string name, int elapsed)
        {
            // Setting a timer to start at a given value is equivalent to
            // starting the timer that number of milliseconds in the past.
            _timers[name] = DateTime.UtcNow.AddMilliseconds(-elapsed);
        }

        internal static void RemoveTimer(string name)
        {
            _timers.Remove(name);
        }

        internal static bool TimerExists(string name)
        {
            return _timers.ContainsKey(name);
        }

        internal static bool StartScript(Script script)
        {
            if (_activeScript != null)
                return false;

            _activeScript = script;
            _executionState = ExecutionState.RUNNING;

            ExecuteScript();

            return true;
        }

        internal static void StopScript()
        {
            _activeScript = null;
            _executionState = ExecutionState.RUNNING;
        }

        internal static bool ExecuteScript()
        {
            if (_activeScript == null)
                return false;

            if (_executionState == ExecutionState.PAUSED)
            {
                if (_pauseTimeout < DateTime.UtcNow.Ticks)
                    _executionState = ExecutionState.RUNNING;
                else
                    return true;
            }
            else if (_executionState == ExecutionState.TIMING_OUT)
            {
                if (_pauseTimeout < DateTime.UtcNow.Ticks)
                {
                    if (_timeoutCallback != null)
                    {
                        if (_timeoutCallback())
                        {
                            _activeScript.Advance();
                        }

                        _timeoutCallback = null;
                    }

                    /* If the callback changed the state to running, continue
                     * on. Otherwise, exit.
                     */
                    if (_executionState != ExecutionState.RUNNING)
                    {
                        _activeScript = null;
                        return false;
                    }
                }
            }
            Script s = _activeScript;
            if (!_activeScript.ExecuteNext())
            {
                if(s == _activeScript)
                    _activeScript = null;
                return false;
            }

            return true;
        }

        private static Queue<Script> _subScripts = new Queue<Script>();
        internal static void Enqueue(Script script)
        {
            if(script != null)
                _subScripts.Enqueue(script);
        }

        internal static Script GetFirstValidQueued()
        {
            Script s = null;
            while (s == null && _subScripts.Count > 0)
                s = _subScripts.Dequeue();
            return s;
        }

        internal static bool HasQueuedScripts()
        {
            return _subScripts.Count > 0;
        }

        // Pause execution for the given number of ticks (as defined by DateTime.UtcNow)
        internal static void Pause(long duration)
        {
            // Already paused or timing out
            if (_executionState != ExecutionState.RUNNING)
                return;

            _pauseTimeout = DateTime.UtcNow.Ticks + (duration * 10000);
            _executionState = ExecutionState.PAUSED;
        }

        // Unpause execution
        internal static void Unpause()
        {
            if (_executionState != ExecutionState.PAUSED)
                return;

            _pauseTimeout = 0;
            _executionState = ExecutionState.RUNNING;
        }

        // If forward progress on the script isn't made within this
        // amount of time (milliseconds), bail
        internal static void Timeout(long duration, TimeoutCallback callback)
        {
            // Don't change an existing timeout
            if (_executionState != ExecutionState.RUNNING)
                return;

            _pauseTimeout = DateTime.UtcNow.Ticks + (duration * 10000);
            _executionState = ExecutionState.TIMING_OUT;
            _timeoutCallback = callback;
        }

        // Clears any previously set timeout. Automatically
        // called any time the script advances a statement.
        internal static void ClearTimeout()
        {
            if (_executionState != ExecutionState.TIMING_OUT)
                return;

            _pauseTimeout = 0;
            _executionState = ExecutionState.RUNNING;
        }
    }
}
