namespace BombBoy.Models.State;

/// <summary>
/// Concrete State 4: Terminal eliminated state for the round.
/// Player cannot move, place bombs, use weapons, or take further damage/frost effects.
/// </summary>
public sealed class DeadGhostState : IPlayerState
{
    public string StateName => "DeadGhost";
    public string StatusEmoji => "👻";
    public bool AllowsSpeedBonus => false;

    public bool CanMove(Player player) => false;

    public bool CanAct(Player player) => false;

    public bool HandleDamage(Player player) => false;

    public void HandleFrostHit(Player player, float freezeDuration)
    {
        // Dead players cannot be frozen
    }

    public void Update(Player player, Match match, Map map, float deltaTime)
    {
        player.IsAlive = false;
        player.Lives = 0;
    }
}
