using System;
using System.Collections.Generic;
using System.Linq;

namespace BombBoy.Models.Composite;

/// <summary>
/// Composite node in the Composite Pattern (Safetiness variant).
/// Stores child <see cref="IExplosionComponent"/> nodes (either <see cref="ExplosionLeaf"/> leaves
/// or nested <see cref="CompositeExplosionStrategy"/> sub-trees) and executes them uniformly.
/// </summary>
public class CompositeExplosionStrategy : IExplosionComponent
{
    private readonly List<IExplosionComponent> _children = new();

    public string Name { get; }
    public string BombEmoji { get; }

    public CompositeExplosionStrategy(string name = "Combo", string bombEmoji = "⚛️")
    {
        Name = name;
        BombEmoji = bombEmoji;
    }

    public CompositeExplosionStrategy(string name, string bombEmoji, IEnumerable<IExplosionComponent> initialChildren)
        : this(name, bombEmoji)
    {
        foreach (var child in initialChildren)
        {
            Add(child);
        }
    }

    // --- Child Management (SAFETINESS: declared only on Composite, not on IExplosionComponent) ---

    public void Add(IExplosionComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        if (ReferenceEquals(this, component))
            throw new InvalidOperationException("A composite explosion component cannot add itself as a child.");

        _children.Add(component);
    }

    public bool Remove(IExplosionComponent component)
    {
        return _children.Remove(component);
    }

    public IReadOnlyList<IExplosionComponent> GetChildren() => _children.AsReadOnly();

    // --- Uniform Component Operations ---

    public void Explode(Map map, int originX, int originY, int range)
    {
        foreach (var child in _children)
        {
            child.Explode(map, originX, originY, range);
        }
    }

    public string GetPatternDescription()
    {
        if (_children.Count == 0)
            return Name;

        return $"{Name} [{string.Join(" + ", _children.Select(c => c.GetPatternDescription()))}]";
    }

    public bool ContainsStrategyType(Type strategyType)
    {
        foreach (var child in _children)
        {
            if (child.GetType() == strategyType)
                return true;

            if (child is ExplosionLeaf leaf && leaf.InnerStrategy.GetType() == strategyType)
                return true;

            if (child is CompositeExplosionStrategy nested && nested.ContainsStrategyType(strategyType))
                return true;
        }
        return false;
    }
}
