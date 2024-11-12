using SimpleECS.Internal;
using SimpleECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WorldInfo
{
    public string name;
    public World world;

    public int archetype_count => archetype_terminating_index - free_archetypes.Count;
    public int entity_count;
    public int archetype_terminating_index;
    public int archetype_structure_update_count;

    public Stack<int> free_archetypes = new Stack<int>();
    public WorldData[] world_data = new WorldData[32];

    public List<int> assigned_world_data = new List<int>();

    public (ArchetypeInfo data, int version)[] archetypes = new (ArchetypeInfo, int)[32];
    public Dictionary<TypeSignature, int> signature_to_archetype_index = new Dictionary<TypeSignature, int>();

    public TypeSignature buffer_signature = new TypeSignature(); // just a scratch signature so that I'm not making new ones all the time

    public static (WorldInfo data, int version)[] All;
    public static int world_count;

    /// <summary>
    /// Handles all structural changes to the ecs world
    /// </summary>
    public StructureEventHandler StructureEvents;

    static WorldInfo()
    {
        All = new (WorldInfo, int)[4];
        All[0].version = 1; // added a version to the 0th index so that a default world will be invalid
    }

   
    public WorldInfo(string name, World world)
    {
        this.name = name;
        this.world = world;
        archetypes[0].version++;    // this is just to prevent default archetype and default entity from being valid
        StructureEvents = new StructureEventHandler(this);
    }

    bool _cache_structural_changes;
    public bool cache_structural_changes
    {
        get => _cache_structural_changes;
        set
        {
            StructureEvents.EnqueueEvents += value ? 1 : -1;
            _cache_structural_changes = value;
        }
    }

    public ArchetypeInfo GetArchetypeData(TypeSignature signature)
    {
        if (!signature_to_archetype_index.TryGetValue(signature, out var index))
        {
            if (free_archetypes.Count > 0)
            {
                index = free_archetypes.Pop();
                archetype_structure_update_count++;
            }
            else
            {
                if (archetype_terminating_index == archetypes.Length)
                    System.Array.Resize(ref archetypes, archetype_terminating_index * 2);
                index = archetype_terminating_index;
                archetype_terminating_index++;
            }
            var sig = new TypeSignature(signature);
            signature_to_archetype_index[sig] = index;
            archetypes[index].data = new ArchetypeInfo(this, sig, index, archetypes[index].version);
        }
        return archetypes[index].data;
    }

    public WorldDataImp<T> GetData<T>()
    {
        int type_id = TypeID.Get(typeof(T));
        if (type_id >= world_data.Length)
        {
            var size = world_data.Length;
            while (size <= type_id)
                size *= 2;
            System.Array.Resize(ref world_data, size);
        }
        if (world_data[type_id] == null)
            world_data[type_id] = new WorldDataImp<T>();
        return (WorldDataImp<T>)world_data[type_id];
    }

    public WorldData GetData(int type_id)
    {
        if (type_id >= world_data.Length)
        {
            var size = world_data.Length;
            while (size <= type_id)
                size *= 2;
            System.Array.Resize(ref world_data, size);
        }
        if (world_data[type_id] == null)
        {
            var type = TypeID.Get(type_id);
            world_data[type_id] = (WorldData)System.Activator.CreateInstance(typeof(WorldDataImp<>).MakeGenericType(type));
        }
        return world_data[type_id];
    }
}
