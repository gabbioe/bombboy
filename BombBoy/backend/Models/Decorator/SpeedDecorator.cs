namespace BombBoy.Models.Decorator;

public class SpeedDecorator : PlayerDecorator
{
    private readonly int _bonus;
    private float _duration;
    private bool _active = true;

    public SpeedDecorator(IPlayerComponent wrapped, int bonus = 1, float duration = 10f) : base(wrapped)
    {
        _bonus = bonus;
        _duration = duration;
    }

    public override int SpeedBonus => _active ? (_wrapped.SpeedBonus + _bonus) : _wrapped.SpeedBonus;

    public override void Update(float deltaTime)
    {
        if (_active)
        {
            _duration -= deltaTime;
            if (_duration <= 0)
            {
                _active = false;
                _duration = 0;
            }
        }
        _wrapped.Update(deltaTime);
    }

    public override string GetStatusDescription()
    {
        return _active
            ? $"{_wrapped.GetStatusDescription()} + Speed({_bonus}, {(int)System.Math.Ceiling(_duration)}s)"
            : _wrapped.GetStatusDescription();
    }
}
