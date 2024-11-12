namespace SimpleECS
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Internal;

    /// <summary>
    /// manages all entities and archetype information
    /// </summary>
    public partial struct World : IEquatable<World>, IEnumerable<Archetype>
    {
        World(int index, int version)
        {
            this.index = index; this.version = version;
        }

        /// <summary>
        /// the index and version together create a unique identifier for the world
        /// </summary>
        public readonly int index;

        /// <summary>
        /// the index and version together create a unique identifier for the world
        /// </summary>
        public readonly int version;

        /// <summary>
        /// Name of the world
        /// </summary>
        public string Name
        {
            get => this.TryGetWorldInfo(out var info) ? info.name : "~World";
            set
            {
                if (this.TryGetWorldInfo(out var info))
                    info.name = value;
            }
        }

        /// <summary>
        /// Returns a copy of all the archetypes in the current world
        /// </summary>
        public Archetype[] GetArchetypes()
        {
            Archetype[] archetypes;
            if (this.TryGetWorldInfo(out var info))
            {
                archetypes = new Archetype[info.archetype_count];
                int count = 0;
                foreach (var arch in info.archetypes)
                    if (arch.data != null)
                    {
                        archetypes[count] = arch.data.archetype;
                        count++;
                    }
            }
            else
                archetypes = new Archetype[0];
            return archetypes;
        }

        /// <summary>
        /// Returns a copy of all the entities in the the current world
        /// </summary>
        public Entity[] GetEntities()
        {
            Entity[] entities;
            if (this.TryGetWorldInfo(out var info))
            {
                entities = new Entity[EntityCount];
                int count = 0;
                foreach (var archetype in info.archetypes)
                {
                    if (archetype.data != null)
                    {
                        for (int e = 0; e < archetype.data.entity_count; ++e)
                        {
                            entities[count] = archetype.data.entities[e];
                            count++;
                        }
                    }
                }
            }
            else
            {
                entities = new Entity[0];
            }
                
            return entities;
        }

        /// <summary>
        /// Creates a new world
        /// </summary>
        public static World Create() => Create("World");

        /// <summary>
        /// Gets world with name, else creates and returns a world with name
        /// </summary>
        public static World GetOrCreate(string name)
        {
            if (!TryGetWorld(name, out var world))
                return Create(name);
            return world;
        }

        /// <summary>
        /// Tries to get world with name
        /// </summary>
        /// <param name="name">name of the target world</param>
        /// <param name="target_world">target world</param>
        /// <returns>returns false if not found</returns>
        public static bool TryGetWorld(string name, out World target_world)
        {
            foreach (var current in GetAll())
                if (current.Name == name)
                {
                    target_world = current;
                    return true;
                }
            target_world = default;
            return false;
        }

        /// <summary>
        /// Creates an new world with Name
        /// </summary>
        public static World Create(string Name)
        {
            var index = -1;
            for (int i = 0; i < WorldInfo.All.Length; ++i)
            {
                if (WorldInfo.All[i].data == null)
                {
                    index = i;
                    break;
                }
            }
            if (index < 0)
            {
                index = WorldInfo.All.Length;
                System.Array.Resize(ref WorldInfo.All, index + 4);
            }

            ref var world_data = ref WorldInfo.All[index];
            var version = world_data.version;
            world_data.data = new WorldInfo(Name, new World(index, version));
            WorldInfo.world_count++;
            return world_data.data.world;
        }

        /// <summary>
        /// Returns true if the world is not null or destroyed
        /// </summary>
        public bool IsValid() => WorldInfo.All[index].version == version;

        /// <summary>
        /// Destroys the world along with all it's archetypes and entities
        /// </summary>
        public void Destroy()
        {
            if (this.TryGetWorldInfo(out var info))
                info.StructureEvents.DestroyWorld();
        }

        /// <summary>
        /// [structural]
        /// Creates an entity in this world
        /// </summary>
        public Entity CreateEntity()
        {
            if (this.TryGetWorldInfo(out var info))
                return info.StructureEvents.CreateEntity(info.GetArchetypeData(info.buffer_signature.Clear()));
            return default;
        }

        /// <summary>
        /// Returns how many entities are currently in this world
        /// </summary>
        public int EntityCount => this.TryGetWorldInfo(out var info) ? info.entity_count : 0;

        /// <summary>
        /// Creates a query that operates on this world
        /// </summary>
        public Query CreateQuery() => new Query(this);

        /// <summary>
        /// Tries to get the archetype that matches the supplied TypeSignature.
        /// Returns false if the world is destroyed or null
        /// </summary>
        public bool TryGetArchetype(out Archetype archetype, TypeSignature signature)
        {
            if (this.TryGetWorldInfo(out var info))
            {
                archetype = info.GetArchetypeData(signature).archetype;
                return true;
            }
            archetype = default;
            return false;
        }

        /// <summary>
        /// Tries to get an archetype that has the supplied types.
        /// Returns false if the world is destroyed or null
        /// </summary>
        public bool TryGetArchetype(out Archetype archetype, params Type[] types)
            => TryGetArchetype(out archetype, new TypeSignature(types));

        /// <summary>
        /// Tries to get an archetype that has the supplied types.
        /// Returns false if the world is destroyed or null
        /// </summary>
        public bool TryGetArchetype(out Archetype archetype, IEnumerable<Type> types)
            => TryGetArchetype(out archetype, new TypeSignature(types));

        /// <summary>
        /// WorldData is data unique to this world
        /// Set's the world's data to value.
        /// </summary>
        public World SetData<WorldData>(WorldData world_data)
        {
            if (this.TryGetWorldInfo(out var info))
            {
                var stored = info.GetData<WorldData>();
                stored.assigned_data = true;
                stored.data = world_data;
            }
            return this;
        }

        /// <summary>
        /// WorldData is data unique to this world
        /// Get's a reference to the data which can be assigned.
        /// Throws an exception if the world is destroyed or null
        /// </summary>
        public ref WorldData GetData<WorldData>()
        {
            if (this.TryGetWorldInfo(out var info))
            {
                var stored = info.GetData<WorldData>();
                stored.assigned_data = true;
                return ref info.GetData<WorldData>().data;
            }
            throw new Exception($"{this} is invalid, cannot get resource {typeof(WorldData).Name}");
        }

        /// <summary>
        /// Returns a copy of all the world data currently assigned in the world
        /// </summary>
        public object[] GetAllWorldData()
        {
            List<object> all = new List<object>();
            if (this.TryGetWorldInfo(out var info))
            {
                foreach (var stored in info.world_data)
                {
                    if (stored != null && stored.assigned_data)
                        all.Add(stored.GetData());
                }
            }
            return all.ToArray();
        }

        /// <summary>
        /// Retuns a copy of all the Types of world data currently assigned in the world
        /// </summary>
        public Type[] GetAllWorldDataTypes()
        {
            List<Type> all = new List<Type>();
            if (this.TryGetWorldInfo(out var info))
            {
                foreach (var stored in info.world_data)
                {
                    if (stored != null && stored.assigned_data)
                        all.Add(stored.data_type);
                }
            }
            return all.ToArray();
        }

        /// <summary>
        /// Adds a callback to be invoked whenever an entity sets a component of type
        /// </summary>
        /// <param name="callback">callback to invoke</param>
        /// <param name="register">set true to add callback, false to remove the callback</param>
        public World OnSet<Component>(SetComponentEvent<Component> callback, bool register = true)
        {
            if (this.TryGetWorldInfo(out var info))
            {
                var data = info.GetData<Component>();
                if (register)
                    data.set_callback += callback;
                else data.set_callback -= callback;
                data.has_set_callback = data.set_callback != null;
            }
            return this;
        }

        /// <summary>
        /// Adds a callback to be invoked whenever an entity sets a component of type
        /// </summary>
        /// <param name="callback">callback to invoke</param>
        /// <param name="register">set true to add callback, false to remove the callback</param>
        public World OnSet<Component>(SetComponentEventRefOnly<Component> callback, bool register = true)
        {
            if (this.TryGetWorldInfo(out var info))
            {
                var data = info.GetData<Component>();
                if (register)
                {
                    if (data.set_ref_callback == null)
                        data.set_callback += data.CallSetRefCallback;
                    data.set_ref_callback += callback;
                }
                else
                {
                    data.set_ref_callback -= callback;
                    if (data.set_ref_callback == null)
                        data.set_callback -= data.CallSetRefCallback;
                }

                data.has_set_callback = data.set_callback != null;
            }
            return this;
        }

        /// <summary>
        /// Adds a callback to be invoked whenever an entity sets a component of type
        /// </summary>
        /// <param name="callback">callback to invoke</param>
        /// <param name="register">set true to add callback, false to remove the callback</param>
        public World OnSet<Component>(SetComponentEventCompOnly<Component> callback, bool register = true)
        {
            if (this.TryGetWorldInfo(out var info))
            {
                var data = info.GetData<Component>();
                if (register)
                {
                    if (data.set_comp_callback == null)
                        data.set_callback += data.CallSetCompCallback;
                    data.set_comp_callback += callback;
                }
                else
                {
                    data.set_comp_callback -= callback;
                    if (data.set_comp_callback == null)
                        data.set_callback -= data.CallSetCompCallback;
                }

                data.has_set_callback = data.set_callback != null;
            }
            return this;
        }

        /// <summary>
        /// Adds a callback to be invoked whenever an entity removes a component of type
        /// </summary>
        /// <param name="callback">callback to invoke</param>
        /// <param name="register">set true to add callback, false to remove the callback</param>
        public World OnRemove<Component>(RemoveComponentEvent<Component> callback, bool register = true)
        {
            if (this.TryGetWorldInfo(out var world_info))
            {
                var data = world_info.GetData<Component>();
                if (register)
                    data.remove_callback += callback;
                else data.remove_callback -= callback;
                data.has_remove_callback = data.remove_callback != null;
            }
            return this;
        }

        /// <summary>
        /// Adds a callback to be invoked whenever an entity removes a component of type
        /// </summary>
        /// <param name="callback">callback to invoke</param>
        /// <param name="register">set true to add callback, false to remove the callback</param>
        public World OnRemove<Component>(RemoveComponentEventCompOnly<Component> callback, bool register = true)
        {
            if (this.TryGetWorldInfo(out var world_info))
            {
                var data = world_info.GetData<Component>();
                if (register)
                {
                    if (data.remove_comp_callback == null)
                        data.remove_callback += data.CallRemoveCompCallback;
                    data.remove_comp_callback += callback;
                }
                else
                {
                    data.remove_comp_callback -= callback;
                    if (data.remove_comp_callback == null)
                        data.remove_callback -= data.CallRemoveCompCallback;
                }
                data.has_remove_callback = data.remove_callback != null;
            }
            return this;
        }

        /// <summary>
        /// [structural]
        /// Resizes all archetype's backing arrays to the minimum power of 2 needed to store their entities
        /// </summary>
        public void ResizeBackingArrays()
        {
            foreach (var archetype in GetArchetypes())
                archetype.ResizeBackingArrays();
        }

        /// <summary>
        /// [structural]
        /// Destroys all archetypes with 0 entities
        /// </summary>
        public void DestroyEmptyArchetypes()
        {
            foreach (var archetype in GetArchetypes())
            {
                if (archetype.EntityCount == 0)
                    archetype.Destroy();
            }
        }

        /// <summary>
        /// When enabled all structural methods will be cached like they are when iterating a query.
        /// They will be applied once you disable caching.
        /// Use to prevent iterator invalidation when manually iterating over entity or component buffers.
        /// </summary>
        public void CacheStructuralEvents(bool enable)
        {
            if (this.TryGetWorldInfo(out var info))
                info.cache_structural_changes = enable;
        }

        /// <summary>
        /// Returns true if the world is currently caching structural changes
        /// </summary>
        public bool IsCachingStructuralEvents()
        {
            if (this.TryGetWorldInfo(out var info))
                return info.StructureEvents.EnqueueEvents > 0;
            return false;
        }


        public override string ToString()
         => $"{(IsValid() ? "" : "~")}{Name} {index}.{version}";

        bool IEquatable<World>.Equals(World other) => index == other.index && version == other.version;

        public override int GetHashCode() => index;

        public override bool Equals(object obj)
        {
            if (obj is World world)
                return world == this;
            return false;
        }

        public static bool operator ==(World a, World b) => a.index == b.index && a.version == b.version;
        public static bool operator !=(World a, World b) => !(a == b);

        public static implicit operator bool(World world) => WorldInfo.All[world.index].version == world.version;

        /// <summary>
        /// Returns a copy of all active Worlds
        /// </summary>
        public static World[] GetAll()
        {
            var worlds = new List<World>();
            foreach (var info in WorldInfo.All)
            {
                if (info.data != null)
                    worlds.Add(info.data.world);
            }
            return worlds.ToArray();
        }

        /// <summary>
        /// Tries to get the entity with index.
        /// returns true if entity is valid
        /// </summary>
        public static bool TryGetEntity(int index, out Entity entity)
        {
            if (index <= Entities.Last)
            {
                var data = Entities.All[index];
                if (data.arch_info != null)
                {
                    entity = new Entity(index, data.version);
                    return true;
                }
            }
            entity = default;
            return false;
        }

        IEnumerator<Archetype> IEnumerable<Archetype>.GetEnumerator()
        {
            if (this.TryGetWorldInfo(out var info))
            {
                for (int i = 0; i < info.archetype_terminating_index; ++i)
                {
                    var arche_info = info.archetypes[i].data;
                    if (arche_info != null)
                        yield return arche_info.archetype;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this.TryGetWorldInfo(out var info))
            {
                for (int i = 0; i < info.archetype_terminating_index; ++i)
                {
                    var arche_info = info.archetypes[i].data;
                    if (arche_info != null)
                        yield return arche_info.archetype;
                }
            }
        }
    }
}