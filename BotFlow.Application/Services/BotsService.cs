using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BotFlow.Application.Common.DTOs.Bots;
using BotFlow.Application.Interfaces;
using BotFlow.Domain.Entities;
using BotFlow.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotFlow.Application.Services
{
    public interface IBotsService
    {
        Task<BotResponse> GetBotByIdAsync(Guid id);
        Task<IEnumerable<BotResponse>> GetBotsAsync(BotFilterRequest filter);
        Task<BotStatsResponse> GetBotsStatsAsync(Guid userId);
        Task<BotResponse> CreateBotAsync(CreateBotRequest request, Guid userId);
        Task<BotResponse> UpdateBotAsync(Guid id, UpdateBotRequest request);
        Task<bool> DeleteBotAsync(Guid id);
        Task<bool> ChangeBotStatusAsync(Guid id, string status);
        Task<IEnumerable<BotResponse>> SearchBotsAsync(string query, Guid userId);
    }

    public class BotsService : IBotsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BotsService> _logger;

        public BotsService(
            ApplicationDbContext context,
            ILogger<BotsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BotResponse> GetBotByIdAsync(Guid id)
        {
            try
            {
                var bot = await _context.Bots
                    .Include(b => b.Pages)
                    .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

                if (bot == null)
                    throw new ApplicationException("Bot not found");

                return MapToBotResponse(bot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bot by ID: {BotId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<BotResponse>> GetBotsAsync(BotFilterRequest filter)
        {
            try
            {
                var query = _context.Bots
                    .Include(b => b.Pages)
                    .Where(b => b.IsActive);

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Status) && filter.Status != "all")
                    query = query.Where(b => b.Status == filter.Status);

                if (!string.IsNullOrEmpty(filter.SearchQuery))
                    query = query.Where(b => 
                        b.Name.Contains(filter.SearchQuery) || 
                        b.Description.Contains(filter.SearchQuery));

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortOrder == "asc" 
                        ? query.OrderBy(b => b.Name) 
                        : query.OrderByDescending(b => b.Name),
                    "updated" => filter.SortOrder == "asc" 
                        ? query.OrderBy(b => b.UpdatedAt) 
                        : query.OrderByDescending(b => b.UpdatedAt),
                    "chats" => filter.SortOrder == "asc" 
                        ? query.OrderBy(b => b.TotalConversations) 
                        : query.OrderByDescending(b => b.TotalConversations),
                    _ => query.OrderByDescending(b => b.CreatedAt)
                };

                // Apply pagination
                var bots = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return bots.Select(MapToBotResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bots");
                throw;
            }
        }

        public async Task<BotStatsResponse> GetBotsStatsAsync(Guid userId)
        {
            try
            {
                var stats = await _context.Bots
                    .Where(b => b.UserId == userId && b.IsActive)
                    .GroupBy(b => 1)
                    .Select(g => new BotStatsResponse
                    {
                        TotalBots = g.Count(),
                        ActiveBots = g.Count(b => b.Status == "active"),
                        ConversationsToday = g.Sum(b => b.TotalConversations) / 30, // Simplified
                        ResponseRate = g.Average(b => b.TotalConversations > 0 ? 94.0m : 0)
                    })
                    .FirstOrDefaultAsync() ?? new BotStatsResponse();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bots stats for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<BotResponse> CreateBotAsync(CreateBotRequest request, Guid userId)
        {
            try
            {
                var bot = new Bot
                {
                    Name = request.Name,
                    Description = request.Description,
                    Status = "draft",
                    FlowConfiguration = request.FlowConfiguration,
                    WelcomeMessage = request.WelcomeMessage,
                    FallbackMessage = request.FallbackMessage,
                    IsAutoResponder = request.IsAutoResponder,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add pages if specified
                if (request.PageIds != null && request.PageIds.Any())
                {
                    var pages = await _context.Pages
                        .Where(p => request.PageIds.Contains(p.Id) && p.UserId == userId)
                        .ToListAsync();
                    
                    foreach (var page in pages)
                    {
                        bot.Pages.Add(page);
                    }
                }

                await _context.Bots.AddAsync(bot);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Bot created successfully: {BotId}", bot.Id);

                return MapToBotResponse(bot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bot");
                throw;
            }
        }

        public async Task<BotResponse> UpdateBotAsync(Guid id, UpdateBotRequest request)
        {
            try
            {
                var bot = await _context.Bots
                    .Include(b => b.Pages)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (bot == null)
                    throw new ApplicationException("Bot not found");

                // Update properties if provided
                if (!string.IsNullOrEmpty(request.Name))
                    bot.Name = request.Name;

                if (!string.IsNullOrEmpty(request.Description))
                    bot.Description = request.Description;

                if (!string.IsNullOrEmpty(request.Status))
                    bot.Status = request.Status;

                if (!string.IsNullOrEmpty(request.FlowConfiguration))
                    bot.FlowConfiguration = request.FlowConfiguration;

                if (!string.IsNullOrEmpty(request.WelcomeMessage))
                    bot.WelcomeMessage = request.WelcomeMessage;

                if (!string.IsNullOrEmpty(request.FallbackMessage))
                    bot.FallbackMessage = request.FallbackMessage;

                if (request.IsAutoResponder.HasValue)
                    bot.IsAutoResponder = request.IsAutoResponder.Value;

                // Update pages if specified
                if (request.PageIds != null)
                {
                    bot.Pages.Clear();
                    var pages = await _context.Pages
                        .Where(p => request.PageIds.Contains(p.Id))
                        .ToListAsync();
                    
                    foreach (var page in pages)
                    {
                        bot.Pages.Add(page);
                    }
                }

                bot.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Bot updated successfully: {BotId}", bot.Id);

                return MapToBotResponse(bot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bot: {BotId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteBotAsync(Guid id)
        {
            try
            {
                var bot = await _context.Bots.FindAsync(id);
                if (bot == null)
                    return false;

                bot.IsActive = false;
                bot.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Bot deleted: {BotId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bot: {BotId}", id);
                throw;
            }
        }

        public async Task<bool> ChangeBotStatusAsync(Guid id, string status)
        {
            try
            {
                var bot = await _context.Bots.FindAsync(id);
                if (bot == null)
                    return false;

                if (!new[] { "active", "draft", "paused" }.Contains(status))
                    throw new ApplicationException("Invalid status");

                bot.Status = status;
                bot.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Bot status changed: {BotId} -> {Status}", id, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing bot status: {BotId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<BotResponse>> SearchBotsAsync(string query, Guid userId)
        {
            try
            {
                var bots = await _context.Bots
                    .Include(b => b.Pages)
                    .Where(b => b.UserId == userId && 
                               b.IsActive && 
                               (b.Name.Contains(query) || b.Description.Contains(query)))
                    .OrderByDescending(b => b.UpdatedAt)
                    .Take(10)
                    .ToListAsync();

                return bots.Select(MapToBotResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching bots");
                throw;
            }
        }

        private BotResponse MapToBotResponse(Bot bot)
        {
            return new BotResponse
            {
                Id = bot.Id,
                Name = bot.Name,
                Description = bot.Description,
                Status = bot.Status,
                Chats = bot.TotalConversations,
                Updated = GetTimeAgo(bot.UpdatedAt),
                Platforms = bot.Pages.Select(p => p.Platform).Distinct().ToArray(),
                Icon = GetBotIcon(bot.Name),
                IconBg = GetBotIconColor(bot.Id)
            };
        }

        private string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.UtcNow - date;
            
            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            
            return date.ToString("MMM dd");
        }

        private string GetBotIcon(string botName)
        {
            // Map bot names to icons based on keywords
            if (botName.Contains("commerce", StringComparison.OrdinalIgnoreCase) || 
                botName.Contains("store", StringComparison.OrdinalIgnoreCase) ||
                botName.Contains("shop", StringComparison.OrdinalIgnoreCase))
                return "fa-shopping-cart";
            
            if (botName.Contains("real estate", StringComparison.OrdinalIgnoreCase) || 
                botName.Contains("property", StringComparison.OrdinalIgnoreCase))
                return "fa-house";
            
            if (botName.Contains("appointment", StringComparison.OrdinalIgnoreCase) || 
                botName.Contains("booking", StringComparison.OrdinalIgnoreCase))
                return "fa-calendar-check";
            
            if (botName.Contains("support", StringComparison.OrdinalIgnoreCase) || 
                botName.Contains("help", StringComparison.OrdinalIgnoreCase))
                return "fa-headset";
            
            if (botName.Contains("promotion", StringComparison.OrdinalIgnoreCase) || 
                botName.Contains("campaign", StringComparison.OrdinalIgnoreCase))
                return "fa-percent";
            
            if (botName.Contains("lead", StringComparison.OrdinalIgnoreCase) || 
                botName.Contains("qualification", StringComparison.OrdinalIgnoreCase))
                return "fa-user-check";
            
            return "fa-robot";
        }

        private string GetBotIconColor(Guid botId)
        {
            // Generate consistent color based on bot ID
            var colors = new[]
            {
                "from-blue-500 to-blue-600",
                "from-purple-500 to-purple-600",
                "from-pink-500 to-pink-600",
                "from-green-500 to-green-600",
                "from-orange-500 to-orange-600",
                "from-teal-500 to-teal-600"
            };
            
            var index = Math.Abs(botId.GetHashCode()) % colors.Length;
            return colors[index];
        }
    }
}