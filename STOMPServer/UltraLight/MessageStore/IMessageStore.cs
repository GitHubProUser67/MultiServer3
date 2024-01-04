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

namespace Ultralight.MessageStore
{
    public interface IMessageStore
    {
        /// <summary>
        /// Enqueues the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Enqueue(string message);

        /// <summary>
        /// Tries to dequeue a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        bool TryDequeue(out string message);

        /// <summary>
        /// Determines whether this instance has messages.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has messages; otherwise, <c>false</c>.
        /// </returns>
        bool HasMessages();

        /// <summary>
        /// Counts the messages.
        /// </summary>
        /// <returns></returns>
        int CountMessages();

        /// <summary>
        /// Tries to peek the first message.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <param name="numberOfMessages">The number of messages.</param>
        /// <returns></returns>
        bool TryPeek(out string[] messages, int numberOfMessages);
    }
}