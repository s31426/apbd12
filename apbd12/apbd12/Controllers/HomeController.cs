using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using apbd12.Models;

namespace apbd12.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var trips = await _context.Trips
            .Include(t => t.Countries)
            .Include(t => t.Clients)
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Name,
                t.Description,
                t.DateFrom,
                t.DateTo,
                t.MaxPeople,
                Countries = t.Countries.Select(c => new { c.Name }),
                Clients = t.Clients.Select(c => new { c.FirstName, c.LastName })
            }).ToListAsync();

        int totalTrips = await _context.Trips.CountAsync();
        return Ok(new
        {
            PageNum = page,
            PageSize = pageSize,
            AllPages = (int)Math.Ceiling((double)totalTrips / pageSize),
            Trips = trips
        });
    }
    
    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var client = await _context.Clients.Include(c => c.Trips).FirstOrDefaultAsync(c => c.IdClient == idClient);
        if (client == null) return NotFound("Client not found.");
        if (client.Trips.Any()) return BadRequest("Client has associated trips.");

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
        return NoContent();
    }

}