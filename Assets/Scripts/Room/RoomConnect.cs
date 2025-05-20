using FishNet.Transporting;
using FishNet.Managing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomConnector : MonoBehaviour
{
    public NetworkManager networkManager;
    public GameClient gameClient;

    public void ConnectToRoom(int roomId)
    {
        gameClient.JoinRoom(roomId, 
            onSuccess: (room) => {
                var transport = networkManager.TransportManager.Transport;
                transport.SetClientAddress(room.address);
                transport.SetPort((ushort)room.port);

                Debug.Log($"[RoomConnector] Подключаемся к {room.address}:{room.port}");
                networkManager.ClientManager.StartConnection();
            },
            onError: (error) => {
                Debug.LogError($"[RoomConnector] Ошибка при подключении к комнате: {error}");
            }
        );
    }
}
