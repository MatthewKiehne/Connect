using SimpleECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class Extensions
{
    public static bool TryGetWorldInfo(this World world, out WorldInfo info)
    {
        var data = WorldInfo.All[world.index];
        if (data.version == world.version)
        {
            info = data.data;
            return true;
        }
        info = default;
        return false;
    }
}
