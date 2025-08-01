using JobPortal.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace JobPortal.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<CompanyWorker> CompanyWorkers { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<CV> CVs { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<CVSkill> CVSkills { get; set; }
        public DbSet<JobSkill> JobSkills { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names (optional - EF will use plural by default)
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Company>().ToTable("Companies");
            modelBuilder.Entity<Sector>().ToTable("Sectors");
            modelBuilder.Entity<Department>().ToTable("Departments");
            modelBuilder.Entity<CompanyWorker>().ToTable("CompanyWorkers");
            modelBuilder.Entity<Job>().ToTable("Jobs");
            modelBuilder.Entity<CV>().ToTable("CVs");
            modelBuilder.Entity<Skill>().ToTable("Skills");
            modelBuilder.Entity<CVSkill>().ToTable("CVSkills");
            modelBuilder.Entity<JobSkill>().ToTable("JobSkills");
            modelBuilder.Entity<Application>().ToTable("Applications");
            modelBuilder.Entity<Favorite>().ToTable("Favorites");

            // Configure enum conversions
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Application>()
                .Property(a => a.Status)
                .HasConversion<string>();

            // Configure string lengths for better database optimization
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Surname).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Location).HasMaxLength(200);
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.SalaryMin).HasColumnType("decimal(10,2)");
                entity.Property(e => e.SalaryMax).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<CV>(entity =>
            {
                entity.Property(e => e.Summary).HasMaxLength(1000);
                entity.Property(e => e.EducationLevel).HasMaxLength(100);
                entity.Property(e => e.SkillsText).HasMaxLength(2000);
            });

            modelBuilder.Entity<Sector>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<Skill>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            });

            // Configure relationships and constraints

            // User - Company relationship (nullable for job seekers)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            // CompanyWorker relationships
            modelBuilder.Entity<CompanyWorker>()
                .HasOne(cw => cw.Company)
                .WithMany(c => c.CompanyWorkers)
                .HasForeignKey(cw => cw.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompanyWorker>()
                .HasOne(cw => cw.User)
                .WithMany(u => u.CompanyWorkers)
                .HasForeignKey(cw => cw.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompanyWorker>()
                .HasOne(cw => cw.Department)
                .WithMany(d => d.CompanyWorkers)
                .HasForeignKey(cw => cw.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Job relationships
            modelBuilder.Entity<Job>()
                .HasOne(j => j.Creator)
                .WithMany(u => u.CreatedJobs)
                .HasForeignKey(j => j.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Job>()
                .HasOne(j => j.Company)
                .WithMany(c => c.Jobs)
                .HasForeignKey(j => j.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Job>()
                .HasOne(j => j.Department)
                .WithMany(d => d.Jobs)
                .HasForeignKey(j => j.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // CV relationships
            modelBuilder.Entity<CV>()
                .HasOne(cv => cv.User)
                .WithMany(u => u.CVs)
                .HasForeignKey(cv => cv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Application relationships
            modelBuilder.Entity<Application>()
                .HasOne(a => a.User)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // Favorite relationships
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Job)
                .WithMany(j => j.Favorites)
                .HasForeignKey(f => f.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // Skill relationships
            modelBuilder.Entity<CVSkill>()
                .HasOne(cs => cs.CV)
                .WithMany(cv => cv.CVSkills)
                .HasForeignKey(cs => cs.CvId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CVSkill>()
                .HasOne(cs => cs.Skill)
                .WithMany(s => s.CVSkills)
                .HasForeignKey(cs => cs.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobSkill>()
                .HasOne(js => js.Job)
                .WithMany(j => j.JobSkills)
                .HasForeignKey(js => js.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobSkill>()
                .HasOne(js => js.Skill)
                .WithMany(s => s.JobSkills)
                .HasForeignKey(js => js.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            // Company - Sector relationship
            modelBuilder.Entity<Company>()
                .HasOne(c => c.Sector)
                .WithMany(s => s.Companies)
                .HasForeignKey(c => c.SectorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            modelBuilder.Entity<Application>()
                .HasIndex(a => new { a.UserId, a.JobId })
                .IsUnique()
                .HasDatabaseName("IX_Applications_UserId_JobId");

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.JobId })
                .IsUnique()
                .HasDatabaseName("IX_Favorites_UserId_JobId");

            modelBuilder.Entity<CVSkill>()
                .HasIndex(cs => new { cs.CvId, cs.SkillId })
                .IsUnique()
                .HasDatabaseName("IX_CVSkills_CvId_SkillId");

            modelBuilder.Entity<JobSkill>()
                .HasIndex(js => new { js.JobId, js.SkillId })
                .IsUnique()
                .HasDatabaseName("IX_JobSkills_JobId_SkillId");

            // Additional indexes for performance
            modelBuilder.Entity<Job>()
                .HasIndex(j => j.CompanyId)
                .HasDatabaseName("IX_Jobs_CompanyId");

            modelBuilder.Entity<Job>()
                .HasIndex(j => j.CreatedAt)
                .HasDatabaseName("IX_Jobs_CreatedAt");

            modelBuilder.Entity<Application>()
                .HasIndex(a => a.Status)
                .HasDatabaseName("IX_Applications_Status");

            // Configure default values
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Job>()
                .Property(j => j.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<CV>()
                .Property(cv => cv.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Application>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Sectors
            modelBuilder.Entity<Sector>().HasData(
                new Sector { SectorId = 1, Name = "Technology" },
                new Sector { SectorId = 2, Name = "Healthcare" },
                new Sector { SectorId = 3, Name = "Finance" },
                new Sector { SectorId = 4, Name = "Education" },
                new Sector { SectorId = 5, Name = "Manufacturing" },
                new Sector { SectorId = 6, Name = "Retail" },
                new Sector { SectorId = 7, Name = "Consulting" },
                new Sector { SectorId = 8, Name = "Government" }
            );

            // Seed Departments
            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentId = 1, Name = "Engineering" },
                new Department { DepartmentId = 2, Name = "Marketing" },
                new Department { DepartmentId = 3, Name = "Sales" },
                new Department { DepartmentId = 4, Name = "Human Resources" },
                new Department { DepartmentId = 5, Name = "Finance" },
                new Department { DepartmentId = 6, Name = "Operations" },
                new Department { DepartmentId = 7, Name = "Customer Service" },
                new Department { DepartmentId = 8, Name = "Research & Development" }
            );

            // Seed Skills
            modelBuilder.Entity<Skill>().HasData(
                // Programming Languages
                new Skill { SkillId = 1, Name = "C#" },
                new Skill { SkillId = 2, Name = "JavaScript" },
                new Skill { SkillId = 3, Name = "Python" },
                new Skill { SkillId = 4, Name = "Java" },
                new Skill { SkillId = 5, Name = "TypeScript" },
                new Skill { SkillId = 6, Name = "PHP" },
                new Skill { SkillId = 7, Name = "Go" },
                new Skill { SkillId = 8, Name = "Ruby" },

                // Frameworks & Technologies
                new Skill { SkillId = 9, Name = "React" },
                new Skill { SkillId = 10, Name = "Vue.js" },
                new Skill { SkillId = 11, Name = "Angular" },
                new Skill { SkillId = 12, Name = "Node.js" },
                new Skill { SkillId = 13, Name = "ASP.NET Core" },
                new Skill { SkillId = 14, Name = "Django" },
                new Skill { SkillId = 15, Name = "Spring Boot" },

                // Databases
                new Skill { SkillId = 16, Name = "SQL" },
                new Skill { SkillId = 17, Name = "PostgreSQL" },
                new Skill { SkillId = 18, Name = "MySQL" },
                new Skill { SkillId = 19, Name = "MongoDB" },
                new Skill { SkillId = 20, Name = "Redis" },

                // Cloud & DevOps
                new Skill { SkillId = 21, Name = "AWS" },
                new Skill { SkillId = 22, Name = "Azure" },
                new Skill { SkillId = 23, Name = "Docker" },
                new Skill { SkillId = 24, Name = "Kubernetes" },
                new Skill { SkillId = 25, Name = "CI/CD" },

                // Soft Skills
                new Skill { SkillId = 26, Name = "Project Management" },
                new Skill { SkillId = 27, Name = "Team Leadership" },
                new Skill { SkillId = 28, Name = "Communication" },
                new Skill { SkillId = 29, Name = "Problem Solving" },
                new Skill { SkillId = 30, Name = "Agile/Scrum" }
            );
        }
    }
}