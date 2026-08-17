using System;
using System.Collections.Generic;
using System.Text;

namespace MatchFinder.Domain.Entities;

public class MatchPlayer
{
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
