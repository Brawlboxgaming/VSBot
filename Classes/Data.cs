using Microsoft.EntityFrameworkCore;
using VPBot.Commands;

namespace VPBot.Classes
{
    public class VPContext : DbContext
    {
        public DbSet<TrackSubmission> Submissions { get; set; }
        public DbSet<NewTrack> NewTracks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
#if DEBUG
            string db = "testVP";
#else
            string db = "VP";
#endif
            options.UseSqlServer(Util.GetDBConnectionString(db));
            options.EnableSensitiveDataLogging(true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrackSubmission>().ToTable("Submissions");
            modelBuilder.Entity<NewTrack>().ToTable("NewTracks");
        }
    }
}
