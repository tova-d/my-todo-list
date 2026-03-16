using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace TodoApi;

public partial class ToDoDbContext : DbContext
{
   // בנאי (Constructor) ריק - נחוץ כדי ש-Entity Framework יוכל ליצור מופע של ה-Context
    public ToDoDbContext()
    {
    }

    // בנאי שמקבל הגדרות חיצוניות (כמו ה-Connection String) ומעביר אותן למחלקת הבסיס
    public ToDoDbContext(DbContextOptions<ToDoDbContext> options)
        : base(options)
    {
    }

    // מגדיר את הטבלה 'Items' כקבוצה של אובייקטים שניתן לשלוף ולשנות
    public virtual DbSet<Item> Items { get; set; }

    // פונקציה שקובעת את הגדרות החיבור (במקרה הזה, קישור ל-appsettings וגרסת ה-MySQL)
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        // כאן אנחנו אומרים לו: "אל תחפש קיצורי דרך, קח את הכתובת המלאה מה-appsettings"
        optionsBuilder.UseMySql("server=localhost;port=3306;user=root;password=;database=tododb", 
            Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.4.3-mysql"));
    }
}
    // הפונקציה המרכזית שקובעת איך הקוד ב-C# יתורגם למבנה הטבלאות במסד הנתונים
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            // הגדרת שיטת קידוד התווים (Collation) עבור מסד הנתונים
            .UseCollation("utf8mb4_0900_ai_ci")
            // הגדרת סט התווים (CharSet) לתמיכה רחבה בשפות
            .HasCharSet("utf8mb4");

        // הגדרת המיפוי הספציפי עבור הטבלה 'Item'
        modelBuilder.Entity<Item>(entity =>
        {
            // קביעת השדה Id כמפתח הראשי של הטבלה
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            // קישור המחלקה לטבלה הפיזית שנקראת "items" במסד הנתונים
            entity.ToTable("items");

            // הגדרת הגבלת אורך של 100 תווים לשדה השם
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        // קריאה לפונקציה שמאפשרת להוסיף הגדרות נוספות בקבצים אחרים (Partial)
        OnModelCreatingPartial(modelBuilder);
    }

    // הצהרה על פונקציה חלקית (Partial) לשימוש עתידי או הרחבות
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
