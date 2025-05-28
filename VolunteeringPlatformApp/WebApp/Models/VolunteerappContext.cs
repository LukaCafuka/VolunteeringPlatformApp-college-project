using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Models;

public partial class VolunteerappContext : DbContext
{
    public VolunteerappContext()
    {
    }

    public VolunteerappContext(DbContextOptions<VolunteerappContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<LogEntry> LogEntries { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectType> ProjectTypes { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("server=10.0.0.42;Database=volunteerapp;User=sa;Password=AlgebruhGrupa2024;TrustServerCertificate=True;MultipleActiveResultSets=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_User");

            entity.ToTable("AppUser");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);
            entity.Property(e => e.PswdHash).HasMaxLength(256);
            entity.Property(e => e.PswdSalt).HasMaxLength(256);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasMany(d => d.Projects).WithMany(p => p.Appusers)
                .UsingEntity<Dictionary<string, object>>(
                    "UserProject",
                    r => r.HasOne<Project>().WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Project_UserProject"),
                    l => l.HasOne<AppUser>().WithMany()
                        .HasForeignKey("AppuserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_User_UserProject"),
                    j =>
                    {
                        j.HasKey("AppuserId", "ProjectId").HasName("PK__UserProj__888929261057404D");
                        j.ToTable("UserProject");
                    });
        });

        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LogEntry__3214EC070412A756");

            entity.ToTable("LogEntry");

            entity.Property(e => e.Level).HasMaxLength(50);
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Project__3214EC07D2E0F8B4");

            entity.ToTable("Project");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.ProjectType).WithMany(p => p.Projects)
                .HasForeignKey(d => d.ProjectTypeId)
                .HasConstraintName("FK_ProjectType");

            entity.HasMany(d => d.Skills).WithMany(p => p.Projects)
                .UsingEntity<Dictionary<string, object>>(
                    "ProjectSkill",
                    r => r.HasOne<Skill>().WithMany()
                        .HasForeignKey("SkillId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Skill_ProjectSkill"),
                    l => l.HasOne<Project>().WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Project_ProjectSkill"),
                    j =>
                    {
                        j.HasKey("ProjectId", "SkillId").HasName("PK__ProjectS__1BE0B7E8A656561B");
                        j.ToTable("ProjectSkill");
                    });
        });

        modelBuilder.Entity<ProjectType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProjectT__3214EC073F1E5EE6");

            entity.ToTable("ProjectType");

            entity.Property(e => e.Name).HasMaxLength(70);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Skill__3214EC07F0B9BFA6");

            entity.ToTable("Skill");

            entity.Property(e => e.Name).HasMaxLength(70);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
