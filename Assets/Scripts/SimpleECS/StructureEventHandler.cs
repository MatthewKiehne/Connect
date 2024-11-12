using SimpleECS.Internal;
using SimpleECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct StructureEventHandler
{

    Queue<EventData> events;
    WorldInfo world;
    int cache_events;
    public int EnqueueEvents
    {
        get => cache_events;
        set
        {
            cache_events = value;
            ExecuteEventPlayback();
        }
    }

    public StructureEventHandler(WorldInfo world)
    {
        cache_events = 0;
        events = new Queue<EventData>();
        this.world = world;
    }

    public void ExecuteEventPlayback()
    {
        while (cache_events == 0 && events.Count > 0)
        {
            var e = events.Dequeue();
            switch (e.type)
            {
                case EventType.CreateEntity:
                    {
                        ref var arch_data = ref world.archetypes[e.archetype.index];
                        if (arch_data.version == e.archetype.version)
                            SetUpEntity(e.entity, world.archetypes[e.archetype.index].data);
                        else
                        {
                            Entities.All[e.entity.index].world_info = default;
                            Entities.Free.Enqueue(e.entity.index);
                        }
                    }
                    break;

                case EventType.SetComponent:
                    world.GetData(e.type_id).Set(e.entity, this);
                    break;

                case EventType.RemoveComponent:
                    world.GetData(e.type_id).Remove(e.entity, this);
                    break;

                case EventType.DestroyEntity:
                    Destroy(e.entity);
                    break;

                case EventType.TransferEntity:
                    Transfer(e.entity, e.world);
                    break;

                case EventType.DestroyArchetype:
                    DestroyArchetype(e.archetype);
                    break;

                case EventType.DestroyWorld:
                    DestroyWorld();
                    break;

                case EventType.ResizeBackingArrays:
                    ResizeBackingArrays(e.archetype);
                    break;
            }
        }
    }

    public Entity CreateEntity(ArchetypeInfo archetype_data)
    {
        int index = 0;
        if (Entities.Free.Count > 0)
            index = Entities.Free.Dequeue();
        else
        {
            index = Entities.Last;
            if (index == Entities.All.Length)
                System.Array.Resize(ref Entities.All, index * 2);
            Entities.Last++;
        }
        var version = Entities.All[index].version;
        var entity = new Entity(index, version);
        Entities.All[index].world_info = world;

        if (cache_events > 0)
        {
            Entities.All[index].version++;
            events.Enqueue(new EventData { type = EventType.CreateEntity, entity = entity, archetype = archetype_data.archetype });
        }
        else SetUpEntity(entity, archetype_data);
        return entity;
    }

    void SetUpEntity(Entity entity, ArchetypeInfo archetype_data)
    {
        ref var entity_data = ref Entities.All[entity.index];
        entity_data.version = entity.version;
        entity_data.arch_info = archetype_data;
        var arch_index = entity_data.arch_index = archetype_data.entity_count;
        archetype_data.entity_count++;
        archetype_data.world_info.entity_count++;
        archetype_data.EnsureCapacity(arch_index);
        archetype_data.entities[arch_index] = entity;
    }

    public void Set<Component>(in Entity entity, in Component component)
    {
        var world_data = world.GetData<Component>();
        if (cache_events > 0)
        {
            world_data.set_queue.Enqueue(component);
            events.Enqueue(new EventData { type = EventType.SetComponent, entity = entity, type_id = TypeID.Get(typeof(Component)) });
            return;
        }

        ref var entity_info = ref Entities.All[entity.index];
        if (entity_info.version == entity.version)
        {
            if (entity_info.arch_info.TryGetArray<Component>(out var buffer))
            {
                int index = entity_info.arch_index;
                Component old = buffer[index];
                buffer[index] = component;
                world_data.set_callback?.Invoke(entity, old, ref buffer[index]);
            }
            else
            {
                var old_index = entity_info.arch_index;
                var archetype = entity_info.arch_info;
                var last_index = --archetype.entity_count;
                var last = archetype.entities[old_index] = archetype.entities[last_index];
                Entities.All[last.index].arch_index = old_index; // reassign moved entity to to index

                // adding entity to target archetype
                var target_archetype = entity_info.arch_info = world.GetArchetypeData(world.buffer_signature.Copy(archetype.signature).Add<Component>());
                var target_index = entity_info.arch_index = target_archetype.entity_count;
                target_archetype.EnsureCapacity(target_index);
                target_archetype.entity_count++;

                // moving components over
                target_archetype.entities[target_index] = entity;
                for (int i = 0; i < archetype.component_count; ++i)
                    archetype.component_buffers[i].buffer.Move(old_index, last_index, target_archetype, target_index);

                // setting the added component and calling the callback event
                if (target_archetype.TryGetArray<Component>(out var target_buffer))
                {
                    target_buffer[target_index] = component;
                    world_data.set_callback?.Invoke(entity, default, ref target_buffer[target_index]);
                }
                else
                    throw new System.Exception("Frame Work Bug");
            }
        }
    }

    public void Remove<Component>(in Entity entity)
    {
        int type_id = TypeID.Get(typeof(Component));
        if (cache_events > 0)
        {
            events.Enqueue(new EventData { type = EventType.RemoveComponent, entity = entity, type_id = type_id });
        }
        else
        {
            ref var entity_info = ref Entities.All[entity.index];
            if (entity.version == entity_info.version)
            {
                var old_arch = entity_info.arch_info;
                if (old_arch.TryGetArray<Component>(out var old_buffer))  // if archetype already has component, just set and fire event
                {
                    var old_index = entity_info.arch_index;

                    var target_arch = world.GetArchetypeData(world.buffer_signature.Copy(old_arch.signature).Remove(type_id));
                    var target_index = target_arch.entity_count;
                    target_arch.entity_count++;
                    target_arch.EnsureCapacity(target_index);

                    old_arch.entity_count--;
                    var last_index = old_arch.entity_count;
                    var last = old_arch.entities[old_index] = old_arch.entities[last_index];
                    Entities.All[last.index].arch_index = old_index;

                    entity_info.arch_index = target_index;
                    entity_info.arch_info = target_arch;

                    target_arch.entities[target_index] = entity;
                    var removed = old_buffer[old_index];
                    for (int i = 0; i < old_arch.component_count; ++i)
                        old_arch.component_buffers[i].buffer.Move(old_index, last_index, target_arch, target_index);
                    world.GetData<Component>().remove_callback?.Invoke(entity, removed);
                }
            }
        }
    }

    public void Transfer(Entity entity, World target_world)
    {
        if (cache_events > 0)
            events.Enqueue(new EventData { type = EventType.TransferEntity, entity = entity, world = target_world });
        else
        {
            ref var entity_info = ref Entities.All[entity.index];
            if (entity_info.version == entity.version
                && entity_info.arch_info.world_info.world != target_world
                && target_world.TryGetWorldInfo(out var target_world_info))
            {
                var target_arch = target_world_info.GetArchetypeData(entity_info.arch_info.signature);
                var target_index = target_arch.entity_count;
                target_arch.EnsureCapacity(target_index);
                target_arch.entity_count++;
                target_arch.world_info.entity_count++;

                var old_index = entity_info.arch_index;
                var old_arch = entity_info.arch_info;
                var last_index = --old_arch.entity_count;
                --old_arch.world_info.entity_count;

                var last = old_arch.entities[old_index] = old_arch.entities[last_index];
                Entities.All[last.index].arch_index = old_index;
                target_arch.entities[target_index] = entity;

                for (int i = 0; i < old_arch.component_count; ++i)
                    old_arch.component_buffers[i].buffer.Move(old_index, last_index, target_arch.component_buffers[i].buffer.array, target_index);

                entity_info.arch_index = target_index;
                entity_info.arch_info = target_arch;
                entity_info.world_info = target_world_info;
            }
        }
    }

    public void Destroy(Entity entity)
    {
        if (cache_events > 0)
            events.Enqueue(new EventData { type = EventType.DestroyEntity, entity = entity });
        else
        {
            ref var entity_info = ref Entities.All[entity.index];
            if (entity_info.version == entity.version)
            {
                entity_info.version++;
                var old_arch = entity_info.arch_info;
                var old_index = entity_info.arch_index;
                --old_arch.entity_count;
                --world.entity_count;
                var last_index = old_arch.entity_count;
                var last = old_arch.entities[old_index] = old_arch.entities[last_index];    // swap 
                Entities.All[last.index].arch_index = old_index;

                (WorldData callback, object value)[] removed =          // this causes allocations
                    new (WorldData, object)[old_arch.component_count];  // but other means are quite convuluted
                int length = 0;

                for (int i = 0; i < old_arch.component_count; ++i)
                {
                    var pool = old_arch.component_buffers[i];
                    var callback = world.GetData(pool.type_id);
                    if (callback.has_remove_callback)
                    {
                        removed[length] = (callback, pool.buffer.array[entity_info.arch_index]); // this causes boxing :(
                        length++;
                    }
                    pool.buffer.Remove(old_index, last_index);
                }
                entity_info.version++;
                entity_info.arch_info = default;
                entity_info.world_info = default;
                Entities.Free.Enqueue(entity.index);

                for (int i = 0; i < length; ++i)
                    removed[i].callback.InvokeRemoveCallback(entity, removed[i].value);
            }
        }
    }

    public void DestroyArchetype(Archetype archetype)
    {
        if (cache_events > 0)
        {
            events.Enqueue(new EventData { type = EventType.DestroyArchetype, archetype = archetype });
        }
        else
        {
            if (archetype.TryGetArchetypeInfo(out var arch_info))
            {
                world.entity_count -= arch_info.entity_count;
                world.signature_to_archetype_index.Remove(arch_info.signature);   // update archetype references
                world.archetypes[archetype.index].version++;
                world.archetypes[archetype.index].data = default;
                world.free_archetypes.Push(archetype.index);
                world.archetype_structure_update_count++;

                for (int i = 0; i < arch_info.entity_count; ++i)    // remove entities from world
                {
                    var entity = arch_info.entities[i];
                    ref var info = ref Entities.All[entity.index];
                    info.version++;
                    info.arch_info = default;
                    info.world_info = default;
                    Entities.Free.Enqueue(entity.index);
                }

                for (int i = 0; i < arch_info.component_count; ++i) // invoke callbacks
                {
                    var pool = arch_info.component_buffers[i];
                    var callback = world.GetData(pool.type_id);
                    if (callback.has_remove_callback)
                    {
                        callback.InvokeRemoveCallbackAll(arch_info.entities, pool.buffer.array, arch_info.entity_count);
                    }
                }
            }
        }
    }

    public void DestroyWorld()
    {
        if (cache_events > 0)
            events.Enqueue(new EventData { type = EventType.DestroyWorld });
        else
        {
            ref var world_info = ref WorldInfo.All[world.world.index];
            if (world_info.version == world.world.version)  // still needs to be checked incase multiple destorys are queued
            {
                world_info.version++;
                var data = world_info.data;
                world_info.data = default;

                foreach (var archetype in data.archetypes)   // delete all entities first
                {
                    var arche_info = archetype.data;
                    if (arche_info == null) continue;

                    for (int i = 0; i < arche_info.entity_count; ++i)
                    {
                        var index = arche_info.entities[i].index;
                        ref var info = ref Entities.All[index];
                        info.version++;
                        info.arch_info = default;
                        info.world_info = default;
                        Entities.Free.Enqueue(index);
                    }
                }

                foreach (var archetype in data.archetypes) // then do all their callbacks
                {
                    var arche_info = archetype.data;
                    if (arche_info == null) continue;
                    for (int i = 0; i < arche_info.component_count; ++i)
                    {
                        var pool = arche_info.component_buffers[i];
                        var world_data = data.GetData(pool.type_id);
                        if (world_data.has_remove_callback)
                        {
                            world_data.InvokeRemoveCallbackAll(arche_info.entities, pool.buffer.array, arche_info.entity_count);
                        }
                    }
                }
            }
        }
    }

    public void ResizeBackingArrays(Archetype archetype)
    {
        if (cache_events > 0)
            events.Enqueue(new EventData { type = EventType.ResizeBackingArrays, archetype = archetype });
        else
            if (archetype.TryGetArchetypeInfo(out var info))
            info.ResizeBackingArrays();
    }
}

