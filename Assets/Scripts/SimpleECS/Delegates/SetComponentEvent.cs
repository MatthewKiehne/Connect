using SimpleECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void SetComponentEvent<T>(Entity entity, T old_comp, ref T new_comp);
