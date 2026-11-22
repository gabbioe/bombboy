using System;
using Bombboy.Models.Tiles;

namespace BombBoy.Models.State;

/// <summary>
/// Concrete State 2: Immobilized by a frost weapon/blast for a set duration (default 2 seconds).
/// Cannot move, place bombs, or use weapons while frozen. Automatically returns to HealthyState when thawed.
/// </summary>
public sealed class FrozenState : IPlayerState
{
    private float _remainingDuration;

    public FrozenState(float duration = 2.0f)
    {
        _remainingDuration = duration;
    }

    public float RemainingDuration => _remainingDuration;
    public string StateName => "Frozen";
    public string StatusEmoji => "🥶";
    public bool AllowsSpeedBonus => false;

    public bool CanMove(Player player) => false;

    public bool CanAct(Player player) => false;

    public bool HandleDamage(Player player)
    {
        bool died = player.Component.TakeDamage(player);
        if (died)
        {
            player.TransitionToState(new DeadGhostState());
        }
        else
        {
            // Explosive damage shatters the ice if the player survives
            player.TransitionToState(new HealthyState());
        }
        return died;
    }

    public void HandleFrostHit(Player player, float freezeDuration)
    {
        // Refresh freeze timer if hit by frost again while already frozen
        _remainingDuration = Math.Max(_remainingDuration, freezeDuration);
    }

    public void Update(Player player, Match match, Map map, float deltaTime)
    {
        if (!player.IsAlive)
        {
            player.TransitionToState(new DeadGhostState());
            return;
        }

        if (!player.CanPassThroughWalls && !map.CheckIfCanMove(player.X, player.Y))
        {
            player.HasShield = false;
            player.Lives = 0;
            player.IsAlive = false;
            player.TransitionToState(new DeadGhostState());
            match.NotifyPlayerDied(player, null);
            return;
        }

        _remainingDuration -= deltaTime;
        if (_remainingDuration <= 0f)
        {
            if (map.GetTile(player.X, player.Y) is WaterTile waterTile && waterTile.CanDrown())
            {
                player.TransitionToState(new DrowningState());
            }
            else
            {
                player.TransitionToState(new HealthyState());
            }
        }
    }
}
