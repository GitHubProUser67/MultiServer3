using System;

namespace MultiServer.Addons.Org.BouncyCastle.Math.Field
{
    public interface IPolynomialExtensionField
        : IExtensionField
    {
        IPolynomial MinimalPolynomial { get; }
    }
}
