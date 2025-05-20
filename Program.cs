using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Serilog.Sinks;
using Serilog;
using System.Net;
using System.Net.Sockets;

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/GameMasterServer.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(); // Использование Serilog
builder.Services.AddCors();

var serverIp = "ХУЙ";
var basePort = builder.Configuration.GetValue<int>("BasePort", 5000);

Log.Information("Сервер запущен на {Ip}:{Port}", "teamfluffygames.ru", basePort);

var app = builder.Build();

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

var rooms = new ConcurrentDictionary<int, Room>();
var roomProcesses = new ConcurrentDictionary<int, Process>();
var idCounter = 1;

Process StartRoomProcess(int roomId, string roomName, int port)
{
    var arguments = $"{roomId} \"{roomName}\" {serverIp} {port}";
    Log.Information("Запуск комнаты {RoomId} на {Ip}:{Port}", roomId, serverIp, port);
    Log.Information("Аргументы запуска: {Arguments}", arguments);
    
    var startInfo = new ProcessStartInfo
    {
        FileName = "/root/MasterServer/Scene/Room/Room.x86_64",
        Arguments = arguments,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
    };

    var process = Process.Start(startInfo);
    if (process == null)
    {
        Log.Error("Не удалось запустить Room.x86_64! Проверьте путь или права.");
        return null!;
    }

    process.OutputDataReceived += (s, e) =>
    {
        if (!string.IsNullOrEmpty(e.Data))
            Log.Information("Room {RoomId} out: {Data}", roomId, e.Data);
    };
    process.BeginOutputReadLine();

    process.ErrorDataReceived += (s, e) =>
    {
        if (!string.IsNullOrEmpty(e.Data))
            Log.Error("Room {RoomId} err: {Data}", roomId, e.Data);
    };
    process.BeginErrorReadLine();

    return process!;
}

app.MapGet("/", () => "GameMasterServer is running!");

app.MapGet("/rooms", () =>
{
    Log.Information("Успешный запрос на получение комнат");
    return Results.Ok(rooms.Values);
});

app.MapPost("/rooms", async (HttpContext http) =>
{
    var body = await http.Request.ReadFromJsonAsync<CreateRoomRequest>();
    
    var port = basePort + idCounter;
    
    var room = new Room
    {
        Id = idCounter++,
        Name = body?.Name ?? $"Room {idCounter}",
        Players = 0,
        Address = "ХУЙ222",
        Port = port
    };

    Log.Information("Создан объект комнаты: ID={Id}, Name={Name}, Address={Address}, Port={Port}", 
        room.Id, room.Name, room.Address, room.Port);

    rooms[room.Id] = room;
    Log.Information("Комната добавлена в словарь комнат");

    var process = StartRoomProcess(room.Id, room.Name, room.Address,room.Port);
    if (process == null)
    {
        Log.Error("Не удалось запустить процесс комнаты!");
        return Results.Problem("Failed to start room process");
    }

    roomProcesses[room.Id] = process;
    Log.Information("Процесс комнаты добавлен в словарь процессов");
    Log.Information("=== Комната успешно создана ===");

    return Results.Ok(room);
});

app.MapPost("/rooms/{id}/join", (int id) =>
{
    if (!rooms.TryGetValue(id, out var room))
    {
        Log.Warning("Попытка присоединиться к несуществующей комнате {RoomId}", id);
        return Results.NotFound(new { error = "Room not found" });
    }

    room.Players++;
    Log.Information("Игрок присоединился к комнате {RoomId}. Текущее количество игроков: {Players}", id, room.Players);
    return Results.Ok(room);
});

app.MapDelete("/rooms/{id}", (int id) =>
{
    if (roomProcesses.TryRemove(id, out var process))
    {
        if (!process.HasExited)
            process.Kill();

        rooms.TryRemove(id, out _);
        Log.Information("Комната {RoomId} остановлена и удалена.", id);
        return Results.Ok(new { message = $"Room {id} stopped and removed." });
    }

    Log.Warning("Попытка удалить несуществующую или неработающую комнату {RoomId}", id);
    return Results.NotFound(new { error = "Room not found or not running." });
});

app.Urls.Add("http://0.0.0.0:5000");
app.Run();

record Room
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Players { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
}

record CreateRoomRequest(string Name); 