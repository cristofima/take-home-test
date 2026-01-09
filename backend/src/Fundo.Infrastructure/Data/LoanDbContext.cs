using Microsoft.EntityFrameworkCore;
using Fundo.Domain.Entities;
using Fundo.Domain.Constants;

namespace Fundo.Infrastructure.Data;

public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options)
    {
    }

    public DbSet<Loan> Loans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Loan entity
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("Loans");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(e => e.CurrentBalance)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(e => e.ApplicantName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasMaxLength(15)
                .IsRequired()
                .HasDefaultValue(LoanStatus.Active);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Create indexes for better query performance
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ApplicantName);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Seed data matching the frontend mockup
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Use static dates to avoid non-deterministic model changes
        var baseDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Loan>().HasData(
            new Loan
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Amount = 25000.00m,
                CurrentBalance = 18750.00m,
                ApplicantName = "John Doe",
                Status = LoanStatus.Active,
                CreatedAt = new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Loan
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Amount = 15000.00m,
                CurrentBalance = 0.00m,
                ApplicantName = "Jane Smith",
                Status = LoanStatus.Paid,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Loan
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Amount = 50000.00m,
                CurrentBalance = 32500.00m,
                ApplicantName = "Robert Johnson",
                Status = LoanStatus.Active,
                CreatedAt = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 12, 17, 0, 0, 0, DateTimeKind.Utc)
            },
            new Loan
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Amount = 10000.00m,
                CurrentBalance = 0.00m,
                ApplicantName = "Emily Williams",
                Status = LoanStatus.Paid,
                CreatedAt = new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Loan
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Amount = 75000.00m,
                CurrentBalance = 72000.00m,
                ApplicantName = "Michael Brown",
                Status = LoanStatus.Active,
                CreatedAt = new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}