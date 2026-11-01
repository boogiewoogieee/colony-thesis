using System;
using System.Collections.Generic;

namespace Colony.Core
{
    /// <summary>
    /// Typed publish/subscribe for simulation events, so modules can react to each other
    /// without referencing each other. Events are small structs, e.g. <c>DomeActivated</c>.
    /// Not thread-safe; the simulation runs on the main thread.
    /// </summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handlers.TryGetValue(typeof(T), out var existing);
            _handlers[typeof(T)] = Delegate.Combine(existing, handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null || !_handlers.TryGetValue(typeof(T), out var existing))
            {
                return;
            }

            var remaining = Delegate.Remove(existing, handler);
            if (remaining == null)
            {
                _handlers.Remove(typeof(T));
            }
            else
            {
                _handlers[typeof(T)] = remaining;
            }
        }

        /// <summary>Delivers the event to every current subscriber of its type.</summary>
        public void Publish<T>(T evt) where T : struct
        {
            if (_handlers.TryGetValue(typeof(T), out var handlers))
            {
                ((Action<T>)handlers).Invoke(evt);
            }
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
