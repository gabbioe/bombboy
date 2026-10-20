namespace BombBoy.Models.Observer;

public class PlayerStats
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int Kills { get; set; } = 0;
    public int Deaths { get; set; } = 0;
    public int RoundWins { get; set; } = 0;
    public int Score { get; set; } = 0;
}
