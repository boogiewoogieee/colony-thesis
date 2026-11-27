using System;
using System.Collections.Generic;

namespace Colony.Core
{
    /// <summary>
    /// Lets modules react to each other without referencing each other: one publishes a
    /// message (a small struct such as <c>DomeActivated</c>), the others subscribe to its type.
    /// Runs on the main thread only.
    /// </summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, Delegate> _handlersByMessageType =
            new Dictionary<Type, Delegate>();

        public void Subscribe<TMessage>(Action<TMessage> handler)
            where TMessage : struct
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handlersByMessageType.TryGetValue(typeof(TMessage), out Delegate existingHandlers);
            _handlersByMessageType[typeof(TMessage)] = Delegate.Combine(existingHandlers, handler);
        }

        /// <summary>Removes a handler. Removing one that was never subscribed does nothing.</summary>
        public void Unsubscribe<TMessage>(Action<TMessage> handler)
            where TMessage : struct
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!_handlersByMessageType.TryGetValue(typeof(TMessage), out Delegate existingHandlers))
            {
                return;
            }

            Delegate remainingHandlers = Delegate.Remove(existingHandlers, handler);
            if (remainingHandlers == null)
            {
                _handlersByMessageType.Remove(typeof(TMessage));
                return;
            }

            _handlersByMessageType[typeof(TMessage)] = remainingHandlers;
        }

        /// <summary>
        /// Delivers the message to every handler subscribed to its type at this moment,
        /// in the order they subscribed.
        /// </summary>
        public void Publish<TMessage>(TMessage message)
            where TMessage : struct
        {
            if (_handlersByMessageType.TryGetValue(typeof(TMessage), out Delegate handlers))
            {
                ((Action<TMessage>)handlers).Invoke(message);
            }
        }
    }
}
