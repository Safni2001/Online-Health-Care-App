using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthCareApp.Data;
using HealthCareApp.Models;

namespace HealthCareApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HealthCareDbContext _context;

    public HomeController(ILogger<HomeController> logger, HealthCareDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var dbStatus = new { IsConnected = false, Message = "", ConnectionString = "" };
        
        try
        {
            // Test database connection
            await _context.Database.OpenConnectionAsync();
            await _context.Database.CloseConnectionAsync();
            
            dbStatus = new { 
                IsConnected = true, 
                Message = "✅ Database connection successful!", 
                ConnectionString = _context.Database.GetConnectionString() ?? "Connection string not available"
            };
            
            _logger.LogInformation("Database connection successful");
        }
        catch (Exception ex)
        {
            dbStatus = new { 
                IsConnected = false, 
                Message = $"❌ Database connection failed: {ex.Message}", 
                ConnectionString = ""
            };
            
            _logger.LogError(ex, "Database connection failed");
        }

        ViewBag.DbStatus = dbStatus;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
