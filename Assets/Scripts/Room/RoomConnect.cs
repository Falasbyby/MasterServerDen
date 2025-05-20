using System;
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;
using static GameClient;

public class RoomConnector : MonoBehaviour
{
    public NetworkManager networkManager;
    public GameClient gameClient;

    public void ConnectToRoom(Room room)
    {
        if (networkManager == null)
        {
            Debug.LogError("[RoomConnector] NetworkManager не назначен!");
            return;
        }

        if (string.IsNullOrEmpty(room.address))
        {
            Debug.LogError("[RoomConnector] Адрес комнаты пустой!");
            return;
        }

        if (room.port <= 0)
        {
            Debug.LogError($"[RoomConnector] Неверный порт комнаты: {room.port}");
            return;
        }

        var transport = networkManager.TransportManager.Transport;
        transport.SetClientAddress(room.address);
        transport.SetPort((ushort)room.port);
        
        Debug.Log($"[RoomConnector] Подключаемся к комнате {room.name} на {room.address}:{room.port}");
        
        // Подписываемся на события подключения
        networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
        
        // Используем правильный метод подключения с параметрами
        networkManager.ClientManager.StartConnection();
    }
    private void OnDestroy()
    {
        if (networkManager != null)
        {
            networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        }
    }
    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        Debug.Log($"[RoomConnector] Состояние подключения: {args.ConnectionState}");
    }
}
