using System;
using HashLib;
using Tpm2Lib;

namespace NetHasher
{
    public class DotNetHasher
    {
        public static byte[] ComputeMD5(object input)
        {
            byte[] result = HashFactory.Crypto.BuildIn.CreateMD5CryptoServiceProvider().ComputeObject(input).GetBytes();

            if (result.Length != 16)
                throw new InvalidOperationException("The computed MD5 hash is not 16 bytes long.");

            return result;
        }

        public static string ComputeMD5String(object input)
        {
            return BitConverter.ToString(ComputeMD5(input)).Replace("-", string.Empty);
        }

        public static byte[] ComputeSHA1(object input)
        {
            byte[] result = null;
            Tpm2 _tpm = null;

            if (input is byte[])
            {
                try
                {
                    TbsDevice _crypto_device = new TbsDevice();
                    _crypto_device.Connect();
                    _tpm = new Tpm2(_crypto_device);

                    result = _tpm.Hash((byte[])input,
                           TpmAlgId.Sha1,
                           TpmRh.Owner,
                           out _);
                }
                catch
                {
                    // Fallback to classic HashFactory Methods.
                }
                finally
                {
                    if (_tpm != null) _tpm.Dispose();
                }
            }

            if (result == null)
                result = HashFactory.Crypto.BuildIn.CreateSHA1Managed().ComputeObject(input).GetBytes();

            if (result.Length != 20)
                throw new InvalidOperationException("The computed SHA1 hash is not 20 bytes long.");

            return result;
        }

        public static string ComputeSHA1String(object input)
        {
            return BitConverter.ToString(ComputeSHA1(input)).Replace("-", string.Empty);
        }

        public static byte[] ComputeSHA224(object input)
        {
            byte[] result = HashFactory.Crypto.CreateSHA224().ComputeObject(input).GetBytes();

            if (result.Length != 28)
                throw new InvalidOperationException("The computed SHA224 hash is not 28 bytes long.");

            return result;
        }

        public static string ComputeSHA224String(object input)
        {
            return BitConverter.ToString(ComputeSHA224(input)).Replace("-", string.Empty);
        }

        public static byte[] ComputeSHA256(object input)
        {
            byte[] result = null;
            Tpm2 _tpm = null;

            if (input is byte[])
            {
                try
                {
                    TbsDevice _crypto_device = new TbsDevice();
                    _crypto_device.Connect();
                    _tpm = new Tpm2(_crypto_device);

                    result = _tpm.Hash((byte[])input,
                           TpmAlgId.Sha256,
                           TpmRh.Owner,
                           out _);
                }
                catch
                {
                    // Fallback to classic HashFactory Methods.
                }
                finally
                {
                    if (_tpm != null) _tpm.Dispose();
                }
            }

            if (result == null)
                result = HashFactory.Crypto.BuildIn.CreateSHA256Managed().ComputeObject(input).GetBytes();

            if (result.Length != 32)
                throw new InvalidOperationException("The computed SHA256 hash is not 32 bytes long.");

            return result;
        }

        public static string ComputeSHA256String(object input)
        {
            return BitConverter.ToString(ComputeSHA256(input)).Replace("-", string.Empty);
        }

        public static byte[] ComputeSHA384(object input)
        {
            byte[] result = null;
            Tpm2 _tpm = null;

            if (input is byte[])
            {
                try
                {
                    TbsDevice _crypto_device = new TbsDevice();
                    _crypto_device.Connect();
                    _tpm = new Tpm2(_crypto_device);

                    result = _tpm.Hash((byte[])input,
                           TpmAlgId.Sha384,
                           TpmRh.Owner,
                           out _);
                }
                catch
                {
                    // Fallback to classic HashFactory Methods.
                }
                finally
                {
                    if (_tpm != null) _tpm.Dispose();
                }
            }

            if (result == null)
                result = HashFactory.Crypto.BuildIn.CreateSHA384Managed().ComputeObject(input).GetBytes();

            if (result.Length != 48)
                throw new InvalidOperationException("The computed SHA384 hash is not 48 bytes long.");

            return result;
        }

        public static string ComputeSHA384String(object input)
        {
            return BitConverter.ToString(ComputeSHA384(input)).Replace("-", string.Empty);
        }

        public static byte[] ComputeSHA512(object input)
        {
            byte[] result = null;
            Tpm2 _tpm = null;

            if (input is byte[])
            {
                try
                {
                    TbsDevice _crypto_device = new TbsDevice();
                    _crypto_device.Connect();
                    _tpm = new Tpm2(_crypto_device);

                    result = _tpm.Hash((byte[])input,
                           TpmAlgId.Sha512,
                           TpmRh.Owner,
                           out _);
                }
                catch
                {
                    // Fallback to classic HashFactory Methods.
                }
                finally
                {
                    if (_tpm != null) _tpm.Dispose();
                }
            }

            if (result == null)
                result = HashFactory.Crypto.BuildIn.CreateSHA512Managed().ComputeObject(input).GetBytes();

            if (result.Length != 64)
                throw new InvalidOperationException("The computed SHA512 hash is not 64 bytes long.");

            return result;
        }

        public static string ComputeSHA512String(object input)
        {
            return BitConverter.ToString(ComputeSHA512(input)).Replace("-", string.Empty);
        }
    }
}
