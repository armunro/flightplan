# Features

FlightPlan is designed to be a "single pane of glass" for developers, integrating various tools into a unified productivity environment.

## 📊 Dashboard (My Day)
The "My Day" view provides an at-a-glance summary of your workday:
- **Tasks Due Today**: Automatically aggregates tasks from all projects that have a due date of today.
- **Recent Emails**: Shows the latest messages from your Inbox, highlighting those that match your automation rules.
- **Calendar Events**: Displays your upcoming meetings and appointments for the day.

## 📝 Task Management
A flexible task tracking system that goes beyond simple lists:
- **Hierarchical Tasks**: Support for nested subtasks to break down complex work.
- **Bulk Operations**: Move or update multiple tasks simultaneously.
- **Contextual Creation**: Quickly create tasks from emails or Jira issues to maintain your flow.
- **Project Organization**: Organize tasks into projects with customizable statuses, types, and priorities.

## 🤖 Rule Engine
Automate your workflow with a powerful rule-based system:
- **Email Automation**: Automatically categorize, move, or star emails based on sender, subject, or content.
- **One-Click Rules**: Create new automation rules directly from an existing email.
- **Bulk Application**: Manually run rules against your entire inbox or specific folders.
- **Visual Feedback**: Matching rules are visually tagged on emails in the dashboard and email views.

## ⏰ Scheduled Tasks
Never miss recurring responsibilities:
- **Cron-based Scheduling**: Use standard Cron expressions to define complex recurrence patterns.
- **Template Support**: Automatically generate tasks with dynamic titles (e.g., including the current date).
- **Automation**: Tasks are automatically created in your specified project and list by the background worker.

## 📧 Microsoft Graph Integration
Full access to your Microsoft 365 data:
- **Email**: Browse folders, read messages, and manage your inbox.
- **Calendar**: Integrated calendar view with support for multiple calendars.
- **Actionable**: Delete emails or convert them to tasks directly from the UI.

## 🛠️ Developer Integrations
- **Jira**: Monitor starred issues and run custom JQL queries. Convert Jira issues into local tasks for detailed tracking.
- **GitHub**: Track Pull Requests (authored or assigned for review) and repository activity.

## 📓 Notepad
A built-in Markdown editor for your thoughts and documentation:
- **Markdown Support**: Full syntax highlighting and preview using MD Editor V3.
- **File Management**: Create, save, and delete multiple notes stored as `.md` files.

## 🔗 Bookmarks
Quick access to your most-used resources:
- **Categorization**: Organize bookmarks into categories and subcategories.
- **Customization**: Assign icons and colors to categories for easy identification.

## ⚙️ Settings & Monitoring
- **Web Configuration**: Update your service credentials and page visibility settings directly through the UI.
- **Debug View**: Monitor system health, view application paths, and check the status of the background job scheduler.
