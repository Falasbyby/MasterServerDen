using FishNet.Managing;
using UnityEngine;
using static GameClient;

public class RoomConnector : MonoBehaviour
{
    public NetworkManager networkManager;
    public GameClient gameClient;

    public void ConnectToRoom(Room room)
    {
        var transport = networkManager.TransportManager.Transport;
        transport.SetClientAddress(room.address);
        transport.SetPort((ushort)room.port);
        Debug.LogWarning($"[RoomConnector] Подключаемся к {room.address}:{room.port}");
        networkManager.ClientManager.StartConnection();
    }
}
