using SimpleECS.Internal;
using SimpleECS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchetypeInfo
{

    public int entity_count;
    public Entity[] entities = new Entity[8];
    public WorldInfo world_info;
    public TypeSignature signature;
    public readonly Archetype archetype;
    public readonly int component_count;
    public CompBufferData[] component_buffers;

    public ArchetypeInfo(WorldInfo world, TypeSignature signature, int arch_index, int arch_version)
    {
        this.world_info = world;
        this.signature = signature;
        this.archetype = new Archetype(world.world, arch_index, arch_version);

        component_buffers = new CompBufferData[signature.Count == 0 ? 1 : signature.Count];
        component_count = signature.Count;

        for (int i = 0; i < component_buffers.Length; ++i)
            component_buffers[i].next = -1;

        // add components into empty bucket, skip if bucket is occupied
        for (int i = 0; i < component_count; ++i)
        {
            var type = signature.Types[i];
            var type_id = TypeID.Get(type);
            var index = type_id % component_buffers.Length;
            ref var buffer_data = ref component_buffers[index];
            if (buffer_data.type_id == 0)
            {
                buffer_data.type_id = type_id;
                buffer_data.buffer = CreatePool(type);
            }
        }

        // add skipped components into buckets not filled in first pass
        // hopefully this minimizes lookup time
        for (int i = 0; i < component_count; ++i)
        {
            var type = signature.Types[i];
            var type_id = TypeID.Get(type);
            if (ContainsType(type_id)) continue;
            var index = GetEmptyIndex(type_id % component_buffers.Length);
            ref var buffer_data = ref component_buffers[index];
            buffer_data.type_id = type_id;
            buffer_data.buffer = CreatePool(type);
        }

        bool ContainsType(int type_id)
        {
            foreach (var val in component_buffers)
                if (val.type_id == type_id) return true;
            return false;
        }

        // if current index is filled, will return an empty index with a way to get to that index from the provided one
        int GetEmptyIndex(int current_index)
        {
            if (component_buffers[current_index].type_id == 0)
                return current_index;

            while (component_buffers[current_index].next >= 0)
            {
                current_index = component_buffers[current_index].next;
            }

            for (int i = 0; i < component_count; ++i)
                if (component_buffers[i].type_id == 0)
                {
                    component_buffers[current_index].next = i;
                    return i;
                }
            throw new Exception("FRAMEWORK BUG: not enough components in archetype");
        }

        CompBuffer CreatePool(Type type)
            => Activator.CreateInstance(typeof(CompBufferImp<>).MakeGenericType(type)) as CompBuffer;
    }


    /// <summary>
    /// resizes all backing arrays to minimum power of 2
    /// </summary>
    public void ResizeBackingArrays()
    {
        int size = 8;
        while (size <= entity_count)
            size *= 2;
        System.Array.Resize(ref entities, size);
        for (int i = 0; i < component_count; ++i)
            component_buffers[i].buffer.Resize(size);
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity >= entities.Length)
        {
            int size = entities.Length;
            while (capacity >= size)
                size *= 2;
            System.Array.Resize(ref entities, size);
            for (int i = 0; i < component_count; ++i)
                component_buffers[i].buffer.Resize(size);
        }
    }

    public bool Has(int type_id)
    {
        var data = component_buffers[type_id % component_buffers.Length];
        if (data.type_id == type_id)
            return true;

        while (data.next >= 0)
        {
            data = component_buffers[data.next];
            if (data.type_id == type_id)
                return true;
        }
        return false;
    }

    public bool TryGetArray<Component>(out Component[] components)
    {
        int type_id = TypeID.Get(typeof(Component));
        var data = component_buffers[type_id % component_buffers.Length];
        if (data.type_id == type_id)
        {
            components = (Component[])data.buffer.array;
            return true;
        }
        while (data.next >= 0)
        {
            data = component_buffers[data.next];
            if (data.type_id == type_id)
            {
                components = (Component[])data.buffer.array;
                return true;
            }
        }
        components = default;
        return false;
    }

    public bool TryGetCompBuffer(int type_id, out CompBuffer buffer)
    {
        var data = component_buffers[type_id % component_buffers.Length];
        if (data.type_id == type_id)
        {
            buffer = data.buffer;
            return true;
        }
        while (data.next >= 0)
        {
            data = component_buffers[data.next];
            if (data.type_id == type_id)
            {
                buffer = data.buffer;
                return true;
            }
        }
        buffer = default;
        return false;
    }

    public object[] GetAllComponents(int entity_arch_index)
    {
        object[] components = new object[component_count];

        for (int i = 0; i < component_count; ++i)
            components[i] = component_buffers[i].buffer.array[entity_arch_index];
        return components;
    }

    public Type[] GetComponentTypes()
    {
        Type[] components = new Type[component_count];
        for (int i = 0; i < component_count; ++i)
            components[i] = TypeID.Get(component_buffers[i].type_id);
        return components;
    }
}
