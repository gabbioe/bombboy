using System;
using System.Collections.Generic;
using BombBoy.Models.Strategies;

namespace BombBoy.Models.Composite;

/// <summary>
/// Component interface for the Composite Pattern (extends <see cref="IExplosionStrategy"/>).
/// All Composite-related types live exclusively inside BombBoy.Models.Composite.
///
/// Currently configured for SAFETINESS (Saugumas):
/// Child management methods (Add, Remove, GetChildren) are declared ONLY on
/// <see cref="CompositeExplosionStrategy"/> so calling Add/Remove on any Leaf
/// is rejected at compile time.
///
/// To switch to VISIBILITY / TRANSPARENCY (Matomumas) during the defense,
/// uncomment the 3 default interface methods below:
///   void Add(IExplosionComponent component) => throw new NotSupportedException("Leaf component cannot contain children.");
///   bool Remove(IExplosionComponent component) => throw new NotSupportedException("Leaf component cannot contain children.");
///   IReadOnlyList<IExplosionComponent> GetChildren() => Array.Empty<IExplosionComponent>();
/// </summary>
public interface IExplosionComponent : IExplosionStrategy
{
    string BombEmoji { get; }
    string GetPatternDescription();

    public static string Describe(IExplosionStrategy strategy) =>
        strategy is IExplosionComponent comp
            ? comp.GetPatternDescription()
            : strategy.GetType().Name.Replace("ExplosionStrategy", "");
}
