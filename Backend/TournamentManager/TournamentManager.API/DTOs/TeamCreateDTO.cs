using System.ComponentModel.DataAnnotations;

namespace TournamentManager.API.DTOs
{
    public class TeamCreateDTO
    {
        [Required(ErrorMessage = "Team name is required.")]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int TournamentId { get; set; }
    }
}
