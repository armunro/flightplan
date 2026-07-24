# FlightPlan

FlightPlan is a comprehensive developer productivity dashboard that aggregates data from Jira, GitHub, and Microsoft Graph (Outlook/Calendar) into a single, unified view. It features a modern Vue-based frontend and a robust .NET 9 backend.

## Features

- **Jira Integration**: Monitor your Jira issues, starred items, and custom JQL queries.
- **GitHub Integration**: Track pull requests, reviews, and repository activity.
- **Microsoft Graph Integration**: Unified access to your Outlook emails and Calendar events.
- **Task Management**: Simple task tracking with scheduled tasks and alarm support.
- **Rule Engine**: Automated actions for email and task management.
- **Bookmarks & Notes**: Built-in notepad and bookmark management for quick access to resources.
- **Developer Tools**: Integrated debug and system monitoring views.

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
The application stores its configuration and data in `%AppData%\FlightPlan`. 
On the first run, it will generate a default `config.yaml` in that directory.

You can also provide a `config.yaml` in the project root which will be migrated to the AppData folder.

**Configuration Examples:**

#### Main Configuration (`config.yaml`)
```yaml
jira:
  url: "https://your-domain.atlassian.net/"
  username: "user@example.com"
  apiToken: "your-api-token"
  queries:
    - name: "My Open Issues"
      jql: "assignee = currentUser() AND status != Closed"
    - name: "Starred"
      jql: "issueKey in (starredIssues())"

github:
  organization: "your-org"
  username: "your-username"
  accessToken: "your-personal-access-token"
  queries:
    - name: "My PRs"
      query: "is:open is:pr author:your-username"
    - name: "Need Review"
      query: "is:open is:pr review-requested:your-username"

microsoftGraph:
  tenantId: "your-azure-tenant-id"
  clientId: "your-azure-client-id"

pageVisibilities:
  - id: "jira"
    visible: true
  - id: "github"
    visible: true
  - id: "tasks"
    visible: true
  - id: "scheduledtasks"
    visible: true
  - id: "alarms"
    visible: true
  - id: "email"
    visible: true
  - id: "calendar"
    visible: true
  - id: "links"
    visible: true
  - id: "notepad"
    visible: true
  - id: "debug"
    visible: true
```

#### Projects and Tasks (`projects.yaml`)
```yaml
- id: "d6b5e0a0-6f7d-4b5a-9c2e-1234567890ab"
  name: "Main Project"
  description: "Personal tasks and goals"
  icon: "bi-house"
  color: "#ffca28"
  statuses:
    - id: "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"
      name: "To Do"
      color: "#cccccc"
      isCompletedState: false
      order: 1
    - id: "b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e"
      name: "In Progress"
      color: "#2196f3"
      isCompletedState: false
      order: 2
    - id: "c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f"
      name: "Done"
      color: "#4caf50"
      isCompletedState: true
      order: 3
  taskTypes:
    - id: "11111111-2222-3333-4444-555555555555"
      name: "Feature"
      color: "#673ab7"
      icon: "bi-gear"
    - id: "22222222-3333-4444-5555-666666666666"
      name: "Bug"
      color: "#f44336"
      icon: "bi-bug"
  priorities:
    - id: "33333333-4444-5555-6666-777777777777"
      name: "Critical"
      color: "#d32f2f"
      icon: "bi-exclamation-octagon"
      order: 1
  lists:
    - id: "44444444-5555-6666-7777-888888888888"
      name: "Inbox"
      tasks:
        - id: "55555555-6666-7777-8888-999999999999"
          title: "Setup FlightPlan configuration"
          description: "Follow the README to configure all services."
          isCompleted: false
          statusId: "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"
          priority: "High"
          estimateMinutes: 30
```

#### Bookmarks (`bookmarks.yaml`)
```yaml
- id: "cat-1"
  name: "Development"
  color: "#2196f3"
  bookmarks:
    - id: "bm-1"
      title: "GitHub"
      url: "https://github.com"
      description: "Source control"
      icon: "bi-github"
    - id: "bm-2"
      title: "Stack Overflow"
      url: "https://stackoverflow.com"
      icon: "bi-patch-question"
  subcategories:
    - id: "subcat-1"
      name: "Docs"
      bookmarks:
        - id: "bm-3"
          title: "MDN Web Docs"
          url: "https://developer.mozilla.org"
          icon: "bi-file-text"
```

#### Scheduled Tasks (`scheduled_tasks.yaml`)
```yaml
- id: "10000000-0000-0000-0000-000000000001"
  name: "Daily Standup"
  cronSchedule: "0 0 9 * * MON-FRI"
  isEnabled: true
  projectId: "d6b5e0a0-6f7d-4b5a-9c2e-1234567890ab"
  listId: "44444444-5555-6666-7777-888888888888"
  taskTitleTemplate: "Prepare for Standup - {date}"
  taskDescription: "Review PRs and tasks completed yesterday."
  priority: "Medium"
```

#### Alarms (`alarms.yaml`)
```yaml
- id: "f0000000-0000-0000-0000-000000000001"
  title: "Tea Break"
  type: "Timer"
  duration: "00:10:00"
  isActive: true
  isCompleted: false
```

#### Automation Rules (`Rules/*.yaml`)
```yaml
name: "GitHub Notifications"
color: "#333333"
rootFolder: "Inbox"
filters:
  - from:
      - "notifications@github.com"
    subjectContains:
      - "[PR]"
      - "Issue"
actions:
  - type: "MarkAsRead"
  - type: "Move"
    value: "Work/GitHub"
  - type: "Star"
```

### 3. Build and Run

#### The Easy Way (Visual Studio / Rider)
Simply open `FlightPlan.sln` and run the project. The MSBuild targets are configured to automatically build the frontend during the build process.

#### Manual Build (CLI)
**Build the Frontend:**
```bash
cd frontend
npm install
npm run build
cd ..
```

**Build and Run the Backend:**
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