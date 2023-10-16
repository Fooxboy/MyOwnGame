using System.ComponentModel.DataAnnotations;

namespace MyOwnGame.Backend.Database;

public class User
{
    [Key]
    public long Id { get; set; }
    
    public string Name { get; set; }
    
    public string AvatarImage { get; set; }
}