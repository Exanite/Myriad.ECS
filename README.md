# Exanite.Myriad.Ecs

Exanite.Myriad.Ecs is a high performance Entity-Component-System (ECS) for C#.

This is a fork of Myriad.ECS, which can be found here: https://github.com/martindevans/Myriad.ECS \
This repository has been heavily modified for use in Exanite.Engine (engine is currently private).

Also note, this ECS is not yet battle-tested. Many changes were made to the ECS recently (as of February 2026), and while I consider this ECS feature complete, there can still be many smaller issues that are yet to be discovered and fixed. Also, no benchmarks yet. Performance is not my current focus.

## Design goals

- Focus on providing a fully featured entity storage, but not much more
  - Entity manipulation (create entity/destroy entity/set component/remove component)
  - Events (see the [event design section](#event-design))
  - Light relation support
    - Implemented through EcsRefs, which are strongly typed, storable references to components
    - Events can also facilitate the implementation of relations.
  - Entity copying support
    - Can be used to implement prefabs and world snapshots.
  - Queries, systems, threading, and serialization are considered high level features and should be implemented separately.
- Strong performance focus, but without sacrificing simplicity, safety, or functionality
  - This means that this ECS is likely slower than other ECS's available.
  - Components are stored as managed structs instead of unmanaged memory.
  - All entity-focused structural modifications must be done through the command buffer.
  - TryGet methods for getting components.
  - The ComponentDispatcher can simplify invoking generic methods when given only a ComponentId.
- Support for only the latest .NET
  - This repository is primarily meant for Exanite.Engine, which currently uses .NET 10.
  - Notably, no Unity support.

## Differences from Myriad.ECS

Major modifications:
- Removal of "high-level" code
  - Eg: Queries, systems, and threading
  - Exanite.Engine uses source generated query methods, similar to those found in [Arch ECS](https://github.com/genaray/Arch.Extended/wiki/Source-Generator).
  - Exanite.Engine also has a custom system scheduler, but does not support multithreading yet.
- Removal of phantom components
  - Use events or tagging instead.
- Buffered entities do not need to be resolved
  - Entity IDs are reserved as soon as an entity is pending creation.
- Archetypes and chunks are exposed to the user
  - This is to allow for querying systems to be implemented on top of the base ECS.

Other modifications include:
- Changing the code to use code from [Exanite.Core](https://github.com/Exanite/Exanite.Core/), where applicable.
- Reformatting of codebase to match conventions used in Exanite.Engine
  - Eg: Myriad.Ecs instead of Myriad.ECS
  - Eg: RefT has been renamed to Ref

## Event design

The following events are provided:
- Entity created - Raised when the entity is created, after its components are set
- Entity destroyed - Raised when the entity is destroyed, before its components are removed
- Component added - Raised when the component is set on an entity that never had the component
- Component modified - Raised when the component is set on an entity that already had the component
  - Modifications from outside the command buffer don't trigger this event.
- Component copied - Raised when the component is copied from an existing entity to another entity
  - Copied will be called before added or modified.
- Component removed - Raised when the component is removed from an entity

There are 2 ways of receiving events:
1. Implementing the corresponding interface on the component
2. Subscribing to the world event bus

The component interface callbacks are intentionally simpler than the events raised by the event bus.

This is because components should remain simple data storage containers. \
The component events should be used to maintain data consistency.

The intent is that systems subscribe to the event bus and implement component behavior that way. \
To facilitate this, the events raised by the event bus provide a command buffer that can be used to further modify the world.

## License

This project is based on [Myriad.ECS](https://github.com/martindevans/Myriad.ECS), which is licensed under the MIT License.

All modifications made to this library by William Chen (Exanite) are also licensed under the MIT License.
