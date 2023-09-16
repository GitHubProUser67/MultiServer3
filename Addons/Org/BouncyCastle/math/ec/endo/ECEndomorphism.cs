using System;

namespace MultiServer.Addons.Org.BouncyCastle.Math.EC.Endo
{
    public interface ECEndomorphism
    {
        ECPointMap PointMap { get; }

        bool HasEfficientPointMap { get; }
    }
}
