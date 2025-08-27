using Microsoft.EntityFrameworkCore;
using HealthCareApp.Data;
using HealthCareApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add database connection
builder.Services.AddDbContext<HealthCareDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

var app = builder.Build();

// Test database connection and show success message
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<HealthCareDbContext>();
        context.Database.OpenConnection();
        context.Database.CloseConnection();
        
        Console.WriteLine("✅ Database connection successful!");
        Console.WriteLine("🚀 HealthCare Management App is running successfully!");
        Console.WriteLine($"📊 Connected to: {builder.Configuration.GetConnectionString("DefaultConnection")}");
        
        // Initialize database with sample data
        await DbInitializer.Initialize(context);
        Console.WriteLine("✅ Database initialized with sample data!");
    }
}
catch (Exception ex)
{
    Console.WriteLine("❌ Database connection failed!");
    Console.WriteLine($"Error: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
