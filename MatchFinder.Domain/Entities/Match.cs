using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;

namespace MatchFinder.Domain.Entities;

public class Match
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SportType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public int MaxPlayers { get; set; }
    public int CurrentPlayersCount { get; set; } = 1;
    public Guid HostId { get; set; }
    public ApplicationUser Host { get; set; } = null!;
    public MatchStatus Status { get; set; } = MatchStatus.Open;

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<MatchPlayer> MatchPlayers { get; set; } = new List<MatchPlayer>();
}
