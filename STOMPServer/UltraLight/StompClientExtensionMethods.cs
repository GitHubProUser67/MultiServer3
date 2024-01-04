// Copyright 2011 Ernst Naezer, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

namespace Ultralight
{
    using System;
    using Listeners;

    public static class StompClientExtensionMethods
    {
        /// <summary>
        /// Determines whether the specified stomp client is connected.
        /// </summary>
        /// <param name="stompClient">The stomp client.</param>
        /// <returns>
        ///   <c>true</c> if the specified stomp client is connected; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsConnected(this IStompClient stompClient)
        {
            return stompClient.SessionId != Guid.Empty;
        }
    }
}