using ChronicTruant.History;
using ChronicTruant.Persistence.Entities;
using ChronicTruant.Teachers;
using Microsoft.EntityFrameworkCore;

namespace ChronicTruant.Persistence;

public class ChronicTruantDbContext : DbContext
{
    public ChronicTruantDbContext(DbContextOptions<ChronicTruantDbContext> options)
        : base(options)
    {
    }

    public DbSet<SemesterEntity> Semesters => Set<SemesterEntity>();
    public DbSet<TeacherRuleEntity> TeacherRules => Set<TeacherRuleEntity>();
    public DbSet<SemesterDayEntity> SemesterDays => Set<SemesterDayEntity>();
    public DbSet<LessonEntity> Lessons => Set<LessonEntity>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Перечисления хранятся строками, чтобы таблицы можно было читать глазами.
        configurationBuilder.Properties<Subject>().HaveConversion<string>().HaveMaxLength(32);
        configurationBuilder.Properties<RuleType>().HaveConversion<string>().HaveMaxLength(32);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SemesterEntity>(semester =>
        {
            semester.HasMany(s => s.TeacherRules)
                .WithOne()
                .HasForeignKey(r => r.SemesterId);

            semester.HasMany(s => s.Days)
                .WithOne()
                .HasForeignKey(d => d.SemesterId);

            semester.HasIndex(s => s.IsFinished);
        });

        modelBuilder.Entity<TeacherRuleEntity>()
            .HasKey(r => new { r.SemesterId, r.Subject });

        modelBuilder.Entity<SemesterDayEntity>(day =>
        {
            day.HasKey(d => new { d.SemesterId, d.Day });

            day.HasMany(d => d.Lessons)
                .WithOne()
                .HasForeignKey(l => new { l.SemesterId, l.Day });
        });

        modelBuilder.Entity<LessonEntity>()
            .HasKey(l => new { l.SemesterId, l.Day, l.Subject });
    }
}
