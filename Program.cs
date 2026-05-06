using System.Collections.Concurrent;
using Microsoft.Data.SqlClient;  // <-- правильный using

var builder = WebApplication.CreateBuilder(args);

// Swagger не обязателен, поэтому убираем его
// builder.Services.AddEndpointsApiExplorer(); 
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// Строки для Swagger тоже убираем
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// In-memory storage for notes
var notes = new ConcurrentDictionary<int, Note>();
int nextId = 1;

// --- Diagnostic endpoints ---
app.MapGet("/health", () => new { status = "ok", timestamp = DateTime.UtcNow });

app.MapGet("/version", (IConfiguration config) => new
{
    appName = config["App:Name"] ?? "IsLabApp",
    version = config["App:Version"] ?? "0.0.1"
});

// --- Notes CRUD (in-memory) ---
app.MapPost("/api/notes", (NoteDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest("Title is required");

    var note = new Note
    {
        Id = nextId++,
        Title = dto.Title,
        Text = dto.Text,
        CreatedAt = DateTime.UtcNow
    };
    notes[note.Id] = note;
    return Results.Created($"/api/notes/{note.Id}", note);
});

app.MapGet("/api/notes", () => notes.Values.OrderByDescending(n => n.Id));

app.MapGet("/api/notes/{id:int}", (int id) =>
{
    if (notes.TryGetValue(id, out var note))
        return Results.Ok(note);
    return Results.NotFound();
});

app.MapDelete("/api/notes/{id:int}", (int id) =>
{
    if (notes.TryRemove(id, out var note))
        return Results.Ok(note);
    return Results.NotFound();
});

// --- DB ping (проверка подключения к SQL Server) ---
app.MapGet("/db/ping", (IConfiguration config) =>
{
    var connStr = config.GetConnectionString("Mssql");
    if (string.IsNullOrEmpty(connStr))
        return Results.Ok(new { status = "no_connection_string" });

    try
    {
        using var conn = new SqlConnection(connStr);
        conn.Open();
        return Results.Ok(new { status = "ok" });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { status = "error", message = ex.Message });
    }
});

app.Run();

// --- Models ---
record Note
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Text { get; set; }
    public DateTime CreatedAt { get; set; }
}

record NoteDto(string Title, string? Text);