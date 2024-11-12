using SimpleECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void RemoveComponentEvent<T>(Entity entity, T component);
