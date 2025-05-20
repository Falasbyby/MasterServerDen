using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

public class RoomServer : MonoBehaviour
{
    public NetworkManager networkManager;
    private string serverIp;
    private int serverPort;

    private void Start()
    {
        // Получаем аргументы командной строки
        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length >= 4)
        {
            serverIp = args[2];
            if (int.TryParse(args[3], out int port))
            {
                serverPort = port;
            }
        }
        else
        {
            Debug.LogError("[RoomServer] Не получены IP и порт из аргументов командной строки!");
            return;
        }

        Debug.Log($"[RoomServer] Запускаем сервер на {serverIp}:{serverPort}");
        
        // Настраиваем транспорт
        var transport = networkManager.TransportManager.Transport;
        transport.SetServerBindAddress(serverIp, IPAddressType.IPv4);
        transport.SetPort((ushort)serverPort);
        
        networkManager.ServerManager.StartConnection();
    }
}
