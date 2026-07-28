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
            ),
            new GitHubPullRequestDto(
                "ARmunro/flightplan",
                43,
                "Refactor: Extract common utility functions",
                "AndrewM",
                "open",
                DateTime.Now.AddDays(-2),
                "https://github.com/ARmunro/flightplan/pull/43",
                false,
                "Extracting repeated logic into a shared Utilities class to improve code reuse.",
                new List<GitHubCommentDto>()
            ),
            new GitHubPullRequestDto(
                "ARmunro/flightplan",
                44,
                "Docs: Update README with deployment instructions",
                "Junie",
                "open",
                DateTime.Now.AddDays(-4),
                "https://github.com/ARmunro/flightplan/pull/44",
                false,
                "Added a section on how to deploy the application using Docker and Kubernetes.",
                new List<GitHubCommentDto>()
            ),
            new GitHubPullRequestDto(
                "ARmunro/dashboard-ui",
                13,
                "Feature: Implement dark mode support",
                "AndrewM",
                "open",
                DateTime.Now.AddDays(-5),
                "https://github.com/ARmunro/dashboard-ui/pull/13",
                false,
                "Adding a toggle to switch between light and dark modes in the UI.",
                new List<GitHubCommentDto>()
            ),
            new GitHubPullRequestDto(
                "ARmunro/flightplan",
                45,
                "Test: Add integration tests for API",
                "QA-Bot",
                "open",
                DateTime.Now.AddDays(-6),
                "https://github.com/ARmunro/flightplan/pull/45",
                true,
                "Adding new integration tests to cover the recently added authentication features.",
                new List<GitHubCommentDto>()
            ),
            new GitHubPullRequestDto(
                "ARmunro/core-lib",
                105,
                "Fix: Resolve race condition in task scheduler",
                "AndrewM",
                "open",
                DateTime.Now.AddDays(-8),
                "https://github.com/ARmunro/core-lib/pull/105",
                false,
                "Critical fix for a race condition that was causing occasional application crashes.",
                new List<GitHubCommentDto>()
            ),
            new GitHubPullRequestDto(
                "ARmunro/flightplan",
                46,
                "Chore: Update project to .NET 9.0",
                "AndrewM",
                "closed",
                DateTime.Now.AddDays(-10),
                "https://github.com/ARmunro/flightplan/pull/46",
                false,
                "Upgrading the entire solution to the latest LTS version of .NET.",
                new List<GitHubCommentDto>()
            ),
            new GitHubPullRequestDto(
                "ARmunro/dashboard-ui",
                14,
                "Perf: Optimize component rendering in large lists",
                "Junie",
                "open",
                DateTime.Now.AddDays(-1),
                "https://github.com/ARmunro/dashboard-ui/pull/14",
                false,
                "Using memoization to prevent unnecessary re-renders in the project list view.",
                new List<GitHubCommentDto>()
            ),
            new GitHubPullRequestDto(
                "ARmunro/flightplan",
                47,
                "Security: Patch CVE-2026-XXXX in dependency",
                "Security-Team",
                "open",
                DateTime.Now.AddDays(-2),
                "https://github.com/ARmunro/flightplan/pull/47",
                false,
                "Urgent patch for a newly discovered security vulnerability in the JSON parser.",
                new List<GitHubCommentDto>()
            ),
            new GitHubPullRequestDto(
                "ARmunro/flightplan",
                48,
                "I18n: Add German translation files",
                "AndrewM",
                "open",
                DateTime.Now.AddDays(-3),
                "https://github.com/ARmunro/flightplan/pull/48",
                false,
                "Translating the UI strings into German for the European market expansion.",
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
