namespace MicroserviceTemplate.DAL.Model.Database;

public class User
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
}