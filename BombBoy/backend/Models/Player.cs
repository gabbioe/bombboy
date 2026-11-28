using BombBoy.Models.Decorator;
using BombBoy.Models.Flyweight;
using BombBoy.Models.GridEntities;
using BombBoy.Models.Items;
using BombBoy.Models.Memento;
using BombBoy.Models.State;
using BombBoy.Models.Strategies;
using BombBoy.Models.Visitor;
using System;
using System.Text.Json.Nodes;

namespace BombBoy.Models;

public class Player : IMatchVisitable
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = "Player";
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;

    public int DeathCount { get; set; } = 0;
    public int KillCount { get; set; } = 0;

    public string Colour { get; set; } = string.Empty;

    public string Direction = "Down";
    public bool IsAlive { get; set; } = true;
    public int BombCount { get; set; } = 1;

    public int BombRange { get; set; } = 1;
    public int Lives { get; set; } = 1;

    public Weapon? CurrentWeapon { get; set; } = null;

    // State Pattern Context for player physical condition (Healthy, Frozen, Drowning, DeadGhost)
    public IPlayerState State { get; private set; } = new HealthyState();

    public void TransitionToState(IPlayerState newState)
    {
        State = newState ?? throw new ArgumentNullException(nameof(newState));
    }

    public bool CanMove => State.CanMove(this);
    public bool CanAct => State.CanAct(this);

    // Decorator Component for power-ups (Speed, WallPass, Immortal/Shield)
    public IPlayerComponent Component { get; private set; } = new BasicPlayerComponent();

    private bool _manualNoclip = false;

    public void Decorate(Func<IPlayerComponent, IPlayerComponent> decoratorFactory)
    {
        Component = decoratorFactory(Component);
    }

    public void ResetDecorators()
    {
        Component = new BasicPlayerComponent();
        _manualNoclip = false;
        State = new HealthyState();
    }

    public bool HasShield
    {
        get => Component.HasShield;
        set
        {
            if (value && !Component.HasShield)
            {
                Decorate(c => new ImmortalDecorator(c, 10f));
            }
            else if (!value && Component.HasShield)
            {
                // Absorb / clear active shields
                Component.TakeDamage(this);
            }
        }
    }

    public int ShieldDuration { get; set; } = 0;

    public bool CanPassThroughWalls
    {
        get => _manualNoclip || Component.CanPassThroughWalls;
        set => _manualNoclip = value;
    }

    public int SpeedBonus => Component.SpeedBonus;

    //reikalingas tarp match, galvoju sita padaryti matcho player klase, game manager atskira turetu turet
    public int StartingLocation { get; set; }

    public JsonObject GetPossitionAndDirectionJson()
    {
        return new JsonObject
        {
            ["Id"] = Id,
            ["Name"] = Name,
            ["Direction"] = Direction,
            ["IsAlive"] = IsAlive,
            ["Lives"] = Lives,
            ["HasShield"] = HasShield,
            ["ShieldDuration"] = ShieldDuration,
            ["WeaponEmoji"] = CurrentWeapon?.GetEmoji() ?? "",
            ["WeaponUses"] = CurrentWeapon?.UsesCount ?? 0,
            ["BombCount"] = BombCount,
            ["BombRange"] = BombRange,
            ["State"] = State.StateName,
            ["StateEmoji"] = State.StatusEmoji
        };
    }

    public IExplosionStrategy ActiveBombStrategy { get; set; } = ExplosionStrategyFlyweightFactory.GetStrategy("classic");

    public void Accept(IMatchVisitor visitor) => visitor.VisitPlayer(this);
    public Bomb PlantBomb(int x, int y)
    {
        return new Bomb(x, y, BombRange, ActiveBombStrategy, Id);
    }
    public void ApplyPowerUp(IExplosionStrategy newStrategy)
    {
        ActiveBombStrategy = newStrategy;
    }

    /// <summary>
    /// Applies damage to this player through the current IPlayerState.
    /// Returns true if the player died (lives reached 0).
    /// </summary>
    public bool TakeDamage()
    {
        return State.HandleDamage(this);
    }

    public void ApplyFrost(float duration = 2.0f)
    {
        State.HandleFrostHit(this, duration);
    }

    public void UpdateState(Match match, Map map, float deltaTime)
    {
        State.Update(this, match, map, deltaTime);
    }

    public void UpdateDecorators(float deltaTime)
    {
        Component.Update(deltaTime);
    }


    /// <summary>
    /// Creates an encapsulated Memento snapshot of the player's current buff, stat, weapon, and condition state.
    /// </summary>
    public IPlayerMemento CreateMemento(string label = "Baseline")
    {
        return new PlayerMemento(
            playerId: Id,
            label: label,
            isAlive: IsAlive,
            lives: Lives,
            bombCount: BombCount,
            bombRange: BombRange,
            shieldDuration: ShieldDuration,
            manualNoclip: _manualNoclip,
            currentWeapon: CurrentWeapon,
            component: Component,
            state: State,
            activeBombStrategy: ActiveBombStrategy
        );
    }

    /// <summary>
    /// Restores the player's buffs, stats, weapon, decorators, and state from a saved Memento snapshot.
    /// </summary>
    public void RestoreMemento(IPlayerMemento memento)
    {
        if (memento is not PlayerMemento snapshot)
        {
            throw new ArgumentException("Invalid memento instance provided to Player.", nameof(memento));
        }

        IsAlive = snapshot.IsAlive;
        Lives = snapshot.Lives;
        BombCount = snapshot.BombCount;
        BombRange = snapshot.BombRange;
        ShieldDuration = snapshot.ShieldDuration;
        _manualNoclip = snapshot.ManualNoclip;
        CurrentWeapon = snapshot.CurrentWeapon;
        ActiveBombStrategy = snapshot.ActiveBombStrategy;
        Component = snapshot.Component is BasicPlayerComponent ? new BasicPlayerComponent() : snapshot.Component;
        State = snapshot.State is HealthyState ? new HealthyState() : snapshot.State;
    }

    /// <summary>
    /// Concrete Memento class nested privately inside Player (Originator).
    /// Implements the narrow IPlayerMemento interface for PlayerBuffCaretaker while hiding all player state fields from outside classes.
    /// </summary>
    private sealed class PlayerMemento : IPlayerMemento
    {
        public string PlayerId { get; }
        public string Label { get; }
        public DateTime CreatedAt { get; }

        public bool IsAlive { get; }
        public int Lives { get; }
        public int BombCount { get; }
        public int BombRange { get; }
        public int ShieldDuration { get; }
        public bool ManualNoclip { get; }
        public Weapon? CurrentWeapon { get; }
        public IPlayerComponent Component { get; }
        public IPlayerState State { get; }
        public IExplosionStrategy ActiveBombStrategy { get; }

        public PlayerMemento(
            string playerId,
            string label,
            bool isAlive,
            int lives,
            int bombCount,
            int bombRange,
            int shieldDuration,
            bool manualNoclip,
            Weapon? currentWeapon,
            IPlayerComponent component,
            IPlayerState state,
            IExplosionStrategy activeBombStrategy)
        {
            PlayerId = playerId;
            Label = label;
            CreatedAt = DateTime.UtcNow;
            IsAlive = isAlive;
            Lives = lives;
            BombCount = bombCount;
            BombRange = bombRange;
            ShieldDuration = shieldDuration;
            ManualNoclip = manualNoclip;
            CurrentWeapon = currentWeapon;
            Component = component;
            State = state;
            ActiveBombStrategy = activeBombStrategy;
        }
    }

}
