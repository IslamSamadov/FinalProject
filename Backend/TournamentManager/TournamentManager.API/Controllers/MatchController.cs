using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TournamentManager.API.Data;
using TournamentManager.API.Entities;

namespace TournamentManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MatchController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("generate/{tournamentId}")]
        [Authorize]
        public async Task<IActionResult> GenerateRoundOne(int tournamentId)
        {
            var useStringId = User.FindFirstValue("UserId");
            if(!int.TryParse(useStringId, out var userId)) return Unauthorized();

            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if(tournament == null) return NotFound(new {Error = $"Tournament with {tournamentId} was not found."});

            if(tournament.OrganizerId != userId)
            {
                return StatusCode(403, new { Error = "Only the organizer can generate the bracket."});
            }

            if(tournament.Matches != null && tournament.Matches.Any())
            {
                return BadRequest(new {Error = "Bracket for this tournament has already been created."});
            }

            if(tournament.Teams == null || tournament.Teams.Count < 2)
            {
                return BadRequest(new {Error = "To generate bracket you need at least 2 teams."});
            }

            //Shuffle and pair teams
            var shuffledTeams = tournament.Teams.OrderBy(t => Guid.NewGuid()).ToList();
            var roundOneMatches = new List<Match>();

            for (int i = 0; i < shuffledTeams.Count; i += 2)
            {
                var match = new Match
                {
                    TournamentId = tournament.Id,
                    RoundNumber = 1,
                    TeamAId = shuffledTeams[i].Id,

                    // If there is an odd number of teams, the last team doesn't have an opponent!
                    // They get a a free win to Round 2. We leave TeamBId as null.
                    TeamBId = (i + 1 < shuffledTeams.Count) ? shuffledTeams[i + 1].Id : null
                };
                roundOneMatches.Add(match);
            }

            tournament.Status = "InProgress";
            _context.Matches.AddRange(roundOneMatches);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Message = $"Tournament '{tournament.Name}' has officially started! Generated {roundOneMatches.Count} matches for Round 1.",
                TotalTeams = shuffledTeams.Count
            });
        }
    }
}
