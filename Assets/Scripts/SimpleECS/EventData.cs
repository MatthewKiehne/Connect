using SimpleECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleECS
{
    struct EventData
    {
        public EventType type;
        public Entity entity;
        public int type_id;
        public Archetype archetype;
        public World world;
    }
}

