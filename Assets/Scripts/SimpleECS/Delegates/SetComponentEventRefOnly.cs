using SimpleECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void SetComponentEventRefOnly<T>(Entity entity, ref T new_comp);
