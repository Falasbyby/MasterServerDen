using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
    public Text roomIdText;

    void Start()
    {
        string[] args = Environment.GetCommandLineArgs();

        int roomId = -1;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--roomId" && i + 1 < args.Length)
            {
                int.TryParse(args[i + 1], out roomId);
                break;
            }
        }

        roomIdText.text = $"Room ID: {roomId}";
        Debug.Log($"[Room] Started Room with ID: {roomId}");
    }
}
