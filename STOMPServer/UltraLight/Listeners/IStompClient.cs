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

namespace Ultralight.Listeners
{
    using System;

    public interface IStompClient
           : IComparable<IStompClient>
    {
        /// <summary>
        ///   The client disconnected
        /// </summary>
        Action OnClose { get; set; }

        /// <summary>
        /// Client ID
        /// </summary>
        Guid SessionId { get; set; }

        /// <summary>
        ///   A message from the client is received
        /// </summary>
        Action<StompMessage> OnMessage { get; set; }

        /// <summary>
        ///   Sends a message to the client
        /// </summary>
        /// <param name = "message"></param>
        void Send(StompMessage message);

        /// <summary>
        /// Closes this instance.
        /// </summary>
        void Close();
    }
}