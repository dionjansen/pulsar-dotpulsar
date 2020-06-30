/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace DotPulsar.Internal
{
    using Abstractions;
    using Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMessageQueue<T> : IDequeue<T>
    {
        ValueTask Acknowledge(T obj);
        ValueTask NegativeAcknowledge(T obj);
    }

    public interface IMessageAcksTracker<T>
    {
        ValueTask<T> Add(T message);
        ValueTask Ack(T message);
        ValueTask Nack(T message);
    }

    public sealed class MessageQueue<T> : IMessageQueue<T>
    {
        private readonly AsyncQueue<T> _queue;
        private readonly IMessageAcksTracker<T> _tracker;

        public MessageQueue(
            AsyncQueue<T> queue,
            IMessageAcksTracker<T> tracker
        )
        {
            _queue = queue;
            _tracker = tracker;

        }

        public async ValueTask<T> Dequeue(CancellationToken cancellationToken = default)
        {
            var message = await _queue.Dequeue(cancellationToken).ConfigureAwait(false);
            return await _tracker.Add(message).ConfigureAwait(false); // Tracker should start the ack timeout
        }

        public ValueTask Acknowledge(T obj) => _tracker.Ack(obj);
        public ValueTask NegativeAcknowledge(T obj) => _tracker.Nack(obj);

    }
}
