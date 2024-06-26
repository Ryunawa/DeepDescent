using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkData : NetworkBehaviour
{
    public NetworkVariable<bool> isHostRdy = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isClientOneRdy = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isClientTwoRdy = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isClientThreeRdy = new NetworkVariable<bool>(false);
    
    
    public bool ArePlayersRdy()
    {
        switch (NetworkManager.Singleton.ConnectedClients.Count + NetworkManager.Singleton.PendingClients.Count)
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

    [Rpc(SendTo.Server)]
    public void UpdateRdyServerRpc(int index)
    {
        switch (index)
        {
            case 0:
                isHostRdy.Value = true;
                break;
            case 1:
                isClientOneRdy.Value = true;
                break;
            case 2:
                isClientTwoRdy.Value = true;
                break;
            case 3:
                isClientThreeRdy.Value = true;
                break;
        }
    }
}
