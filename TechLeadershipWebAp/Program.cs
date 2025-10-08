using Microsoft.EntityFrameworkCore;
using TechLeadershipWebApp.Data;
using TechLeadershipWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite("Data Source=techleadership.db");
});

// Configure HttpClient for AI Service
builder.Services.AddHttpClient<GeminiAIService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register Services
builder.Services.AddScoped<IAIQuestionGenerator, GeminiAIService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Assessment/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Assessment}/{action=Index}/{id?}");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        Console.WriteLine("Database initialized successfully.");
        
        // Check if we have any results
        var resultCount = context.TestResults.Count();
        Console.WriteLine($"Database contains {resultCount} assessment results.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
}

Console.WriteLine("Application starting...");
app.Run();