using BombBoy.Models.GridEntities;
using BombBoy.Models.Items;
using BombBoy.Models.Strategies;
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
    public bool HasShield { get; set; } = false;
    public int ShieldDuration { get; set; } = 0;


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
        // shield absorbs one hit entirely
        if (HasShield)
        {
            HasShield = false;
            ShieldDuration = 0;
            return false;
        }

        Lives--;
        if (Lives <= 0)
        {
            Lives = 0;
            IsAlive = false;
            return true;
        }
        return false;
    }
}