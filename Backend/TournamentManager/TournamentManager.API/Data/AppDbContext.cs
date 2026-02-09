using Microsoft.EntityFrameworkCore;

namespace TournamentManager.API.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    }
}
