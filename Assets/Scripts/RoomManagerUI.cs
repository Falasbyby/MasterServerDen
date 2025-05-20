using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using static GameClient;

public class RoomManagerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button refreshRoomsButton;
    [SerializeField] private InputField roomNameInput;
    [SerializeField] private Transform roomsContainer;
    [SerializeField] private RoomButtonPrefab roomButtonPrefab;

    [SerializeField] private List<RoomButtonPrefab> roomButtons = new List<RoomButtonPrefab>();

    public GameClient gameClient;


    void Start()
    {

        // Подписываемся на кнопки
        createRoomButton.onClick.AddListener(OnCreateRoomClick);
        refreshRoomsButton.onClick.AddListener(RefreshRooms);

        // Загружаем список комнат при старте
        RefreshRooms();
    }

    private void OnCreateRoomClick()
    {
        string roomName = string.IsNullOrEmpty(roomNameInput.text) ? "Новая комната" : roomNameInput.text;

        gameClient.CreateRoom(
            roomName,
            onSuccess: (room) =>
            {
                Debug.Log($"Создана комната: {room.name}");
                roomNameInput.text = "";
                RefreshRooms();
            },
            onError: (error) =>
            {
                Debug.LogError($"Ошибка создания комнаты: {error}");
            }
        );
    }

    private void RefreshRooms()
    {

        gameClient.GetRooms(onSuccess: (rooms) =>
        {
            ClearRoomButtons();
            foreach (var room in rooms)
            {
                CreateRoomButton(room);
            }
        },
            onError: (error) =>
            {
                Debug.LogError($"Ошибка получения списка комнат: {error}");
            }
        );
    }

    private void CreateRoomButton(Room room)
    {
        RoomButtonPrefab buttonObj = Instantiate(roomButtonPrefab, roomsContainer);
        roomButtons.Add(buttonObj);
        buttonObj.SetRoom(room, OnRoomButtonClick);
    }

    private void OnRoomButtonClick(Room room)
    {
        gameClient.JoinRoom(
            room.id,
            onSuccess: (updatedRoom) =>
            {
                //Debug.Log($"Присоединились к комнате: {updatedRoom.name}");
                //RefreshRooms();
            },
            onError: (error) =>
            {
                Debug.LogError($"Ошибка присоединения к комнате: {error}");
            }
        );
    }

    private void ClearRoomButtons()
    {
        foreach (var button in roomButtons)
        {
            Destroy(button.gameObject);
        }
        roomButtons.Clear();
    }
}