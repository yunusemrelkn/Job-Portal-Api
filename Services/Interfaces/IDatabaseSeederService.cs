namespace JobPortal.Api.Services.Interfaces
{
    public interface IDatabaseSeederService
    {
        Task SeedAsync();
        Task CreateDefaultAdminAsync();
        Task CreateSampleDataAsync();
    }
}
