/*
 *   Mentalis.org Security Library
 * 
 *     Copyright � 2002-2005, The Mentalis.org Team
 *     All rights reserved.
 *     http://www.mentalis.org/
 *
 *
 *   Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions
 *   are met:
 *
 *     - Redistributions of source code must retain the above copyright
 *        notice, this list of conditions and the following disclaimer. 
 *
 *     - Neither the name of the Mentalis.org Team, nor the names of its contributors
 *        may be used to endorse or promote products derived from this
 *        software without specific prior written permission. 
 *
 *   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 *   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 *   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 *   FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
 *   THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 *   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 *   (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 *   SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 *   HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 *   STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 *   ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 *   OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System.Security.Cryptography;

namespace Org.Mentalis.Security.Cryptography
{
    /// <summary>
    /// Represents the abstract class from which all implementations of the <see cref="RIPEMD160"/> hash algorithm inherit.
    /// </summary>
    public abstract class RIPEMD160 : HashAlgorithm {
		/// <summary>
		/// Initializes a new instance of <see cref="RIPEMD160"/>.
		/// </summary>
		/// <remarks>You cannot create an instance of an abstract class. Application code will create a new instance of a derived class.</remarks>
		protected RIPEMD160() {
			this.HashSizeValue = 160;
		}
		/// <summary>
		/// Creates an instance of the default implementation of the <see cref="RIPEMD160"/> hash algorithm.
		/// </summary>
		/// <returns>A new instance of the RIPEMD160 hash algorithm.</returns>
		public static new RIPEMD160 Create () {
			return Create("RIPEMD160");
		}
		/// <summary>
		/// Creates an instance of the specified implementation of the <see cref="RIPEMD160"/> hash algorithm.
		/// </summary>
		/// <param name="hashName">The name of the specific implementation of RIPEMD160 to use.</param>
		/// <returns>A new instance of the specified implementation of RIPEMD160.</returns>
		public static new RIPEMD160 Create (string hashName) {
			try {
				if (hashName.ToUpper() == "RIPEMD160" || hashName.ToUpper() == "RIPEMD" || hashName.ToLower() == "org.mentalis.security.cryptography.ripemd160")
					return new RIPEMD160Managed();
			} catch {}
			return null;
		}
	}
}