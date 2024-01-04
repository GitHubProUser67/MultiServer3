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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CustomLogger;
    using Listeners;
    using MessageStore;

    /// <summary>
    ///   A small and light STOMP message broker
    /// </summary>
    public class StompServer
    {
        private readonly IDictionary<string, Action<IStompClient, StompMessage>> _actions;
        private readonly List<IStompListener> _listener = new List<IStompListener>();
        private readonly List<StompQueue> _queues = new List<StompQueue>();

        private Func<IMessageStore> _messageStoreBuilder = () => new InMemoryMessageStore();

        public void SetMessageStore<T>()
            where T : IMessageStore, new()
        {
            _messageStoreBuilder = () => new T();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StompServer"/> class.
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <param name="messageStoreType">Type of the message store.</param>
        public StompServer(params IStompListener[] listener)
        {
            if (listener == null) throw new ArgumentNullException("listener");
            _listener.AddRange(listener);

            _actions = new Dictionary<string, Action<IStompClient, StompMessage>>
                           {
                               {"CONNECT", OnStompConnect},
                               {"SUBSCRIBE", OnStompSubscribe},
                               {"UNSUBSCRIBE", OnStompUnsubscribe},
                               {"SEND", OnStompSend},
                               {"DISCONNECT", OnStompDisconnect},
                           };
        }

        /// <summary>
        ///   Gets the queues.
        /// </summary>
        public StompQueue[] Queues
        {
            get { return _queues.ToArray(); }
        }

        /// <summary>
        ///   Starts this instance.
        /// </summary>
        public void Start()
        {
            _listener.ForEach(
                l =>
                    {
                        // attach to listener events
                        l.OnConnect += client =>
                                           {
                                               client.OnMessage += msg => OnClientMessage(client, msg);
                                               client.OnClose += () => client.OnClose = null;
                                           };

                        l.Start();
                    });
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            _queues.ForEach(queue => queue.Clients.ToList().ForEach(client => client.Close()));
            _queues.Clear();
            _listener.ForEach(l => l.Stop());
        }

        /// <summary>
        ///   Excutes the action assigned to the message command
        /// </summary>
        /// <param name = "client"></param>
        /// <param name = "message"></param>
        private void OnClientMessage(IStompClient client, StompMessage message)
        {
            if ( message == null || message.Command == null) return;

            if(client.SessionId == Guid.Empty)
            {
                client.SessionId= Guid.NewGuid();
            }

            LoggerAccessor.LogInfo(string.Format("Processing command: {0} from client {1}", message.Command, client.SessionId));
            
            if (!_actions.ContainsKey(message.Command))
            {
                LoggerAccessor.LogWarn( string.Format("Client {0} sended an unknown command: {1}", client.SessionId, message.Command));
                return;
            }

            if (message.Command != "CONNECT" && client.IsConnected() == false)
            {
                LoggerAccessor.LogInfo(string.Format("Client {0} was not connected before sending command: {1}", client.SessionId, message.Command));

                client.Send(new StompMessage("ERROR", "Please connect before sending '" + message.Command + "'"));
                return;
            }

            _actions[message.Command](client, message);

            // when a receipt is request, we send a receipt frame
            if (message.Command == "CONNECT" || message["receipt"] == string.Empty) return;
            var response = new StompMessage("RECEIPT");
            response["receipt-id"] = message["receipt"];
            client.Send(response);
        }

        /// <summary>
        /// Handles the CONNECT message
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        private static void OnStompConnect(IStompClient client, StompMessage message)
        {
            var result = new StompMessage("CONNECTED");

            result["session-id"] = client.SessionId.ToString();

            client.Send(result);
        }

        /// <summary>
        /// Handles the SUBSCRIBE message
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        private void OnStompSubscribe(IStompClient client, StompMessage message)
        {
            string destination = message["destination"];

            var queue = _queues.FirstOrDefault(s => s.Address == destination) ?? AddNewQueue(destination);

            queue.AddClient(client, message["id"]);
        }

        /// <summary>
        /// Handles the UNSUBSCRIBE message
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        private void OnStompUnsubscribe(IStompClient client, StompMessage message)
        {
            string destination = message["destination"];

            if (string.IsNullOrEmpty(destination)) return;
            var queue = _queues.FirstOrDefault(q => q.Address == destination);
            if (queue == null || queue.Clients.Contains(client) == false)
            {
                client.Send(new StompMessage("ERROR", "You are not subscribed to queue '" + destination + "'"));
                return;
            }

            queue.RemoveClient(client);
        }

        /// <summary>
        /// Handles the SEND message
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        private void OnStompSend(IStompClient client, StompMessage message)
        {
            var destination = message["destination"];

            var queue = _queues.FirstOrDefault(s => s.Address == destination) ?? AddNewQueue(destination);

            queue.Publish(message.Body);
        }

        /// <summary>
        /// Handles the DISCONNECT message
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        public void OnStompDisconnect(IStompClient client, StompMessage message)
        {
            var stompQueues = _queues.Where(q => q.Clients.Contains(client)).ToList();
            stompQueues.ForEach(q => q.RemoveClient(client));
        }

        /// <summary>
        /// Adds the new queue.
        /// </summary>
        /// <param name="destination">The queue name.</param>
        /// <returns></returns>
        private StompQueue AddNewQueue(string destination)
        {
            var queue = new StompQueue(destination, _messageStoreBuilder())
            {
                OnLastClientRemoved =
                    q =>
                    {
                        q.OnLastClientRemoved = null;
                        lock (this)
                        {
                            _queues.Remove(q);
                        }
                    }
            };

            lock (this)
            {
                _queues.Add(queue);
            }
            return queue;
        }

    }
}