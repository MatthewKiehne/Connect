using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CompBuffer    //handles component data
{
    public IList array;
    public abstract void Resize(int capacity);
    /// <summary>
    /// returns removed component
    /// </summary>
    public abstract object Remove(int entity_arch_index, int last);
    public abstract void Move(int entity_arch_index, int last_entity_index, ArchetypeInfo target_archetype, int target_index);
    public abstract void Move(int entity_arch_index, int last_entity_index, object buffer, int target_index);
}
