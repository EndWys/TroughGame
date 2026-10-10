# Network Entities

The GameCore NetworkEntities feature owns entity IDs, factories, spawning and
the runtime registry. These project IDs are distinct from Fusion's NetworkId.

## ID protocol

`NetworkEntityIdData` is an immutable value containing one `uint`. The high
6 bits identify the entity type; the low 26 bits identify the entity within
that type. Indexes range from 0 to 67,108,863 inclusive.

`NetworkEntityType` declares stable protocol codes: None = 0, Player = 1,
Enemy = 2. There is room for 63 nonzero type codes, but only explicitly
supported types are accepted. Do not renumber existing codes. Add new types to
the enum and `NetworkEntityIdValidation.IsSupportedType` together.

The complete value 0 is `NetworkEntityIdData.None`, also the default struct
value. Type zero with a nonzero index is invalid. Actual entities cannot be
assigned None. Index zero is valid for a supported nonzero type.

Construct from a type and index to pack a value, or from a received `uint` to
validate and decode it. Type and Index are computed properties. Equality and
hashing use Value; ToString is for diagnostics only. Validation and errors live
in the feature's static `NetworkEntityIdValidation` and `NetworkEntityIdErrors`.

## Lifetime and authority

`NetworkEntityIdFactory` maintains a monotonically increasing counter per type
in the owning gameplay context. The spawning authority issues IDs; peers read
replicated IDs rather than reconstructing them from local counters.

Despawn removes registry entries without recycling IDs. Exhaustion throws
before advancing the counter and never wraps into another type. Recreating the
world/context starts new counters; old registry entries and entity references
must not survive that boundary. The ID does not include a world identifier or
generation and must not be used as a persistent cross-world identity.

A well-formed ID does not mean its entity still exists. Use
`NetworkEntityRegistry.TryGet` to resolve a live entity.

## Fusion state

`BaseNetworkEntityRoot` replicates one uint NetworkEntityIdValue.
`EnemyNetworkEntityComponent` replicates one uint TargetEntityIdValue. Their
public APIs expose NetworkEntityIdData; clearing a target writes None.Value.

This replaces the previous NetworkString<_32> type plus int index pair. The
change alters the network state layout: communicating peers must run the same
version. It does not promise a fixed packet size or measured bandwidth saving.
