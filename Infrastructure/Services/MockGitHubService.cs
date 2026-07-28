using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models.Config;

namespace FlightPlan.Infrastructure.Services;

public class MockGitHubService : IGitHubService
{
    private readonly DashConfig _config;
    private readonly List<GitHubPullRequestDto> _mockPrs;

    public MockGitHubService(DashConfig config)
    {
        _config = config;
        _mockPrs = new List<GitHubPullRequestDto>
        {
            new GitHubPullRequestDto(
                "ARmunro/flightplan",
                42,
                "Feature: Add support for themes",
                "AndrewM",
                "open",
                DateTime.Now.AddDays(-1),
                "https://github.com/ARmunro/flightplan/pull/42",
                false,
                "This PR adds a new theme engine that supports custom CSS variables.",
                new List<GitHubCommentDto> {
                    new GitHubCommentDto("Junie", "Looks great! I'll review it shortly.", DateTime.Now.AddMinutes(-30))
                }
            ),
            new GitHubPullRequestDto(
                "ARmunro/flightplan",
                41,
                "Bugfix: Fix memory leak in background worker",
                "Junie",
                "open",
                DateTime.Now.AddDays(-3),
                "https://github.com/ARmunro/flightplan/pull/41",
                true,
                "Fixes a potential memory leak when the worker is restarted multiple times.",
                new List<GitHubCommentDto>()
            ),
            new GitHubPullRequestDto(
                "ARmunro/dashboard-ui",
                12,
                "Update dependencies to latest versions",
                "Dependabot",
                "open",
                DateTime.Now.AddDays(-7),
                "https://github.com/ARmunro/dashboard-ui/pull/12",
                false,
                "Updates various NPM packages to their latest secure versions.",
                new List<GitHubCommentDto>()
            )
        };
    }

    public Task<IEnumerable<GitHubPullRequestDto>> GetMyPullRequestsAsync()
    {
        return Task.FromResult(_mockPrs.AsEnumerable());
    }

    public Task<IEnumerable<GitHubPullRequestDto>> GetPullRequestsByQueryAsync(string query)
    {
        // Simple mock: return all for now
        return Task.FromResult(_mockPrs.AsEnumerable());
    }
}
