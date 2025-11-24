using System.ComponentModel.DataAnnotations;

namespace InspectionSystem.Models;

public class TemplateInfo
{
    [Required]
    [MaxLength(200)]
    public string TemplateName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string InspectionLevel { get; set; } = "II";

    [Required]
    public bool IsActive { get; set; } = true;
}