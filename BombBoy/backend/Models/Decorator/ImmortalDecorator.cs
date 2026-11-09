using BombBoy.Models;
using System;

namespace BombBoy.Models.Decorator;

public class ImmortalDecorator : PlayerDecorator
{
    private float _duration;
    private bool _active = true;

    public ImmortalDecorator(IPlayerComponent wrapped, float duration = 10f) : base(wrapped)
    {
        _duration = duration;
    }

    public override bool HasShield => _active || _wrapped.HasShield;

    public override bool TakeDamage(Player player)
    {
        if (_active)
        {
            // Shield absorbs the blow entirely
            _active = false;
            _duration = 0;
            return false;
        }

        return _wrapped.TakeDamage(player);
    }

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
            ? $"{_wrapped.GetStatusDescription()} + Immortal({(int)Math.Ceiling(_duration)}s)"
            : _wrapped.GetStatusDescription();
    }
}
