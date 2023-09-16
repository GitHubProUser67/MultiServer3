using System;

namespace MultiServer.Addons.Org.BouncyCastle.Math.EC.Multiplier
{
    public interface IPreCompCallback
    {
        PreCompInfo Precompute(PreCompInfo existing);
    }
}
