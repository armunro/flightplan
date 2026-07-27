# FlightPlan

FlightPlan is a comprehensive developer productivity dashboard that aggregates data from Jira, GitHub, and Microsoft Graph (Outlook/Calendar) into a single, unified view. It features a modern Vue-based frontend and a robust .NET 9 backend.

## Features

- **Dashboard (My Day)**: Unified view of tasks, emails, and calendar events.
- **Jira Integration**: Monitor issues, starred items, and custom JQL queries.
- **GitHub Integration**: Track pull requests and repository activity.
- **Microsoft Graph Integration**: Access Outlook emails and Calendar events.
- **Task Management**: Advanced tracking with subtasks, bulk actions, and contextual creation.
- **Scheduled Tasks**: Automated task creation using Cron schedules.
- **Rule Engine**: Automated actions for email management with manual and bulk support.
- **Bookmarks & Notes**: Built-in notepad and categorized bookmark management.
- **Developer Tools**: Integrated debug and system monitoring views.

For a detailed breakdown of all features, see [docs/features.md](docs/features.md).

## Tech Stack

- **Backend**: .NET 9.0 (ASP.NET Core)
  - **Dependency Injection**: Autofac
  - **Job Scheduling**: Quartz.NET
  - **APIs**: Octokit (GitHub), Atlassian.SDK (Jira), Microsoft Graph SDK
  - **Configuration**: YamlDotNet
- **Frontend**: Vue 3 + Vite
  - **UI Components**: Codemirror, FullCalendar, MD Editor V3
  - **Styling**: Tailwind CSS (integrated via Vite)

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18 or later recommended)
- [npm](https://www.npmjs.com/)
- An Azure AD Application Registration (for Microsoft Graph features)
- A Jira API Token and GitHub Personal Access Token

## Getting Started

### 1. Clone the repository
```bash
git clone https://github.com/your-username/flightplan.git
cd flightplan
```

### 2. Configuration
Detailed configuration documentation can be found in [docs/configuration.md](docs/configuration.md).

### 3. Build and Run

#### The Easy Way (Visual Studio / Rider)
Simply open `FlightPlan.sln` and run the project. The MSBuild targets are configured to automatically build the frontend during the build process.

#### Manual Build (CLI)
```bash
dotnet build
dotnet run
```
The application will be available at `http://localhost:5000` (or the configured port). It automatically redirects to `/dashboard`.

## Project Structure

- `FlightPlan/`: Main ASP.NET Core project.
  - `Controllers/`: API endpoints for the dashboard.
  - `Core/`: Domain models and interfaces.
  - `Infrastructure/`: External service implementations (Jira, GitHub, Graph).
  - `Services/`: Internal application logic and storage management.
  - `Pages/`: Razor Pages for the hosting container.
- `frontend/`: Vue 3 application source code.
- `wwwroot/`: Static assets and compiled frontend (generated).

## License

Distributed under the MIT License. See `LICENSE` for more information.