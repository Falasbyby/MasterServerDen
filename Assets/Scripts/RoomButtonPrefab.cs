using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using static GameClient;
public class RoomButtonPrefab : MonoBehaviour
{
    public Button button;
    public Text roomText;

    private Room room;


    public void SetRoom(Room room, Action<Room> onClick)
    {
        this.room = room;
        roomText.text = $"Комната: {room.name}\nИгроков: {room.players}";
        button.onClick.AddListener(() => onClick(this.room));
    }
}