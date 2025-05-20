using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Managing.Scened;
using FishNet.Connection;
using FishNet.Managing.Server;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class RoomServer : MonoBehaviour
{
    public NetworkManager networkManager;
    private string serverIp;
    private int serverPort;

    private bool IsPortAvailable(int port)
    {
        try
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                return true;
            }
        }
        catch (SocketException ex)
        {
            Debug.LogWarning($"[RoomServer] Порт {port} недоступен: {ex.Message}");
            return false;
        }
    }

    private void Start()
    {
        // Получаем аргументы командной строки
        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length >= 4)
        {
            serverIp = args[3];
            if (int.TryParse(args[4], out int port))
            {
                serverPort = port;
            }
        }
        else
        {
            Debug.LogError("[RoomServer] Не получены IP и порт из аргументов командной строки!");
            return;
        }

        Debug.Log($"[RoomServer] Проверяем доступность порта {serverPort}...");
        if (!IsPortAvailable(serverPort))
        {
            Debug.LogError($"[RoomServer] Порт {serverPort} занят! Сервер не может быть запущен.");
            return;
        }

        Debug.Log($"=== [RoomServer] Запускаем сервер на {serverIp}:{serverPort} ===");
       
        // Настраиваем транспорт
        var transport = networkManager.TransportManager.Transport;
        transport.SetServerBindAddress(serverIp, IPAddressType.IPv4);
        transport.SetPort((ushort)serverPort);
        
        // Проверяем настройки транспорта
        var actualPort = transport.GetPort();
        var actualAddress = transport.GetServerBindAddress(IPAddressType.IPv4);
        Debug.Log($"[RoomServer] Настройки транспорта: адрес={actualAddress}, порт={actualPort}");
        
        if (actualPort != serverPort)
        {
            Debug.LogWarning($"[RoomServer] Внимание: запрошенный порт {serverPort} отличается от установленного {actualPort}");
        }
        
        networkManager.ServerManager.StartConnection();
        networkManager.ServerManager.OnClientKick += OnClientKicked;
        
        // Подписываемся на события загрузки сцены
        networkManager.SceneManager.OnLoadEnd += OnSceneLoadEnd;
        networkManager.SceneManager.OnLoadStart += OnSceneLoadStart;
        
        // Создаем SceneLoadData для загрузки сцены
        var sceneLoadData = new SceneLoadData("Room");
        networkManager.SceneManager.LoadConnectionScenes(sceneLoadData);
    }

    private void OnDestroy()
    {
        if (networkManager != null)
        {
            networkManager.ServerManager.OnClientKick -= OnClientKicked;
            networkManager.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
            networkManager.SceneManager.OnLoadStart -= OnSceneLoadStart;
        }
    }

    private void OnSceneLoadStart(SceneLoadStartEventArgs args)
    {
        Debug.Log($"[RoomServer] Началась загрузка сцены Room");
    }

    private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
    {
        Debug.Log($"[RoomServer] Сцена Room успешно загружена");
    }

    private void OnClientKicked(NetworkConnection connection, int reason, KickReason kickReason)
    {
        Debug.Log($"[RoomServer] Клиент {connection.ClientId} был кикнут из комнаты. Причина: {kickReason}");
    }
}
