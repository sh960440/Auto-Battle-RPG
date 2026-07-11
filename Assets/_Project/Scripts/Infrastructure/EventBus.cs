using System;
using System.Collections.Generic;

namespace Infrastructure
{
    /// <summary>
    /// Static pub/sub hub keyed by event type. Pair Subscribe with Unsubscribe in component lifecycles.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        /// <summary>
        /// Registers a handler for events of type <typeparamref name="T"/>. Duplicate handlers are ignored.
        /// </summary>
        /// <typeparam name="T">Event payload type.</typeparam>
        /// <param name="handler">Callback invoked when an event of type <typeparamref name="T"/> is published.</param>
        public static void Subscribe<T>(Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var type = typeof(T);
            if (!_subscribers.TryGetValue(type, out var handlers))
            {
                handlers = new List<Delegate>();
                _subscribers[type] = handlers;
            }

            if (!handlers.Contains(handler))
                handlers.Add(handler);
        }

        /// <summary>
        /// Removes a previously registered handler.
        /// </summary>
        /// <typeparam name="T">Event payload type.</typeparam>
        /// <param name="handler">Handler to remove.</param>
        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null)
                return;

            var type = typeof(T);
            if (!_subscribers.TryGetValue(type, out var handlers))
                return;

            handlers.Remove(handler);
            if (handlers.Count == 0)
                _subscribers.Remove(type);
        }

        /// <summary>
        /// Dispatches an event to all current subscribers of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Event payload type.</typeparam>
        /// <param name="evt">Event instance passed to each handler.</param>
        public static void Publish<T>(T evt)
        {
            if (!_subscribers.TryGetValue(typeof(T), out var handlers) || handlers.Count == 0)
                return;

            var snapshot = handlers.ToArray();
            for (var i = 0; i < snapshot.Length; i++)
                ((Action<T>)snapshot[i]).Invoke(evt);
        }

        /// <summary>
        /// Drops all subscriptions.
        /// </summary>
        public static void Clear()
        {
            _subscribers.Clear();
        }

        /// <summary>
        /// Returns how many handlers are subscribed to type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Event payload type.</typeparam>
        /// <returns>Subscriber count for the given event type.</returns>
        public static int GetSubscriberCount<T>()
        {
            return _subscribers.TryGetValue(typeof(T), out var handlers) ? handlers.Count : 0;
        }
    }
}