using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Entities
{
    public static EntityInfo[] All;
    public static Queue<int> Free;
    public static int Last;

    static Entities()
    {
        All = new EntityInfo[1024];
        Free = new Queue<int>(1024);
        All[0].version++;
    }
}
