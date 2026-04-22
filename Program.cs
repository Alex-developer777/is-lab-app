using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

[cite_start]// Модель заметки [cite: 365]
public record Note(int Id, string Title, string Text, DateTime CreatedAt);
[cite_start]// Хранилище в памяти (CRUD) [cite: 364]
var _notes = new ConcurrentDictionary<int, Note>();
var _idCounter = 1;

[cite_start]// 1. Диагностика: /health [cite: 356]
app.MapGet("/health", () => new { Status = "ok", Time = DateTime.UtcNow });

[cite_start]// 2. Диагностика: /version [cite: 357]
app.MapGet("/version", (IConfiguration conf) => new {
    AppName = conf["App:Name"],
    Version = conf["App:Version"]
});

[cite_start]// 3. Проверка БД: /db/ping [cite: 376, 377]
app.MapGet("/db/ping", (IConfiguration conf) => {
    var connectionString = conf.GetConnectionString("Mssql");
    [cite_start]// Здесь мы просто имитируем логику, так как БД будет в ЛР7 [cite: 378]
    return Results.Problem("SQL Server not reachable yet (expected in Lab 7)");
});

[cite_start]// 4. API Заметки: CRUD [cite: 366]
app.MapPost("/api/notes", (Note note) => {
    var newNote = note with { Id = _idCounter++, CreatedAt = DateTime.UtcNow };
    _notes[newNote.Id] = newNote;
    return Results.Created($"/api/notes/{newNote.Id}", newNote);
});

app.MapGet("/api/notes", () => _notes.Values);

app.MapGet("/api/notes/{id}", (int id) =>
    _notes.TryGetValue(id, out var n) ? Results.Ok(n) : Results.NotFound());

app.MapDelete("/api/notes/{id}", (int id) =>
    _notes.TryRemove(id, out _) ? Results.NoContent() : Results.NotFound());

app.Run();