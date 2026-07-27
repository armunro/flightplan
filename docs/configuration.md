# Configuration

FlightPlan stores its configuration and data in `%AppData%\FlightPlan`. 
On the first run, it will generate a default `config.yaml` in that directory.

You can also provide a `config.yaml` in the project root which will be migrated to the AppData folder.

## Configuration Examples

### Main Configuration (`config.yaml`)
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

### Projects and Tasks (`projects.yaml`)
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

### Bookmarks (`bookmarks.yaml`)
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

### Scheduled Tasks (`scheduled_tasks.yaml`)
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

### Automation Rules (`Rules/*.yaml`)
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
