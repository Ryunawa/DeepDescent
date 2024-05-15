using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public static class DynamicNavMesh
{
    static NavMeshSurface navMeshSurface;

    public static void UpdateNavMesh()
    {
        Debug.LogWarning("update?");
        if (navMeshSurface == null)
        {
            Debug.LogWarning("find?");
            navMeshSurface = UnityEngine.Object.FindObjectOfType<NavMeshSurface>();
            if(navMeshSurface == null ) 
            {
                Debug.LogError("No NavMeshSurface found in scene.");
                return;
            }
            else
            {

                Debug.LogWarning("found.");
            }
        }
        navMeshSurface.BuildNavMesh();
        Debug.LogWarning("BuildNavMesh.");
    }
}
