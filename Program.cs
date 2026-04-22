using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

[cite_start]// Хранилище (CRUD в памяти) [cite: 364]
var _notes = new ConcurrentDictionary<int, NoteRecord>();
var _idCounter = 1;

[cite_start]// Диагностические эндпоинты [cite: 354, 357, 376]
app.MapGet("/health", () => new { Status = "ok", Time = DateTime.UtcNow });
app.MapGet("/version", (IConfiguration conf) => new { App = conf["App:Name"], Ver = conf["App:Version"] });
app.MapGet("/db/ping", () => Results.Problem("SQL Server not reachable yet (expected in Lab 7)"));

[cite_start]// Прикладной API "Заметки" [cite: 366]
app.MapPost("/api/notes", (NoteRecord note) => {
    var n = note with { Id = _idCounter++, CreatedAt = DateTime.UtcNow };
    _notes[n.Id] = n;
    return Results.Created($"/api/notes/{n.Id}", n);
});
app.MapGet("/api/notes", () => _notes.Values);

app.Run(); // Конец логики

// ТИПЫ ДОЛЖНЫ БЫТЬ ТОЛЬКО ЗДЕСЬ (В КОНЦЕ ФАЙЛА)
public record NoteRecord(int Id, string Title, string Text, DateTime? CreatedAt);