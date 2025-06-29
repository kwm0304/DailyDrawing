using System.ComponentModel.DataAnnotations;

namespace Server.DTOs;

public class DeleteAccountDto
{
    [Required(ErrorMessage = "Password is required to delete account")]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    public string ConfirmationText { get; set; } = string.Empty;
}
