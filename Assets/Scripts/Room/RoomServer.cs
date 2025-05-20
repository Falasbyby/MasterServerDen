using FishNet.Managing;
using UnityEngine;

public class RoomServer : MonoBehaviour
{
    public NetworkManager networkManager;

    private void Start()
    {
        Debug.Log("[RoomServer] Запускаем сервер...");
        networkManager.ServerManager.StartConnection();
    }
}
