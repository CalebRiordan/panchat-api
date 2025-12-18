using Microsoft.EntityFrameworkCore;
using PanChatApi.Models;

namespace PanChatApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
}
