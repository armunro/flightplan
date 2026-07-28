export async function fetchJiraIssues(jql = null) {
  let url = '/api/jira';
  if (jql) {
    url += `?jql=${encodeURIComponent(jql)}`;
  }
  const response = await fetch(url);
  if (!response.ok) throw new Error('Failed to fetch Jira issues');
  return response.json();
}

export async function fetchJiraIssue(key) {
  const response = await fetch(`/api/jira/issue/${encodeURIComponent(key)}`);
  if (!response.ok) {
    if (response.status === 404) return null;
    throw new Error('Failed to fetch Jira issue');
  }
  return response.json();
}

export async function fetchJiraQueries() {
  const response = await fetch('/api/jira/queries');
  if (!response.ok) throw new Error('Failed to fetch Jira queries');
  return response.json();
}

export async function fetchGitHubPrs(query = null) {
  let url = '/api/github';
  if (query) {
    url += `?query=${encodeURIComponent(query)}`;
  }
  const response = await fetch(url);
  if (!response.ok) throw new Error('Failed to fetch GitHub PRs');
  return response.json();
}

export async function fetchGitHubQueries() {
  const response = await fetch('/api/github/queries');
  if (!response.ok) throw new Error('Failed to fetch GitHub queries');
  return response.json();
}

export async function fetchSettings() {
  const response = await fetch('/api/settings');
  if (!response.ok) {
      if (response.status === 404) {
          // Fallback if the controller is named differently or route is different
          const altResponse = await fetch('/api/settings/config');
          if (altResponse.ok) return altResponse.json();
      }
      throw new Error('Failed to fetch settings');
  }
  return response.json();
}

export async function updateSettings(config) {
  const response = await fetch('/api/settings', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(config)
  });
  if (!response.ok) throw new Error('Failed to update settings');
  return response.json();
}

export async function fetchJiraStarred() {
  const response = await fetch('/api/jira/starred');
  if (!response.ok) throw new Error('Failed to fetch starred Jira issues');
  return response.json();
}

export async function toggleJiraStar(key) {
  const response = await fetch(`/api/jira/star?key=${encodeURIComponent(key)}`, {
    method: 'POST'
  });
  if (!response.ok) throw new Error('Failed to toggle Jira star');
  return response.json();
}

export async function fetchGitHubStarred() {
  const response = await fetch('/api/github/starred');
  if (!response.ok) throw new Error('Failed to fetch starred GitHub PRs');
  return response.json();
}

export async function toggleGitHubStar(url) {
  const response = await fetch(`/api/github/star?url=${encodeURIComponent(url)}`, {
    method: 'POST'
  });
  if (!response.ok) throw new Error('Failed to toggle GitHub star');
  return response.json();
}
