using DowntimeTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace DowntimeTracker.Data
{
    public class TCZNT5000 : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable(nameof(Users), t => t.ExcludeFromMigrations());
            modelBuilder.Entity<MachineLineArea>().ToTable(nameof(MachineLineAreas), t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Downtime>().ToTable(nameof(Downtimes), t => t.ExcludeFromMigrations());
            modelBuilder.Entity<InfoRecord>().ToTable(nameof(InfoRecords), t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Hrm>().ToTable(nameof(Hrms), t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Personel>().ToTable(nameof(Personel), t => t.ExcludeFromMigrations());
        }

        public TCZNT5000(DbContextOptions<TCZNT5000> options) : base(options)
        { }

        public DbSet<User>? Users { get; set; }
        public DbSet<MachineLineArea>? MachineLineAreas { get; set; }
        public DbSet<Downtime>? Downtimes { get; set; }
        public DbSet<InfoRecord>? InfoRecords { get; set; }
        public DbSet<Hrm>? Hrms { get; set; }
        public DbSet<Personel>? Personel { get; set; }
        public DbSet<UserLog>? UserLogs { get; set; }
    }

    public class TCZNT5000Raptor : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DepartmentCustomer>().ToTable(nameof(Departments), t => t.ExcludeFromMigrations());
            modelBuilder.Entity<CategoryReason>().ToTable(nameof(Categories), t => t.ExcludeFromMigrations());
        }

        public TCZNT5000Raptor(DbContextOptions<TCZNT5000Raptor> options) : base(options)
        { }

        public DbSet<DepartmentCustomer>? Departments { get; set; }
        public DbSet<CategoryReason>? Categories { get; set; }
    }
}