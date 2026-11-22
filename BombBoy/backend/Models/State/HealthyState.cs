using Bombboy.Models.Tiles;

namespace BombBoy.Models.State;

/// <summary>
/// Concrete State 1: Normal active player state. Can move, use speed bonus, plant bombs, and fire weapons.
/// Transitions to FrozenState on frost hits, DrowningState on water tiles, or DeadGhostState on fatal damage.
/// </summary>
public sealed class HealthyState : IPlayerState
{
    public string StateName => "Healthy";
    public string StatusEmoji => string.Empty;
    public bool AllowsSpeedBonus => true;

    public bool CanMove(Player player) => player.IsAlive;

    public bool CanAct(Player player) => player.IsAlive;

    public bool HandleDamage(Player player)
    {
        bool died = player.Component.TakeDamage(player);
        if (died)
        {
            player.TransitionToState(new DeadGhostState());
        }
        return died;
    }

    public void HandleFrostHit(Player player, float freezeDuration)
    {
        if (player.HasShield)
        {
            // Active shield absorbs the freeze hit
            player.Component.TakeDamage(player);
            return;
        }

        player.TransitionToState(new FrozenState(freezeDuration));
    }

    public void Update(Player player, Match match, Map map, float deltaTime)
    {
        if (!player.IsAlive)
        {
            player.TransitionToState(new DeadGhostState());
            return;
        }

        if (map.GetTile(player.X, player.Y) is WaterTile waterTile && waterTile.CanDrown())
        {
            player.TransitionToState(new DrowningState());
            return;
        }

        if (!player.CanPassThroughWalls && !map.CheckIfCanMove(player.X, player.Y))
        {
            player.HasShield = false;
            player.Lives = 0;
            player.IsAlive = false;
            player.TransitionToState(new DeadGhostState());
            match.NotifyPlayerDied(player, null);
        }
    }
}
