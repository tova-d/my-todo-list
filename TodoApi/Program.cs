// using Microsoft.EntityFrameworkCore;
// using TodoApi; // ודאי שזה תואם ל-Namespace בקובץ Item.cs

// var builder = WebApplication.CreateBuilder(args);

// // --- רישום שירותים (Services) ---

// // הזרקת ה-Context: מחבר את ה-API למסד הנתונים של ה-ToDo
// builder.Services.AddDbContext<ToDoDbContext>();

// // הוספת Swagger חלק א': מאפשר ל-API לחשוף את נקודות הקצה (Endpoints) שלו
// builder.Services.AddEndpointsApiExplorer();

// // הוספת Swagger חלק ב': מייצר את הממשק הגרפי שמאפשר לבדוק את ה-API בדפדפן
// builder.Services.AddSwaggerGen();

// // הגדרת CORS: מאפשר לאפליקציות חיצוניות (כמו ה-React שתבני) לגשת ל-API הזה

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll",
//         policy => policy.AllowAnyOrigin() // מאפשר לכל אתר (כמו React) לגשת
//                         .AllowAnyMethod() // מאפשר PUT, DELETE, POST
//                         .AllowAnyHeader()); // מאפשר שליחת JSON
// });
// // יצירת האובייקט app - קורה פעם אחת בלבד!
// // יצירת האובייקט app - קורה פעם אחת בלבד!
// var app = builder.Build();
using Microsoft.EntityFrameworkCore;
using TodoApi; 

var builder = WebApplication.CreateBuilder(args);

// --- רישום שירותים (Services) ---

// הגדרת ה-ConnectionString עם הפרטים של Clever Cloud
var connectionString = "Server=bsma6jabhyhy7zavpxez-mysql.services.clever-cloud.com;Database=bsma6jabhyhy7zavpxez;Uid=u7sifwqeu6qoac9r;Pwd=P5U6fX33y0p9N5mNPrf4;";

// הזרקת ה-Context עם הגדרת MySQL
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// הוספת Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// הגדרת CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();
// --- הוספה חדשה: יצירת הטבלאות באופן אוטומטי ---
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
// --- סוף ההוספה ---

// --- הגדרות צינור העבודה (Middleware Pipeline) ---

// הפעלת Swagger בדפדפן
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
// --- הגדרת ה-Routes (הניתובים) ---

// 1. שליפת כל המשימות
app.MapGet("/items", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());

// 2. הוספת משימה חדשה
app.MapPost("/items", async (ToDoDbContext db, Item item) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

// 3. עדכון משימה (סימון כבוצע או שינוי טקסט)
app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item inputItem) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.IsComplete = inputItem.IsComplete;
    // אם השם שנשלח ריק, תשמור על השם הקיים בפיל
    if (!string.IsNullOrEmpty(inputItem.Name)) {
        item.Name = inputItem.Name;
    }

    await db.SaveChangesAsync();
    return Results.NoContent();
});
// 4. מחיקת משימה
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

app.Run(); // הפעלת השרת