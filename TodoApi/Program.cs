using Microsoft.EntityFrameworkCore;
using TodoApi; 

var builder = WebApplication.CreateBuilder(args);

// --- 1. הגדרת מסד הנתונים (Clever Cloud) ---
var connectionString = "Server=bsma6jabhyhy7zavpxez-mysql.services.clever-cloud.com;Database=bsma6jabhyhy7zavpxez;Uid=u7sifwqeu6qoac9r;Pwd=P5U6fX33y0p9N5mNPrf4;";

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// --- 2. רישום שירותים נוספים ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 3. הגדרת CORS (חיבור ל-React) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// --- 4. יצירת הטבלאות באופן אוטומטי ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ToDoDbContext>();
        context.Database.EnsureCreated();
        Console.WriteLine("Database and tables created/verified successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating database: {ex.Message}");
    }
}

// --- 5. הגדרות Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// --- 6. הגדרת ה-Routes (API Endpoints) ---

app.MapGet("/", () => "ToDo API is running!"); // בדיקת תקינות פשוטה

app.MapGet("/items", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());

app.MapPost("/items", async (ToDoDbContext db, Item item) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item inputItem) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.IsComplete = inputItem.IsComplete;
    if (!string.IsNullOrEmpty(inputItem.Name)) item.Name = inputItem.Name;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) =>
{
    if (await db.Items.FindAsync(id) is Item item)
    {
        db.Items.Remove(item);
        await db.SaveChangesAsync();
        return Results.Ok(item);
    }
    return Results.NotFound();
});

app.Run();