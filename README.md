# Exanite.Myriad.Ecs

Exanite.Myriad.Ecs is a high performance Entity-Component-System (ECS) for C#.

## Note

This repository has been heavily modified for use in Exanite.Engine (repository is currently private).
The original repository can be found here: https://github.com/martindevans/Myriad.ECS

The overall goal is to make Exanite.Myriad.Ecs a robust entity storage system where additional features can be easily implemented on top.

Major modifications include:
- Removal of phantom components
  - Cleanup should be done by tagging entities, through events, or both.
- Removal of query code
  - Exanite.Engine uses source generated query methods, similar to those found in [Arch ECS](https://github.com/genaray/Arch.Extended/wiki/Source-Generator).
- Removal of system code
  - Exanite.Engine has its own system scheduler.
- Removal of threading-related code
  - This might be added back in the future, but is not currently a priority.
- Removal of Unity support
- Addition of events
  - This allows for easier implementation of self relations, relations, data synchronization, and much more.
- Addition of ComponentRefs
  - These are strongly typed, storable references to components implemented by wrapping the Entity struct.
  - This helps with implementing things like relations and making code easier to understand.
- RefT has been renamed to ValueRef
  - ValueRef and ValueBox are part of [Exanite.Core](https://github.com/Exanite/Exanite.Core/).
- Chunks and archetypes are exposed to the user, mainly for querying purposes

Other modifications include:
- Changing the code to use code from [Exanite.Core](https://github.com/Exanite/Exanite.Core/), where applicable
- Reformatting of codebase to match conventions used in Exanite.Engine
  - Eg: Myriad.Ecs instead of Myriad.ECS

## License

This project is based on [Myriad.ECS](https://github.com/martindevans/Myriad.ECS), which is licensed under the MIT License.

All modifications made to this library are also licensed under the MIT License.
