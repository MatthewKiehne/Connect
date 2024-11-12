namespace SimpleECS
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Internal;

    /// <summary>
    /// stores component data of entities that matches the archetype's type signature
    /// </summary>
    public struct Archetype : IEquatable<Archetype>, IEnumerable<Entity> 
    {
        internal Archetype(World world, int index, int version)
        {
            this.world = world; this.index = index; this.version = version;
        }

        /// <summary>
        /// returns a copy of archetype's type signature
        /// </summary>
        public TypeSignature GetTypeSignature()
            => this.TryGetArchetypeInfo(out var archetype_Info) ? new TypeSignature(archetype_Info.signature) : new TypeSignature();

        /// <summary>
        /// returns a copy of component types in this archetype
        /// </summary>
        public Type[] GetTypes()
            => this.TryGetArchetypeInfo(out var archetype_Info) ? archetype_Info.GetComponentTypes() : new Type[0];
        
        /// <summary>
        /// the world this archetype belongs to
        /// </summary>
        public readonly World world;

        /// <summary>
        /// the index and version create a unique identifier for the archetype
        /// </summary>
        public readonly int index;

        /// <summary>
        /// the index and version create a unique identifier for the archetype
        /// </summary>
        public readonly int version;

        /// <summary>
        /// [structural]
        /// creates an entity that matches this archetype
        /// </summary>
        public Entity CreateEntity()
        {
            if (this.TryGetArchetypeInfo(out var world_info, out var archetype_info))
                return world_info.StructureEvents.CreateEntity(archetype_info);
            return default;
        }

        /// <summary>
        /// returns a copy of all the entities stored in the archetype
        /// </summary>
        public Entity[] GetEntities()
        {
            Entity[] entities = new Entity[EntityCount];
            if (this.TryGetArchetypeInfo(out var archetype_info))
                for (int i = 0; i < archetype_info.entity_count; ++i)
                    entities[i] = archetype_info.entities[i];
            return entities;
        }

        /// <summary>
        /// returns the total amount of entities stored in the archetype
        /// </summary>
        public int EntityCount => this.TryGetArchetypeInfo(out var archetype_Info) ? archetype_Info.entity_count : 0;

        /// <summary>
        /// returns false if the archetype is invalid or destroyed.
        /// outputs the raw entity storage buffer.
        /// should be treated as readonly as changing values will break the ecs.
        /// only entities up to archetype's EntityCount are valid, DO NOT use the length of the array
        /// </summary>
        public bool TryGetEntityBuffer(out Entity[] entity_buffer)
        {
            if (this.TryGetArchetypeInfo(out var data))
            {
                entity_buffer = data.entities;
                return true;
            }
            entity_buffer = default;
            return false;
        }

        /// <summary>
        /// returns false if the archetype is invalid or does not store the component buffer
        /// outputs the raw component storage buffer.
        /// only components up to archetype's EntityCount are valid
        /// entities in the entity buffer that share the same index as the component in the component buffer own that component
        /// </summary>
        public bool TryGetComponentBuffer<Component>(out Component[] comp_buffer)
        {
            if (this.TryGetArchetypeInfo(out var data))
                return data.TryGetArray(out comp_buffer);
            comp_buffer = default;
            return false;
        }

        /// <summary>
        /// [structural]
        /// destroys the archetype along with all the entities within it
        /// </summary>
        public void Destroy()
        {
            if (world.IsValid())
                WorldInfo.All[world.index].data.StructureEvents.DestroyArchetype(this);
        }

        /// <summary>
        /// [structural]
        /// resizes the archetype's backing arrays to the minimum number of 2 needed to store the entities
        /// </summary>
        public void ResizeBackingArrays()
        {
            if (world.IsValid())
                WorldInfo.All[world.index].data.StructureEvents.ResizeBackingArrays(this);
        }

        bool IEquatable<Archetype>.Equals(Archetype other)
            => world == other.world && index == other.index && version == other.version;
 
        /// <summary>
        /// returns true if the archetype is not null or destroyed
        /// </summary>
        public bool IsValid()
            => world.TryGetWorldInfo(out var info) && info.archetypes[index].version == version;

        public static implicit operator bool(Archetype archetype) => archetype.IsValid();

        public override bool Equals(object obj) => obj is Archetype a ? a == this : false;

        public static implicit operator int(Archetype a) => a.index;

        public static bool operator ==(Archetype a, Archetype b) => a.world == b.world && a.index == b.index && a.version == b.version;

        public static bool operator !=(Archetype a, Archetype b) => !(a == b);

        public override int GetHashCode() => index;

        public override string ToString() => $"{(IsValid() ? "" : "~")}Arch [{GetTypeString()}]";

        string GetTypeString()
        {
            string val = "";
            if (this.TryGetArchetypeInfo(out var archetype_info))
            {
                for(int i = 0; i < archetype_info.component_count; ++ i)
                {
                    val += $" {TypeID.Get(archetype_info.component_buffers[i].type_id).Name}";
                }
            }
            return val;
        }

        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator()
        {
            if (this.TryGetArchetypeInfo(out var info))
                for(int i = 0; i < info.entity_count; ++ i)
                    yield return info.entities[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this.TryGetArchetypeInfo(out var info))
                for(int i = 0; i < info.entity_count; ++ i)
                    yield return info.entities[i];
        }
    }
}

namespace SimpleECS.Internal
{
    using System;
    using System.Collections;

    public static partial class Extensions
    {
        public static bool TryGetArchetypeInfo(this Archetype archetype, out WorldInfo world_info, out ArchetypeInfo arch_info)
        {
            if (archetype.world.TryGetWorldInfo(out  world_info))
            {
                var arch = world_info.archetypes[archetype.index];
                if (arch.version == archetype.version)
                {
                    arch_info = arch.data;
                    return true;
                }
            }
            arch_info = default;
            world_info = default;
            return false;
        }

        public static bool TryGetArchetypeInfo(this Archetype archetype, out ArchetypeInfo arch_info)
        {
            if (archetype.world.TryGetWorldInfo(out var world_info))
            {
                var arch = world_info.archetypes[archetype.index];
                if (arch.version == archetype.version)
                {
                    arch_info = arch.data;
                    return true;
                }
            }
            arch_info = default;
            return false;
        }
    }
}