using System.Numerics;

namespace Exanite.Myriad.Ecs.Benchmarks.Components;

public record struct ComponentByte(byte Value) : IComponent;
public record struct ComponentInt16(short Value) : IComponent;
public record struct ComponentFloat(float Value) : IComponent;
public record struct ComponentInt32(int Value) : IComponent;
public record struct ComponentInt64(long Value) : IComponent;

public record struct Position(Vector2 Value) : IComponent;
public record struct Velocity(Vector2 Value) : IComponent;
