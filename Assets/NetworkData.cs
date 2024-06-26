using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkData : NetworkBehaviour
{
    public NetworkVariable<bool> isHostRdy = new ();
    public NetworkVariable<bool> isClientOneRdy = new ();
    public NetworkVariable<bool> isClientTwoRdy = new ();
    public NetworkVariable<bool> isClientThreeRdy = new ();
    
    
    public bool ArePlayersRdy()
    {
        switch (NetworkManager.Singleton.ConnectedClients.Count)
        {
            case 1:
                return isHostRdy.Value;
            case 2:
                return isHostRdy.Value && isClientOneRdy.Value;
            case 3:
                return isHostRdy.Value && isClientOneRdy.Value && isClientTwoRdy.Value;
            case 4:
                return isHostRdy.Value && isClientOneRdy.Value && isClientTwoRdy.Value && isClientThreeRdy.Value;
        }

        return false;
    }
}
