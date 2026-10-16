
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Data;
public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options): base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventRoute> EventRoutes => Set<EventRoute>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USERS
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", table =>
            {
                table.HasCheckConstraint(
                    "CK_Users_Role", "[Role] IN ('Organiser', 'Participant')");
            });

            entity.HasKey(e => e.UserID)
                .HasName("PK_Users");

            entity.Property(e => e.UserID)
                .ValueGeneratedNever();

            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("UQ_Users_Email");

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Participant")
                .IsRequired();
        });

        // EVENTS
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events", table =>
            {
                table.HasCheckConstraint(
                    "CK_Events_Type", "[EventType] IN ('Running', 'Walking', 'Cycling')");
            });

            entity.HasKey(e => e.EventID)
                .HasName("PK_Events");

            entity.Property(e => e.EventID)
                .ValueGeneratedNever();

            entity.Property(e => e.EventName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(e => e.EventDate)
                .HasColumnType("date");

            entity.Property(e => e.Location)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.Property(e => e.EventType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Running")
                .IsRequired();

            entity.HasOne(e => e.Organiser)
                .WithMany(u => u.OrganisedEvents)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Events_Organiser");
        });

        // CATEGORIES
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories", table =>
            {
                table.HasCheckConstraint(
                    "CK_Categories_Distance",
                    "[DistanceKM] > 0");

                table.HasCheckConstraint(
                    "CK_Categories_MaxParticipants",
                    "[MaximumParticipants] > 0");

                table.HasCheckConstraint(
                    "CK_Categories_EntryFee",
                    "[EntryFee] >= 0");
            });

            entity.HasKey(e => e.CategoryID)
                .HasName("PK_Categories");

            entity.Property(e => e.CategoryID)
                .ValueGeneratedNever();

            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(e => e.DistanceKM)
                .HasColumnType("decimal(6,2)");

            entity.Property(e => e.MaximumParticipants)
                .HasDefaultValue(100);

            entity.Property(e => e.EntryFee)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0m);

            entity.HasIndex(e => new
            {
                e.EventID,
                e.CategoryName
            })
                .IsUnique()
                .HasDatabaseName("UQ_Categories_Event_Name");

            entity.HasOne(e => e.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Categories_Event");
        });

        // EVENT ROUTES
        modelBuilder.Entity<EventRoute>(entity =>
        {
            entity.ToTable("EventRoutes", table =>
            {
                table.HasCheckConstraint(
                    "CK_EventRoutes_Distance",
                    "[DistanceKM] > 0");
            });

            entity.HasKey(e => e.RouteID)
                .HasName("PK_EventRoutes");

            entity.Property(e => e.RouteID)
                .ValueGeneratedNever();

            entity.Property(e => e.RouteName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(e => e.DistanceKM)
                .HasColumnType("decimal(6,2)");

            entity.Property(e => e.StartPoint)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(e => e.FinishPoint)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(e => e.RouteDescription)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasIndex(e => e.EventID)
                .IsUnique()
                .HasDatabaseName("UQ_EventRoutes_Event");

            entity.HasOne(e => e.Event)
                .WithOne(e => e.Route)
                .HasForeignKey<EventRoute>(e => e.EventID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EventRoutes_Event");
        });

        // ENROLLMENTS
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollments", table =>
            {
                table.HasCheckConstraint(
                    "CK_Enrollments_Status",
                    "[Status] IN ('Confirmed', 'Pending', 'Cancelled')");
            });

            entity.HasKey(e => e.EnrollmentID)
                .HasName("PK_Enrollments");

            entity.Property(e => e.EnrollmentID)
                .ValueGeneratedNever();

            entity.Property(e => e.EnrollmentDate)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSDATETIME()");

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Confirmed")
                .IsRequired();

            entity.HasIndex(e => new
            {
                e.UserID,
                e.EventID,
                e.CategoryID
            })
                .IsUnique()
                .HasDatabaseName(
                    "UQ_Enrollments_User_Event_Category");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Enrollments_User");

            entity.HasOne(e => e.Event)
                .WithMany(e => e.Enrollments)
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Enrollments_Event");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Enrollments_Category");
        });

        // RESULTS
        modelBuilder.Entity<Result>(entity =>
        {
            entity.ToTable("Results", table =>
            {
                table.HasCheckConstraint(
                    "CK_Results_Position",
                    "[Position] > 0");

                table.HasCheckConstraint(
                    "CK_Results_Pace",
                    "[Pace] IS NULL OR [Pace] > 0");
            });

            entity.HasKey(e => e.ResultID)
                .HasName("PK_Results");

            entity.Property(e => e.ResultID)
                .ValueGeneratedNever();

            entity.Property(e => e.FinishTime)
                .HasColumnType("time");

            entity.Property(e => e.Position)
                .IsRequired();

            entity.Property(e => e.Pace)
                .HasColumnType("decimal(5,2)");

            entity.Property(e => e.ResultDate)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(e => e.EnrollmentID)
                .IsUnique()
                .HasDatabaseName("UQ_Results_Enrollment");

            entity.HasOne(e => e.Enrollment)
                .WithOne(e => e.Result)
                .HasForeignKey<Result>(e => e.EnrollmentID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Results_Enrollment");
        });
    }
}

