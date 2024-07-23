using HashLib;
using System;
using System.Linq;
#if NET5_0_OR_GREATER
using ILGPU;
using ILGPU.Runtime;
#endif

namespace CastleLibrary.Utils.Hash
{
    public class NetHasher
    {
#if NET5_0_OR_GREATER
        private static readonly Context context = Context.CreateDefault();
        private static readonly Device device = context.Devices.FirstOrDefault();
#endif
        private static readonly IHash md5hasher = HashFactory.Crypto.BuildIn.CreateMD5CryptoServiceProvider();
        private static readonly IHash sha1hasher = HashFactory.Crypto.BuildIn.CreateSHA1Managed();
        private static readonly IHash sha224hasher = HashFactory.Crypto.CreateSHA224();
        private static readonly IHash sha256hasher = HashFactory.Crypto.BuildIn.CreateSHA256Managed();
        private static readonly IHash sha384hasher = HashFactory.Crypto.BuildIn.CreateSHA384Managed();
        private static readonly IHash sha512hasher = HashFactory.Crypto.BuildIn.CreateSHA512Managed();

        public static byte[] ComputeMD5(object input)
        {
            byte[] result = md5hasher.ComputeObject(input).GetBytes();

            if (result.Length != 16)
                throw new InvalidOperationException("The computed MD5 hash is not 16 bytes long.");

            return result;
        }

        public static string ComputeMD5StringWithCleanup(object input)
        {
            return BitConverter.ToString(ComputeMD5(input)).Replace("-", string.Empty);
        }

        public static byte[] ComputeSHA1(object input)
        {
            byte[] result = sha1hasher.ComputeObject(input).GetBytes();

            if (result.Length != 20)
                throw new InvalidOperationException("The computed SHA1 hash is not 20 bytes long.");

            return result;
        }

        public static string ComputeSHA1StringWithCleanup(object input)
        {
            return BitConverter.ToString(ComputeSHA1(input)).Replace("-", string.Empty);
        }

        public static byte[] ComputeSHA224(object input)
        {
            byte[] result = sha224hasher.ComputeObject(input).GetBytes();

            if (result.Length != 28)
                throw new InvalidOperationException("The computed SHA224 hash is not 28 bytes long.");

            return result;
        }

        public static string ComputeSHA224StringWithCleanup(object input)
        {
            return BitConverter.ToString(ComputeSHA224(input)).Replace("-", string.Empty);
        }

        public static byte[] ComputeSHA256(object input)
        {
            byte[] result = Array.Empty<byte>();

#if NET5_0_OR_GREATER
            if (device != null && input is byte[] byteInput)
            {
#if DEBUG
                CustomLogger.LoggerAccessor.LogWarn($"[NetHasher] - Starting SHA256 task on the GPU: {device.Name}");
#endif
                result = ILGPUModels.Sha256.ComputeSHA256(context, device, byteInput);
            }
            else
#endif
                result = sha256hasher.ComputeObject(input).GetBytes();

            if (result.Length != 32)
                throw new InvalidOperationException("The computed SHA256 hash is not 32 bytes long.");

            return result;
        }

        public static string ComputeSHA256StringWithCleanup(object input)
        {
            return BitConverter.ToString(ComputeSHA256(input)).Replace("-", string.Empty);
        }

        public static byte[] ComputeSHA384(object input)
        {
            byte[] result = sha384hasher.ComputeObject(input).GetBytes();

            if (result.Length != 48)
                throw new InvalidOperationException("The computed SHA384 hash is not 48 bytes long.");

            return result;
        }

        public static string ComputeSHA384StringWithCleanup(object input)
        {
            return BitConverter.ToString(ComputeSHA384(input)).Replace("-", string.Empty);
        }

        public static byte[] ComputeSHA512(object input)
        {
            byte[] result = sha512hasher.ComputeObject(input).GetBytes();

            if (result.Length != 64)
                throw new InvalidOperationException("The computed SHA512 hash is not 64 bytes long.");

            return result;
        }

        public static string ComputeSHA512StringWithCleanup(object input)
        {
            return BitConverter.ToString(ComputeSHA512(input)).Replace("-", string.Empty);
        }
    }
}
