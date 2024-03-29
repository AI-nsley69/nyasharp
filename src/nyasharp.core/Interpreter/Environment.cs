﻿namespace nyasharp.Interpreter;

public class Environment
{
    public readonly Environment? enclosing;
    private readonly Dictionary<string, object?> values = new Dictionary<string, object?>();

    public Environment()
    {
        enclosing = null;
    }

    public Environment(Environment enclosing)
    {
        this.enclosing = enclosing;
    }

    public object? Get(Token name)
    {
        if (values.ContainsKey(name.lexeme))
        {
            object? value;
            values.TryGetValue(name.lexeme, out value);
            return value;
        }
        
        if (enclosing != null) return enclosing.Get(name);
        
        throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
    }

    public void Assign(Token name, object? value)
    {
        if (values.ContainsKey(name.lexeme))
        {
            values[name.lexeme] = value;
            return;
        }

        if (enclosing != null)
        {
            enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
    }

    public void Define(string name, object? value)
    {
        values[name] = value;
    }

    private Environment Ancestor(int distance)
    {
        Environment environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment.enclosing;
        }

        return environment;
    }
    
    public object? GetAt(int distance, string name)
    {
        Ancestor(distance).values.TryGetValue(name, out var value);
        return value;
    }

    public void AssignAt(int distance, Token name, object? value)
    {
        Ancestor(distance).values.Add(name.lexeme, value);
    }
}