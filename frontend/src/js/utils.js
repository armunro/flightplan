export const formatForInput = (dateStr) => {
  if (!dateStr) return null;
  try {
    const date = new Date(dateStr);
    if (isNaN(date.getTime())) return null;
    const tzoffset = date.getTimezoneOffset() * 60000;
    const localISOTime = (new Date(date - tzoffset)).toISOString().slice(0, 16);
    return localISOTime;
  } catch (e) {
    console.error('Error formatting date for input:', e);
    return null;
  }
};

export const formatToISO = (dateStr) => {
  if (!dateStr) return null;
  try {
    const date = new Date(dateStr);
    if (isNaN(date.getTime())) return null;
    return date.toISOString();
  } catch (e) {
    console.error('Error formatting date to ISO:', e);
    return null;
  }
};

export function findTaskInProjects(projects, taskId) {
  for (const project of projects) {
    for (const list of project.lists) {
      const task = findTaskInList(list.tasks, taskId);
      if (task) return { task, project };
    }
  }
  return null;
}

export function getDateColorClass(dateStr, isCompleted) {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  if (isNaN(date.getTime())) return '';

  const now = new Date();
  
  // Strip time for date-only comparisons
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const compareDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());
  
  const diffTime = compareDate - today;
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

  if (isCompleted) {
    if (diffTime < 0) return 'date-muted';
    // Fall through to other colors if it's today/this week even if completed? 
    // Usually "closed" takes precedence for past dates.
  }

  if (diffTime < 0) {
    return isCompleted ? 'date-muted' : 'date-overdue';
  }

  if (diffDays === 0) {
    return 'date-today';
  }

  if (diffDays > 0 && diffDays <= 7) {
    return 'date-this-week';
  }

  return '';
}

export function formatFriendlyDate(dateStr, includeYmd = true, relativeOnly = false) {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  if (isNaN(date.getTime())) return '';

  const yyyymmdd = date.toISOString().split('T')[0];
  const now = new Date();
  const diffMs = now - date;
  const absDiffMs = Math.abs(diffMs);
  const isPast = diffMs > 0;

  const minutes = Math.floor(absDiffMs / 60000);
  const hours = Math.floor(minutes / 60);
  const days = Math.floor(hours / 24);
  const months = Math.floor(days / 30);

  let relative = '';
  if (months > 0) {
    const remainingDays = days % 30;
    relative = `${months}mo${remainingDays > 0 ? ' ' + remainingDays + 'd' : ''}`;
  }
  else if (days > 0) {
    const remainingHours = hours % 24;
    relative = `${days}d${remainingHours > 0 ? ' ' + remainingHours + 'hr' : ''}`;
  }
  else if (hours > 0) {
    const remainingMinutes = minutes % 60;
    relative = `${hours}hr${remainingMinutes > 0 ? ' ' + remainingMinutes + 'm' : ''}`;
  }
  else relative = `${minutes}m`;

  const text = isPast ? `${relative} ago` : `in ${relative}`;
  if (relativeOnly) {
    return text;
  }

  if (includeYmd) {
    return `${yyyymmdd} (${text})`;
  } else {
    return `(${text})`;
  }
}

export function formatEstimate(minutes) {
  if (!minutes || minutes <= 0) return '0m';
  
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  
  let result = '';
  if (h > 0) result += `${h}h `;
  if (m > 0 || h === 0) result += `${m}m`;
  
  return result.trim();
}

export function parseEstimate(input) {
  if (!input) return 0;
  if (typeof input === 'number') return input;
  
  const str = input.toString().toLowerCase().trim();
  if (!str) return 0;

  // If it's just a number, assume minutes
  if (/^\d+$/.test(str)) {
    return parseInt(str, 10);
  }

  let totalMinutes = 0;
  
  // Match hours (e.g., 1h, 1.5h)
  const hMatch = str.match(/(\d+(\.\d+)?)h/);
  if (hMatch) {
    totalMinutes += parseFloat(hMatch[1]) * 60;
  }
  
  // Match minutes (e.g., 30m, 5m)
  const mMatch = str.match(/(\d+)m/);
  if (mMatch) {
    totalMinutes += parseInt(mMatch[1], 10);
  }

  // If we couldn't parse anything with h or m, try parsing as float (for cases like "1.5")
  if (totalMinutes === 0 && !hMatch && !mMatch) {
    const val = parseFloat(str);
    return isNaN(val) ? 0 : Math.round(val);
  }

  return Math.round(totalMinutes);
}

function findTaskInList(tasks, taskId) {
  for (const task of tasks) {
    if (task.id === taskId) return task;
    if (task.subtasks) {
      const found = findTaskInList(task.subtasks, taskId);
      if (found) return found;
    }
  }
  return null;
}
// trigger build
