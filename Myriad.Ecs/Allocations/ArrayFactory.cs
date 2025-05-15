using System;
using System.Collections.Generic;
using System.Reflection;

namespace Exanite.Myriad.Ecs.Allocations;

internal static class ArrayFactory
{
    [ThreadStatic] private static Dictionary<Type, Func<int, Array>>? Factories;

    /// <summary>
    /// Prepare this type so that arrays of it can be constructed later
    /// </summary>
    // ReSharper disable once UnusedMember.Global (Used implicity by reflection)
    public static void Initialize<T>()
    {
        Factories ??= [];

        if (!Factories.ContainsKey(typeof(T)))
        {
            Factories.Add(typeof(T), Create<T>);
        }
    }

    /// <summary>
    /// Prepare this type so that arrays of it can be constructed later
    /// </summary>
    public static void Initialize(Type type)
    {
        typeof(ArrayFactory).GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any, [], null)!
            .MakeGenericMethod(type)
            .Invoke(null, null);
    }

    /// <summary>
    /// Create an array of the given type
    /// </summary>
    public static Array Create(Type type, int capacity)
    {
        if (Factories != null && Factories.TryGetValue(type, out var factory))
        {
            return factory(capacity);
        }

        return Array.CreateInstance(type, capacity);
    }

    private static T[] Create<T>(int capacity)
    {
        return capacity == 0 ? [] : new T[capacity];
    }
}
