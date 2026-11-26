using Bombboy.Models.Tiles;

namespace BombBoy.Models.State;

/// <summary>
/// Concrete State 3: Player stepped into water and is sinking.
/// Cannot place bombs/use weapons or use speed bonus; if they do not step onto dry land within 1 second,
/// they drown and transition to DeadGhostState.
/// </summary>
public sealed class DrowningState : IPlayerState
{
    private float _remainingDrownTime;

    public DrowningState(float drownDelaySeconds = 1.0f)
    {
        _remainingDrownTime = drownDelaySeconds;
    }

    public string StateName => "Drowning";
    public string StatusEmoji => "🫧";
    public bool AllowsSpeedBonus => false;

    public bool CanMove(Player player) => player.IsAlive;

    public bool CanAct(Player player) => false;

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
        player.TransitionToState(new FrozenState(freezeDuration));
    }

    public void Update(Player player, Match match, Map map, float deltaTime)
    {
        if (!player.IsAlive)
        {
            player.TransitionToState(new DeadGhostState());
            return;
        }

        if (map.GetTile(player.X, player.Y) is not WaterTile waterTile || !waterTile.CanDrown())
        {
            // Player stepped off water back onto solid ground in time
            player.TransitionToState(new HealthyState());
            return;
        }

        if (player.IsGodMode)
            return;

        _remainingDrownTime -= deltaTime;
        if (_remainingDrownTime <= 0f)
        {
            match.NotifyPlayerDrowned(player);
            player.HasShield = false;
            player.Lives = 0;
            player.IsAlive = false;
            player.TransitionToState(new DeadGhostState());
            match.NotifyPlayerDied(player, null);
        }
    }
}
