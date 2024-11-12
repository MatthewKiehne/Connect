using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEvent
{
    public struct EventData
    {
        public object Source { get; set; }
        public object Data { get; set; }
        public string Event { get; set; }
        public int Index { get; set; }
    }
}
