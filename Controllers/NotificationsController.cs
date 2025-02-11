using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using Inventory_Management_System__Miracle_Shop_.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly MiracleDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationsController(MiracleDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // Get unread notifications count
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _context.Notifications.CountAsync(n => !n.IsRead);
        return Ok(count);
    }

    // Get all notifications (sorted recent to oldest)
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var notifications = await _context.Notifications.OrderByDescending(n => n.CreatedAt).ToListAsync();
        return Ok(notifications);
    }

    // Mark all notifications as read
    [HttpPost("mark-as-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var notifications = await _context.Notifications.Where(n => !n.IsRead).ToListAsync();
        notifications.ForEach(n => n.IsRead = true);
        await _context.SaveChangesAsync();
        return Ok();
    }


}
