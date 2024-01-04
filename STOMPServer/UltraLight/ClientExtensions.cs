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
    using Listeners;

    public static class ClientExtensions
    {
        /// <summary>
        ///   Sends a 'CONNECT' message
        /// </summary>
        public static void Connect(this IStompClient client)
        {
            var stompMsg = new StompMessage("CONNECT");

            client.Send(stompMsg);
        }

        /// <summary>
        ///   Sends a 'SEND' message
        /// </summary>
        public static void Send(this IStompClient client, string message, string destination)
        {
            var stompMsg = new StompMessage("SEND", message);
            stompMsg["destination"] = destination;

            client.Send(stompMsg);
        }

        /// <summary>
        ///   Sends a 'SUBSCRIBE' message
        /// </summary>
        public static void Subscribe(this IStompClient client, string destination)
        {
            var stompMsg = new StompMessage("SUBSCRIBE");
            stompMsg["destination"] = destination;

            client.Send(stompMsg);
        }

        /// <summary>
        ///   Sends an 'UNSUBSCRIBE' message
        /// </summary>
        public static void UnSubscribe(this IStompClient client, string destination)
        {
            var stompMsg = new StompMessage("UNSUBSCRIBE");
            stompMsg["destination"] = destination;

            client.Send(stompMsg);
        }
    }
}