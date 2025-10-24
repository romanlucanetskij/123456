using System.ComponentModel.DataAnnotations;

namespace StudentApi.Dtos;

public class StudentRequest
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Group { get; set; } = string.Empty;

    [Range(0, 5)]
    public double Gpa { get; set; }
}
