using System;
using System.Collections.Generic;

namespace Infrastructure
{
    /// <summary>
    /// Static registry for shared services.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        /// <summary>
        /// Registers a service instance for type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Service type, usually an interface.</typeparam>
        /// <param name="instance">Live instance to store.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a service of the same type is already registered.</exception>
        public static void Register<T>(T instance) where T : class
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var type = typeof(T);
            if (_services.ContainsKey(type))
                throw new InvalidOperationException($"Service already registered: {type.Name}");

            _services[type] = instance;
        }

        /// <summary>
        /// Returns a previously registered service of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Service type to resolve.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when no service of the given type is registered.</exception>
        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;

            throw new InvalidOperationException($"Service not registered: {typeof(T).Name}");
        }

        /// <summary>
        /// Attempts to resolve a service without throwing when it is missing.
        /// </summary>
        /// <typeparam name="T">Service type to resolve.</typeparam>
        /// <param name="service">Resolved instance, or null when not registered.</param>
        /// <returns><c>true</c> when the service exists; otherwise <c>false</c>.</returns>
        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Removes a registered service.
        /// </summary>
        /// <typeparam name="T">Service type to remove.</typeparam>
        /// <returns><c>true</c> when a service was removed; otherwise <c>false</c>.</returns>
        public static bool Unregister<T>() where T : class
        {
            return _services.Remove(typeof(T));
        }

        /// <summary>
        /// Clears all registered services.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }

        /// <summary>
        /// Returns whether a service of type <typeparamref name="T"/> is currently registered.
        /// </summary>
        /// <typeparam name="T">Service type to check.</typeparam>
        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }
    }
}