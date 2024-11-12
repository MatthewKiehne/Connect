using SimpleECS.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// assigns ids to types
/// </summary>
public static class TypeID    // class to map types to ids
{

    static Dictionary<Type, int> newIDs = new Dictionary<Type, int>();
    static Type[] id_to_type = new Type[64];
    public static Type Get(int type_id) => id_to_type[type_id];

    public static int Get(Type type)
    {
        if (!newIDs.TryGetValue(type, out var id))
        {
            newIDs[type] = id = newIDs.Count + 1;
            if (id == id_to_type.Length)
            {
                Array.Resize(ref id_to_type, id_to_type.Length * 2);
            }

            id_to_type[id] = type;
        }
        return id;
    }

    public static List<Type> GetAssignedTypes()
    {
        var list = new List<Type>();
        foreach (var type in newIDs)
        {
            list.Add(type.Key);
        }
        return list;
    }
}