namespace BombBoy.Models.State;

/// <summary>
/// State Pattern interface representing a player's condition state in the match.
/// </summary>
public interface IPlayerState
{
    string StateName { get; }
    string StatusEmoji { get; }
    bool AllowsSpeedBonus { get; }

    bool CanMove(Player player);
    bool CanAct(Player player);
    bool HandleDamage(Player player);
    void HandleFrostHit(Player player, float freezeDuration);
    void Update(Player player, Match match, Map map, float deltaTime);
}
