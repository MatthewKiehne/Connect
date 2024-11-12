using SimpleECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        World world = World.Create();
        world.CreateEntity("string");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
