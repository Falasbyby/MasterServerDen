using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameClient : MonoBehaviour
{
    private const string ServerUrl = "https://teamfluffygames.ru";
    
    [System.Serializable]
    public class Room
    {
        public int id;
        public string name;
        public int players;
    }

    [System.Serializable]
    public class RoomList
    {
        public List<Room> rooms;
    }

    [System.Serializable]
    public class CreateRoomRequest
    {
        public string Name;
    }

    public void GetRooms(System.Action<List<Room>> onSuccess, System.Action<string> onError)
    {
        Debug.Log("[GameClient] Запрашиваем список комнат...");
        StartCoroutine(GetRoomsCoroutine(onSuccess, onError));
    }

    private IEnumerator GetRoomsCoroutine(System.Action<List<Room>> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{ServerUrl}/rooms"))
        {
            Debug.Log("[GameClient] Отправляем GET запрос на получение списка комнат");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log($"[GameClient] Получен ответ от сервера: {json}");
                
                // Оборачиваем массив в объект для JsonUtility
                string wrappedJson = "{\"rooms\":" + json + "}";
                var roomList = JsonUtility.FromJson<RoomList>(wrappedJson);
                
                Debug.Log($"[GameClient] Найдено комнат: {roomList.rooms.Count}");
                foreach (var room in roomList.rooms)
                {
                    Debug.Log($"[GameClient] Комната: ID={room.id}, Название='{room.name}', Игроков={room.players}");
                }
                onSuccess?.Invoke(roomList.rooms);
            }
            else
            {
                Debug.LogError($"[GameClient] Ошибка при получении списка комнат: {request.error}");
                onError?.Invoke(request.error);
            }
        }
    }

    public void CreateRoom(string roomName, System.Action<Room> onSuccess, System.Action<string> onError)
    {
        Debug.Log($"[GameClient] Создаем новую комнату с названием: {roomName}");
        StartCoroutine(CreateRoomCoroutine(roomName, onSuccess, onError));
    }

    private IEnumerator CreateRoomCoroutine(string roomName, System.Action<Room> onSuccess, System.Action<string> onError)
    {
        var createRequest = new CreateRoomRequest { Name = roomName };
        string json = JsonUtility.ToJson(createRequest);
        Debug.Log($"[GameClient] Отправляем POST запрос на создание комнаты: {json}");

        using (UnityWebRequest request = new UnityWebRequest($"{ServerUrl}/rooms", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log($"[GameClient] Получен ответ от сервера: {responseJson}");
                var room = JsonUtility.FromJson<Room>(responseJson);
                Debug.Log($"[GameClient] Комната успешно создана: ID={room.id}, Название='{room.name}', Игроков={room.players}");
                onSuccess?.Invoke(room);
            }
            else
            {
                Debug.LogError($"[GameClient] Ошибка при создании комнаты: {request.error}");
                onError?.Invoke(request.error);
            }
        }
    }

    public void JoinRoom(int roomId, System.Action<Room> onSuccess, System.Action<string> onError)
    {
        Debug.Log($"[GameClient] Пытаемся присоединиться к комнате с ID: {roomId}");
        StartCoroutine(JoinRoomCoroutine(roomId, onSuccess, onError));
    }

    private IEnumerator JoinRoomCoroutine(int roomId, System.Action<Room> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest request = new UnityWebRequest($"{ServerUrl}/rooms/{roomId}/join", "POST"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            Debug.Log($"[GameClient] Отправляем POST запрос на присоединение к комнате {roomId}");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log($"[GameClient] Получен ответ от сервера: {json}");
                var room = JsonUtility.FromJson<Room>(json);
                Debug.Log($"[GameClient] Успешно присоединились к комнате: ID={room.id}, Название='{room.name}', Игроков={room.players}");
                onSuccess?.Invoke(room);
            }
            else
            {
                Debug.LogError($"[GameClient] Ошибка при присоединении к комнате: {request.error}");
                onError?.Invoke(request.error);
            }
        }
    }
} 
