using JobPortal.Api.Data;
using JobPortal.Api.Models.Entities;
using JobPortal.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Api.Services.Implementations
{
    public class DatabaseSeederService : IDatabaseSeederService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<DatabaseSeederService> _logger;

        public DatabaseSeederService(
            ApplicationDbContext context,
            IPasswordService passwordService,
            ILogger<DatabaseSeederService> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Ensure database is created
                await _context.Database.EnsureCreatedAsync();

                // Run pending migrations
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation($"Applying {pendingMigrations.Count()} pending migrations...");
                    await _context.Database.MigrateAsync();
                    _logger.LogInformation("Migrations applied successfully.");
                }

                // Seed default admin
                await CreateDefaultAdminAsync();

                // Seed sample data if in development
                await CreateSampleDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        public async Task CreateDefaultAdminAsync()
        {
            try
            {
                // Check if admin user already exists
                if (!await _context.Users.AnyAsync(u => u.Role == UserRole.Admin))
                {
                    var adminUser = new User
                    {
                        Name = "System",
                        Surname = "Administrator",
                        Email = "admin@jobseeker.com",
                        Role = UserRole.Admin,
                        PasswordHash = _passwordService.HashPassword("Admin123!"),
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(adminUser);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Default admin user created successfully.");
                    _logger.LogInformation("Admin credentials: admin@jobseeker.com / Admin123!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default admin user.");
                throw;
            }
        }

        public async Task CreateSampleDataAsync()
        {
            try
            {
                // Only create sample data if no companies exist
                if (!await _context.Companies.AnyAsync())
                {
                    _logger.LogInformation("Creating sample data...");

                    // Create sample companies
                    var techSector = await _context.Sectors.FirstAsync(s => s.Name == "Technology");
                    var financeSector = await _context.Sectors.FirstAsync(s => s.Name == "Finance");

                    var companies = new List<Company>
                    {
                        new Company
                        {
                            Name = "TechCorp Solutions",
                            Description = "Leading software development company specializing in web and mobile applications.",
                            Location = "San Francisco, CA",
                            SectorId = techSector.SectorId
                        },
                        new Company
                        {
                            Name = "FinanceMax Inc",
                            Description = "Financial services company providing innovative banking solutions.",
                            Location = "New York, NY",
                            SectorId = financeSector.SectorId
                        },
                        new Company
                        {
                            Name = "StartupLab",
                            Description = "Innovative startup focused on AI and machine learning solutions.",
                            Location = "Austin, TX",
                            SectorId = techSector.SectorId
                        }
                    };

                    _context.Companies.AddRange(companies);
                    await _context.SaveChangesAsync();

                    // Create sample employer users
                    var techCorp = companies.First(c => c.Name == "TechCorp Solutions");
                    var financeMax = companies.First(c => c.Name == "FinanceMax Inc");

                    var employers = new List<User>
                    {
                        new User
                        {
                            Name = "John",
                            Surname = "Manager",
                            Email = "john.manager@techcorp.com",
                            Phone = "+1-555-0101",
                            Role = UserRole.Employer,
                            CompanyId = techCorp.CompanyId,
                            PasswordHash = _passwordService.HashPassword("Password123!"),
                            CreatedAt = DateTime.UtcNow
                        },
                        new User
                        {
                            Name = "Sarah",
                            Surname = "Director",
                            Email = "sarah.director@financemax.com",
                            Phone = "+1-555-0102",
                            Role = UserRole.Employer,
                            CompanyId = financeMax.CompanyId,
                            PasswordHash = _passwordService.HashPassword("Password123!"),
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    _context.Users.AddRange(employers);
                    await _context.SaveChangesAsync();

                    // Create sample job seekers
                    var jobSeekers = new List<User>
                    {
                        new User
                        {
                            Name = "Alice",
                            Surname = "Developer",
                            Email = "alice.developer@email.com",
                            Phone = "+1-555-0201",
                            Role = UserRole.JobSeeker,
                            PasswordHash = _passwordService.HashPassword("Password123!"),
                            CreatedAt = DateTime.UtcNow
                        },
                        new User
                        {
                            Name = "Bob",
                            Surname = "Designer",
                            Email = "bob.designer@email.com",
                            Phone = "+1-555-0202",
                            Role = UserRole.JobSeeker,
                            PasswordHash = _passwordService.HashPassword("Password123!"),
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    _context.Users.AddRange(jobSeekers);
                    await _context.SaveChangesAsync();

                    // Create sample jobs
                    var engineeringDept = await _context.Departments.FirstAsync(d => d.Name == "Engineering");
                    var marketingDept = await _context.Departments.FirstAsync(d => d.Name == "Marketing");
                    var johnManager = employers.First(e => e.Email == "john.manager@techcorp.com");
                    var sarahDirector = employers.First(e => e.Email == "sarah.director@financemax.com");

                    var jobs = new List<Job>
                    {
                        new Job
                        {
                            Title = "Senior Full Stack Developer",
                            Description = "We are looking for an experienced Full Stack Developer to join our dynamic team. You will be responsible for developing and maintaining web applications using modern technologies.",
                            Location = "San Francisco, CA",
                            SalaryMin = 120000,
                            SalaryMax = 180000,
                            CreatedBy = johnManager.UserId,
                            CompanyId = techCorp.CompanyId,
                            DepartmentId = engineeringDept.DepartmentId,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Job
                        {
                            Title = "React Frontend Developer",
                            Description = "Join our frontend team to build amazing user interfaces using React, TypeScript, and modern CSS frameworks.",
                            Location = "Remote",
                            SalaryMin = 90000,
                            SalaryMax = 140000,
                            CreatedBy = johnManager.UserId,
                            CompanyId = techCorp.CompanyId,
                            DepartmentId = engineeringDept.DepartmentId,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Job
                        {
                            Title = "Digital Marketing Specialist",
                            Description = "We need a creative digital marketing specialist to drive our online presence and customer acquisition strategies.",
                            Location = "New York, NY",
                            SalaryMin = 60000,
                            SalaryMax = 90000,
                            CreatedBy = sarahDirector.UserId,
                            CompanyId = financeMax.CompanyId,
                            DepartmentId = marketingDept.DepartmentId,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    _context.Jobs.AddRange(jobs);
                    await _context.SaveChangesAsync();

                    // Create sample CVs
                    var aliceDeveloper = jobSeekers.First(js => js.Email == "alice.developer@email.com");
                    var bobDesigner = jobSeekers.First(js => js.Email == "bob.designer@email.com");

                    var cvs = new List<CV>
                    {
                        new CV
                        {
                            UserId = aliceDeveloper.UserId,
                            Summary = "Experienced full-stack developer with 5+ years of experience in web development. Passionate about creating scalable and efficient applications.",
                            ExperienceYears = 5,
                            EducationLevel = "Bachelor's Degree",
                            SkillsText = "Proficient in React, Node.js, PostgreSQL, and AWS. Experience with Agile methodologies and team collaboration.",
                            CreatedAt = DateTime.UtcNow
                        },
                        new CV
                        {
                            UserId = bobDesigner.UserId,
                            Summary = "Creative UI/UX designer with a passion for user-centered design. 3+ years of experience in digital product design.",
                            ExperienceYears = 3,
                            EducationLevel = "Associate Degree",
                            SkillsText = "Expert in Figma, Adobe Creative Suite, user research, and prototyping. Strong understanding of modern design principles.",
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    _context.CVs.AddRange(cvs);
                    await _context.SaveChangesAsync();

                    // Add skills to CVs
                    var reactSkill = await _context.Skills.FirstAsync(s => s.Name == "React");
                    var jsSkill = await _context.Skills.FirstAsync(s => s.Name == "JavaScript");
                    var nodeSkill = await _context.Skills.FirstAsync(s => s.Name == "Node.js");
                    var sqlSkill = await _context.Skills.FirstAsync(s => s.Name == "SQL");

                    var aliceCV = cvs.First(cv => cv.UserId == aliceDeveloper.UserId);
                    var cvSkills = new List<CVSkill>
                    {
                        new CVSkill { CvId = aliceCV.CvId, SkillId = reactSkill.SkillId },
                        new CVSkill { CvId = aliceCV.CvId, SkillId = jsSkill.SkillId },
                        new CVSkill { CvId = aliceCV.CvId, SkillId = nodeSkill.SkillId },
                        new CVSkill { CvId = aliceCV.CvId, SkillId = sqlSkill.SkillId }
                    };

                    _context.CVSkills.AddRange(cvSkills);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Sample data created successfully.");
                    _logger.LogInformation("Sample users created:");
                    _logger.LogInformation("- Employer: john.manager@techcorp.com / Password123!");
                    _logger.LogInformation("- Employer: sarah.director@financemax.com / Password123!");
                    _logger.LogInformation("- Job Seeker: alice.developer@email.com / Password123!");
                    _logger.LogInformation("- Job Seeker: bob.designer@email.com / Password123!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sample data.");
                throw;
            }
        }
    }
}
