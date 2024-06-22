using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine;

public static class DynamicNavMesh
{
    static NavMeshSurface navMeshSurface;

    public static void UpdateNavMesh()
    {
        if (navMeshSurface == null)
        {
            navMeshSurface = UnityEngine.Object.FindObjectOfType<NavMeshSurface>();
            if(navMeshSurface == null ) 
            {
                Debug.LogError("No NavMeshSurface found in scene.");
                return;
            }
        }

        navMeshSurface.BuildNavMesh();

    }
}
