using BombBoy.Models.Decorator;
using BombBoy.Models.GridEntities;
using BombBoy.Models.Items;
using BombBoy.Models.Strategies;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BombBoy.Models;

public class Player
{
    public string Id{get;set;} = string.Empty;
    public string Name{get;set;} = "Player";
    public int X{get;set;} = 0;
    public int Y {get;set;} = 0;

    public int DeathCount { get; set; } = 0;
    public int KillCount { get; set; } = 0;

    public string Colour { get; set; }

    public string Direction = "Down";
    public bool IsAlive{get;set;} = true;
    public int BombCount{get;set;} = 1;

    public int BombRange { get; set; } = 1; // buvo = 2
    public int Lives { get; set; } = 1;

    public Weapon? CurrentWeapon { get; set; } = null;

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
    public int StartingLocation { get;set;}

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
            ["BombRange"] = BombRange
        };
    }

    public IExplosionStrategy ActiveBombStrategy { get; set; } = new ClassicCrossExplosionStrategy();
    public Bomb PlantBomb(int x, int y)
    {
        return new Bomb(x, y, BombRange, ActiveBombStrategy, Id);
    }
    public void ApplyPowerUp(IExplosionStrategy newStrategy)
    {
        ActiveBombStrategy = newStrategy;
    }

    /// <summary>
    /// Applies damage to this player. Handles shield, lives, and death.
    /// Returns true if the player died (lives reached 0).
    /// </summary>
    public bool TakeDamage()
    {
        return Component.TakeDamage(this);
    }

    public void UpdateDecorators(float deltaTime)
    {
        Component.Update(deltaTime);
    }
}
