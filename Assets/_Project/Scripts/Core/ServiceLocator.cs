using System;
using System.Collections.Generic;
using UnityEngine;

namespace PizzaShop.Core
{
    /// <summary>
    /// Service Locator pattern implementation.
    /// Provides centralized access to all game services without tight coupling.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> services = new Dictionary<Type, IService>();
        private static bool isInitialized = false;

        /// <summary>
        /// Initialize the service locator. Should be called once at game startup.
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[ServiceLocator] Already initialized");
                return;
            }

            isInitialized = true;
            Debug.Log("[ServiceLocator] Initialized");
        }

        /// <summary>
        /// Register a service with the locator.
        /// </summary>
        public static void Register<T>(T service) where T : IService
        {
            var type = typeof(T);

            if (services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} already registered. Replacing.");
                services[type].Shutdown();
            }

            services[type] = service;
            service.Initialize();
            Debug.Log($"[ServiceLocator] Registered: {type.Name}");
        }

        /// <summary>
        /// Get a registered service. Throws exception if not found.
        /// </summary>
        public static T Get<T>() where T : IService
        {
            var type = typeof(T);

            if (services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException(
                $"[ServiceLocator] Service {type.Name} not found. Did you register it?"
            );
        }

        /// <summary>
        /// Try to get a registered service. Returns false if not found.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : IService
        {
            var type = typeof(T);

            if (services.TryGetValue(type, out var foundService))
            {
                service = (T)foundService;
                return true;
            }

            service = default;
            return false;
        }

        /// <summary>
        /// Unregister a specific service.
        /// </summary>
        public static void Unregister<T>() where T : IService
        {
            var type = typeof(T);

            if (services.TryGetValue(type, out var service))
            {
                service.Shutdown();
                services.Remove(type);
                Debug.Log($"[ServiceLocator] Unregistered: {type.Name}");
            }
        }

        /// <summary>
        /// Unregister all services. Called on application quit.
        /// </summary>
        public static void UnregisterAll()
        {
            foreach (var service in services.Values)
            {
                service.Shutdown();
            }

            services.Clear();
            isInitialized = false;
            Debug.Log("[ServiceLocator] All services unregistered");
        }

        /// <summary>
        /// Check if a service is registered.
        /// </summary>
        public static bool IsRegistered<T>() where T : IService
        {
            return services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Get count of registered services.
        /// </summary>
        public static int GetServiceCount()
        {
            return services.Count;
        }
    }
}