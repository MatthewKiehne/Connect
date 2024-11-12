using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CompBufferImp<Component> : CompBuffer
{
    public Component[] components = new Component[8];

    public CompBufferImp()
    {
        array = components;
    }

    public override void Resize(int capacity)
    {
        System.Array.Resize(ref components, capacity);
        array = components;
    }

    public override object Remove(int entity_arch_index, int last)
    {
        var comp = components[entity_arch_index];
        components[entity_arch_index] = components[last];
        components[last] = default;
        return comp;
    }

    public override void Move(int entity_arch_index, int last_entity_index, ArchetypeInfo target_archetype, int target_index)
    {
        if (target_archetype.TryGetArray<Component>(out var target_array))
        {
            target_array[target_index] = components[entity_arch_index];
        }
        components[entity_arch_index] = components[last_entity_index];
        components[last_entity_index] = default;
    }

    public override void Move(int entity_arch_index, int last_entity_index, object buffer, int target_index)
    {
        ((Component[])buffer)[target_index] = components[entity_arch_index];
        components[entity_arch_index] = components[last_entity_index];
        components[last_entity_index] = default;
    }
}
