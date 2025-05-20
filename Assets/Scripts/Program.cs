#if !UNITY_EDITOR && UNITY_EDITOR
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

var rooms = new ConcurrentDictionary<int, Room>();
var roomProcesses = new ConcurrentDictionary<int, Process>();
var idCounter = 1;

Process StartRoomProcess(int roomId, string roomName)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "/root/MasterServer/GameRooms/RoomExecutable", // путь к твоему исполняемому файлу комнаты
        Arguments = $"{roomId} \"{roomName}\"",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
    };

    var process = Process.Start(startInfo);
    if (process != null)
    {
        process.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine($"Room {roomId} out: {e.Data}");
        };
        process.BeginOutputReadLine();

        process.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine($"Room {roomId} err: {e.Data}");
        };
        process.BeginErrorReadLine();
    }
    return process!;
}

app.MapGet("/", () => "GameMasterServer is running!");

app.MapGet("/rooms", () => rooms.Values);

app.MapPost("/rooms", async (HttpContext http) =>
{
    var body = await http.Request.ReadFromJsonAsync<CreateRoomRequest>();
    var room = new Room
    {
        Id = idCounter++,
        Name = body?.Name ?? $"Room {idCounter}",
        Players = 0
    };

    rooms[room.Id] = room;

    var process = StartRoomProcess(room.Id, room.Name);
    roomProcesses[room.Id] = process;

    return Results.Ok(room);
});

app.MapPost("/rooms/{id}/join", (int id) =>
{
    if (!rooms.TryGetValue(id, out var room))
        return Results.NotFound(new { error = "Room not found" });

    room.Players++;
    return Results.Ok(room);
});

app.MapDelete("/rooms/{id}", (int id) =>
{
    if (roomProcesses.TryRemove(id, out var process))
    {
        if (!process.HasExited)
            process.Kill();

        rooms.TryRemove(id, out _);
        return Results.Ok(new { message = $"Room {id} stopped and removed." });
    }
    return Results.NotFound(new { error = "Room not found or not running." });
});

app.Urls.Add("http://0.0.0.0:5000");
app.Run();

record Room
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Players { get; set; }
}

record CreateRoomRequest(string Name);
#endif
