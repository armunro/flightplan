export async function fetchEvents(calendarId = null, start = null, end = null) {
  let url = '/api/calendar';
  const params = new URLSearchParams();
  if (calendarId) params.append('calendarId', calendarId);
  if (start) params.append('start', start.toISOString());
  if (end) params.append('end', end.toISOString());
  
  const queryString = params.toString();
  if (queryString) url += `?${queryString}`;

  const response = await fetch(url);
  if (!response.ok) throw new Error('Failed to fetch calendar events');
  return response.json();
}

export async function fetchCalendars() {
  const response = await fetch('/api/calendar/folders');
  if (!response.ok) throw new Error('Failed to fetch calendars');
  return response.json();
}

export async function fetchCalendarPreferences() {
  const response = await fetch('/api/calendar/preferences');
  if (!response.ok) throw new Error('Failed to fetch calendar preferences');
  return response.json();
}

export async function saveCalendarPreferences(preferences) {
  const response = await fetch('/api/calendar/preferences', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(preferences)
  });
  if (!response.ok) throw new Error('Failed to save calendar preferences');
}

export async function addCalendarEvent(event) {
  const response = await fetch('/api/calendar', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(event)
  });
  if (!response.ok) throw new Error('Failed to add calendar event');
  return response.json();
}

export async function deleteCalendarEvent(eventId, calendarId = null) {
  let url = `/api/calendar/${eventId}`;
  if (calendarId) {
    url += `?calendarId=${encodeURIComponent(calendarId)}`;
  }
  const response = await fetch(url, {
    method: 'DELETE'
  });
  if (!response.ok) throw new Error('Failed to delete calendar event');
}

export async function updateCalendarEvent(eventId, event, calendarId = null) {
  let url = `/api/calendar/${eventId}`;
  if (calendarId) {
    url += `?calendarId=${encodeURIComponent(calendarId)}`;
  }
  const response = await fetch(url, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(event)
  });
  if (!response.ok) throw new Error('Failed to update calendar event');
  return response.json();
}
