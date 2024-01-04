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

    public class StompInMemoryClient
        : IStompClient
    {
        public Action OnClose { get; set; }
        public Guid SessionId { get; set; }

        public Action<StompMessage> OnSend { get; set; }
        public Action<StompMessage> OnMessage { get; set; }

        public void Send(StompMessage message)
        {
            if (OnSend != null) OnSend(message);
        }

        public void Close()
        {
        }

        public int CompareTo(IStompClient other)
        {
            return SessionId.CompareTo(other.SessionId);
        }

        public bool Equals(StompInMemoryClient other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || other.SessionId.Equals(SessionId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof (StompInMemoryClient) && Equals((StompInMemoryClient) obj);
        }

        public override int GetHashCode()
        {
            return SessionId.GetHashCode();
        }
    }
}