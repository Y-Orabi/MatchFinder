using System;
using System.Collections.Generic;
using System.Text;
using MatchFinder.Domain;
using MatchFinder.Domain.Entities;
using MatchFinder.Infrastructure.Common;
using MatchFinder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MatchFinder.Infrastructure.Services;

public interface IMatchService
{
    Task<List<Match>> GetActiveMatchesAsync(string? sport, string? venue);
    Task<Match?> GetMatchByIdAsync(Guid id);
    Task<ServiceResult> CreateMatchAsync(Match match);
    Task<ServiceResult> JoinMatchAsync(Guid matchId, Guid userId);
    Task<ServiceResult> LeaveMatchAsync(Guid matchId, Guid userId);
}

public class MatchService : IMatchService
{
    private readonly MatchFinderDbContext _context;

    public MatchService(MatchFinderDbContext context) => _context = context;

    public async Task<List<Match>> GetActiveMatchesAsync(string? sport, string? venue)
    {
        var query = _context.Matches
            .Include(m => m.Host)
            .Include(m => m.MatchPlayers)
            .Where(m => m.Status == MatchStatus.Open);

        if (!string.IsNullOrWhiteSpace(sport))
            query = query.Where(m => m.SportType.Contains(sport));

        if (!string.IsNullOrWhiteSpace(venue))
            query = query.Where(m => m.Venue.Contains(venue));

        return await query.OrderBy(m => m.MatchDate).ToListAsync();
    }

    public async Task<Match?> GetMatchByIdAsync(Guid id)
    {
        return await _context.Matches
            .Include(m => m.Host)
            .Include(m => m.MatchPlayers)
            .ThenInclude(mp => mp.User)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<ServiceResult> CreateMatchAsync(Match match)
    {
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> JoinMatchAsync(Guid matchId, Guid userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var match = await _context.Matches
                .Include(m => m.MatchPlayers)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null) return ServiceResult.Failure("Match not found.");
            if (match.MatchPlayers.Count >= match.MaxPlayers) return ServiceResult.Failure("Match is fully booked.");
            if (match.MatchPlayers.Any(mp => mp.UserId == userId)) return ServiceResult.Failure("You have already joined this match.");

            _context.MatchPlayers.Add(new MatchPlayer { MatchId = matchId, UserId = userId });
            match.CurrentPlayersCount = match.MatchPlayers.Count + 1;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return ServiceResult.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return ServiceResult.Failure("Concurrent modification detected. Please try again.");
        }
    }

    public async Task<ServiceResult> LeaveMatchAsync(Guid matchId, Guid userId)
    {
        var matchPlayer = await _context.MatchPlayers
            .FirstOrDefaultAsync(mp => mp.MatchId == matchId && mp.UserId == userId);

        if (matchPlayer == null) return ServiceResult.Failure("You are not part of this match.");

        _context.MatchPlayers.Remove(matchPlayer);

        var match = await _context.Matches.FindAsync(matchId);
        if (match != null && match.CurrentPlayersCount > 1)
        {
            match.CurrentPlayersCount--;
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }
}