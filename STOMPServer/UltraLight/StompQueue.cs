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
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using CustomLogger;
    using Listeners;
    using MessageStore;

    /// <summary>
    ///   Stomp message queue
    /// </summary>
    public class StompQueue
    {
        private class SubscriptionMetadata
        {
            public string Id { get; set; }
            public Action OnCloseHandler { get; set; }
        }

        private readonly ConcurrentDictionary<IStompClient, SubscriptionMetadata> _clients =
            new ConcurrentDictionary<IStompClient, SubscriptionMetadata>();

        public IMessageStore Store { get; private set; }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "StompQueue" /> class.
        /// </summary>
        /// <param name = "address">The address.</param>
        /// <param name = "store">The message store.</param>
        public StompQueue(string address, IMessageStore store)
        {
            if (address == null) throw new ArgumentNullException("address");
            if (!address.StartsWith("/")) address = string.Format("/{0}", address);

            Address = address;
            Store = store;
        }

        /// <summary>
        ///   Gets the address.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        ///   Triggered when the last client got removed.
        /// </summary>
        /// <value>
        ///   The on no more clients.
        /// </value>
        public Action<StompQueue> OnLastClientRemoved { get; set; }

        /// <summary>
        ///   Gets the clients.
        /// </summary>
        public IStompClient[] Clients
        {
            get { return _clients.Keys.ToArray(); }
        }

        /// <summary>
        ///   Adds the client.
        /// </summary>
        /// <param name = "client">The client.</param>
        /// <param name = "subscriptionId">The subscription id.</param>
        public void AddClient(IStompClient client, string subscriptionId)
        {
            LoggerAccessor.LogInfo(string.Format("Adding client: {0}", client.SessionId));

            if (_clients.ContainsKey(client))
            {
                LoggerAccessor.LogInfo(string.Format("Duplicate client found: {0}", client.SessionId));
                return;
            }

            Action onClose = () => RemoveClient(client);
            client.OnClose += onClose;

            if (_clients.IsEmpty)
            {
                do
                {
                    string body;
                    if (Store.TryDequeue(out body))
                    {
                        SendMessage(client, body, Guid.NewGuid(), subscriptionId);
                    }
                } while (Store.HasMessages());
            }

            _clients.TryAdd(client, new SubscriptionMetadata {Id = subscriptionId, OnCloseHandler = onClose});
        }

        /// <summary>
        ///   Removes the client.
        /// </summary>
        /// <param name = "client">The client.</param>
        public void RemoveClient(IStompClient client)
        {
            LoggerAccessor.LogInfo(string.Format("Removing client {0}", client.SessionId));

            if (!_clients.ContainsKey(client))
            {
                LoggerAccessor.LogInfo(string.Format("Client to remove not found {0}", client.SessionId));
                return;
            }

            SubscriptionMetadata meta;
            if (_clients.TryRemove(client, out meta))
                if (meta.OnCloseHandler != null) client.OnClose -= meta.OnCloseHandler;

            // raise the last client removed event if needed
            if (!_clients.Any() && OnLastClientRemoved != null)
                OnLastClientRemoved(this);
        }

        /// <summary>
        ///   Publishes the specified message to all subscribed clients.
        /// </summary>
        /// <param name = "message">The message.</param>
        public void Publish(string message)
        {
            LoggerAccessor.LogInfo(string.Format("Publishing message to queue {0}", Address));

            if (_clients.IsEmpty)
            {
                LoggerAccessor.LogInfo("No clients connected, storing message");
                Store.Enqueue(message);
                return;
            }

            var messageId = Guid.NewGuid();
            foreach (var client in _clients)
            {
                var stompClient = client.Key;
                var metadata = client.Value;
                SendMessage(stompClient, message, messageId, metadata.Id);
            }
        }

        private void SendMessage(IStompClient client, string body, Guid messageId, string subscriptionId)
        {
            LoggerAccessor.LogInfo(string.Format("Sending message to {0}", client.SessionId));
            LoggerAccessor.LogDebug(string.Format("message {0}", body));

            var stompMessage = new StompMessage("MESSAGE", body);
            stompMessage["message-id"] = messageId.ToString();
            stompMessage["destination"] = Address;

            if (!string.IsNullOrEmpty(subscriptionId))
            {
                stompMessage["subscription"] = subscriptionId;
            }

            client.Send(stompMessage);
        }
    }
}