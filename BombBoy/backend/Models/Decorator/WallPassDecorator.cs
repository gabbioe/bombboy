namespace BombBoy.Models.Decorator;

public class WallPassDecorator : PlayerDecorator
{
    private float _duration;
    private bool _active = true;

    public WallPassDecorator(IPlayerComponent wrapped, float duration = 10f) : base(wrapped)
    {
        _duration = duration;
    }

    public override bool CanPassThroughWalls => _active || _wrapped.CanPassThroughWalls;

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
            ? $"{_wrapped.GetStatusDescription()} + WallPass({(int)System.Math.Ceiling(_duration)}s)"
            : _wrapped.GetStatusDescription();
    }
}
