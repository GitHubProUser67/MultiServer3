﻿using System;
using System.Security.Cryptography;

namespace Alcatraz.DTO.Helpers
{
#if NET5_0_OR_GREATER
	public static class SecurePasswordHasher
	{
		private const int SaltSize = 8;
		private const int HashSize = 6;

		private const int HashIterations = 10000;

		private static string Hash(string password, int iterations)
		{
			// Create salt
			byte[] salt;
			using (var rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(salt = new byte[SaltSize]);
			}

			// Create hash
			byte[] hash;
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
			{
				hash = pbkdf2.GetBytes(HashSize);
			}

			// Combine salt and hash
			var hashBytes = new byte[SaltSize + HashSize];
			Array.Copy(salt, 0, hashBytes, 0, SaltSize);
			Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

			// Convert to base64
			var base64Hash = Convert.ToHexString(hashBytes);

			// Format hash with extra information
			return base64Hash;
		}

		public static string Hash(string password)
		{
			return Hash(password, HashIterations);
		}

		public static bool Verify(string password, string base64Hash)
		{
			// Get hash bytes
			var hashBytes = Convert.FromHexString(base64Hash);

			// Get salt
			var salt = new byte[SaltSize];
			Array.Copy(hashBytes, 0, salt, 0, SaltSize);

			// Create hash with given salt
			byte[] hash;
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, HashIterations))
			{
				hash = pbkdf2.GetBytes(HashSize);
			}

			// Get result
			for (var i = 0; i < HashSize; i++)
			{
				if (hashBytes[i + SaltSize] != hash[i])
				{
					return false;
				}
			}
			return true;
		}
	}
#endif
}
