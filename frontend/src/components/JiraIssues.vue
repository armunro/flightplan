<template>
  <div class="card border-0 rounded-0 h-100 bg-transparent">
    <div class="card-body p-0 h-100">
      <div v-if="loading" class="p-4 text-center">
        <div class="spinner-border text-info mb-3" role="status"></div>
        <p class="text-muted">Loading Jira issues...</p>
      </div>
      <div v-else-if="issues.length === 0" class="p-5 text-center text-muted">
        <i class="bi bi-search display-4 mb-3 opacity-25"></i>
        <p v-if="showStarredOnly">No starred issues found. Star issues to see them here.</p>
        <p v-else>No active issues found or API token not configured.</p>
      </div>
      <div v-else-if="filteredIssues.length === 0" class="p-5 text-center text-muted">
        <i class="bi bi-search display-4 mb-3 opacity-25"></i>
        <p>No issues match your search "{{ searchQuery }}".</p>
      </div>
      <div v-else class="jira-issues-list theme-bg-dark">
        <div class="jira-list-header theme-bg-dark theme-border">
          <div class="col-key">Key / Status</div>
          <div class="col-summary">Summary</div>
          <div class="col-status">Type / Priority</div>
          <div class="col-assignee">Assignee / Date</div>
          <div class="issue-actions-spacer"></div>
        </div>
        <div v-for="issue in filteredIssues" :key="issue.key" 
             class="jira-issue-row"
             :class="{ selected: selectedIssueKey === issue.key }"
             @contextmenu.prevent="onMenu($event, issue)"
             @click="selectIssue(issue)">
          <div class="jira-issue-main-row">
            <div class="col-key">
              <div class="d-flex align-items-center mb-1">
                <div class="star-container me-2" @click.stop="toggleStar(issue)">
                  <i :class="starredKeys.has(issue.key) ? 'bi bi-star-fill text-warning' : 'bi bi-star text-muted'"></i>
                </div>
                <span class="text-info fw-bold fs-xs">{{ issue.key }}</span>
              </div>
              <div class="d-flex align-items-center gap-2 ps-4 ms-1">
                <span class="status-badge" :style="{ color: getStatusColor(issue.status), backgroundColor: getStatusColor(issue.status) + '15' }">{{ issue.status }}</span>
              </div>
            </div>
            
            <div class="col-summary">
              <span class="theme-text truncate-summary fw-bold mb-1">{{ issue.summary }}</span>
              <div class="col-description">
                <span v-if="issue.description" class="theme-text-muted fs-xs truncate-description">{{ formatPlainTextDescription(issue.description) }}</span>
                <span v-else class="theme-text-muted fs-xs italic">No description</span>
              </div>
            </div>

            <div class="col-status">
              <div class="d-flex flex-column gap-1 align-items-end">
                <span v-if="issue.issueType" class="type-badge" :style="{ color: getTypeColor(issue.issueType), backgroundColor: getTypeColor(issue.issueType) + '15' }">{{ issue.issueType }}</span>
                <span :style="{ color: getPriorityColor(issue.priority) }" class="fs-xxs fw-bold uppercase-text">{{ issue.priority }}</span>
              </div>
            </div>

            <div class="col-assignee">
              <div class="d-flex flex-column align-items-end text-end">
                <span class="theme-text-muted fs-xs text-nowrap mb-1"><i class="bi bi-person me-1"></i> {{ issue.assignee || 'Unassigned' }}</span>
                <span v-if="issue.updated" class="theme-text-muted fs-xxs">Upd. {{ formatFriendlyDate(issue.updated, false, true) }}</span>
                <span v-else-if="issue.created" class="theme-text-muted fs-xxs">Cre. {{ formatFriendlyDate(issue.created, false, true) }}</span>
              </div>
            </div>

            <div class="issue-actions">
              <button class="issue-actions-btn" @click.stop="onMenu($event, issue)">...</button>
            </div>
          </div>
        </div>
      </div>
      

      <teleport to="body">
        <div v-if="menu.visible" 
             class="context-menu" 
             :style="{ top: menu.y + 'px', left: menu.x + 'px' }"
             @click.stop
             @contextmenu.prevent>
          <div class="context-menu-item has-submenu">
            Copy items
            <div class="submenu">
              <div class="context-menu-item" @click="copyUrl">Copy URL</div>
              <div class="context-menu-item" @click="copyKey">Copy Key</div>
              <div class="context-menu-item" @click="copySummary">Copy Summary</div>
              <div class="context-menu-item" @click="copyFormat">[KEY] - Summary</div>
            </div>
          </div>
          <div class="context-menu-item" @click="createTask(menu.issue)">Create Task</div>
          <div v-if="menu.issue && menu.issue.assignee" class="context-menu-item" @click="unassignIssue(menu.issue)">Unassign</div>
        </div>
      </teleport>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue';
import { showToast } from './Toast.vue';
import { fetchJiraIssues, fetchJiraQueries, fetchJiraStarred, toggleJiraStar, fetchJiraIssue } from '../js/dashboard-api';
import { unassignJiraIssue } from '../js/tasks-api';

import { formatFriendlyDate } from '../js/utils';

const props = defineProps({
  selectedIssueKey: {
    type: String,
    default: null
  },
  currentQuery: {
    type: String,
    default: ''
  },
  searchQuery: {
    type: String,
    default: ''
  },
  showStarredOnly: {
    type: Boolean,
    default: true
  },
  projects: {
    type: Array,
    default: () => []
  },
  colorSchemes: {
    type: Array,
    default: () => []
  }
});

const emit = defineEmits(['select-issue', 'create-task', 'fetched-issue']);

const issues = ref([]);
const fetchedIssue = ref(null);
const starredKeys = ref(new Set());
const loading = ref(true);

const filteredIssues = computed(() => {
  const allIssues = [...issues.value];
  
  if (fetchedIssue.value && !issues.value.some(i => i.key === fetchedIssue.value.key)) {
    allIssues.push(fetchedIssue.value);
  }

  if (!props.searchQuery) return allIssues;
  
  const query = props.searchQuery.toLowerCase();
  return allIssues.filter(issue => {
    return (
      (issue.key && issue.key.toLowerCase().includes(query)) ||
      (issue.summary && issue.summary.toLowerCase().includes(query)) ||
      (issue.assignee && issue.assignee.toLowerCase().includes(query)) ||
      (issue.status && issue.status.toLowerCase().includes(query)) ||
      (issue.priority && issue.priority.toLowerCase().includes(query))
    );
  });
});

const menu = ref({
  visible: false,
  x: 0,
  y: 0,
  issue: null
});

const onMenu = (e, issue) => {
  e.preventDefault();
  
  menu.value = {
    visible: true,
    x: e.pageX,
    y: e.pageY,
    issue: issue
  };
  
  setTimeout(() => {
    document.addEventListener('click', closeMenu);
  }, 0);
};

const closeMenu = () => {
  menu.value.visible = false;
  document.removeEventListener('click', closeMenu);
};

const copyToClipboard = async (text, html) => {
  try {
    if (html && window.ClipboardItem) {
      // Modern browsers handle CF_HTML wrapping automatically. 
      // We should just provide the HTML document/fragment.
      const fullHtml = `<!DOCTYPE html><html><head><meta charset="utf-8"></head><body>${html}</body></html>`;
      
      const plainText = new Blob([text], { type: 'text/plain' });
      const htmlText = new Blob([fullHtml], { type: 'text/html' });
      const clipboardItem = new ClipboardItem({
        'text/plain': plainText,
        'text/html': htmlText
      });
      await navigator.clipboard.write([clipboardItem]);
      console.log('Successfully copied rich text to clipboard');
    } else {
      await navigator.clipboard.writeText(text);
      console.log('Successfully copied plain text to clipboard');
    }
  } catch (err) {
    console.error('Failed to copy: ', err);
    showToast('Failed to copy rich text. Falling back to plain text. Error: ' + err.message, 'warning');
    // Fallback to plain text if rich copy fails (e.g. permission issues in some environments)
    try {
      await navigator.clipboard.writeText(text);
    } catch (fallbackErr) {
      console.error('Fallback copy also failed: ', fallbackErr);
    }
  }
};

const copyUrl = () => {
  if (menu.value.issue) {
    const url = menu.value.issue.url;
    copyToClipboard(url, `<a href="${url}">${url}</a>`);
  }
  closeMenu();
};

const copyKey = () => {
  if (menu.value.issue) {
    const key = menu.value.issue.key;
    const url = menu.value.issue.url;
    copyToClipboard(key, `<a href="${url}">${key}</a>`);
  }
  closeMenu();
};

const copySummary = () => {
  if (menu.value.issue) {
    const summary = menu.value.issue.summary;
    const url = menu.value.issue.url;
    copyToClipboard(summary, `<a href="${url}">${summary}</a>`);
  }
  closeMenu();
};

const copyFormat = () => {
  if (menu.value.issue) {
    const text = `[${menu.value.issue.key}] - ${menu.value.issue.summary}`;
    const url = menu.value.issue.url;
    copyToClipboard(text, `<a href="${url}">${text}</a>`);
  }
  closeMenu();
};

const toggleStar = async (issue) => {
  try {
    const result = await toggleJiraStar(issue.key);
    if (result.isStarred) {
      starredKeys.value.add(issue.key);
    } else {
      starredKeys.value.delete(issue.key);
      if (props.showStarredOnly) {
        issues.value = issues.value.filter(i => i.key !== issue.key);
      }
    }
  } catch (e) {
    console.error('Error toggling star:', e);
  }
};


const loadInitialData = async () => {
  loading.value = true;
  try {
    const [starredData] = await Promise.all([
      fetchJiraStarred()
    ]);
    
    starredKeys.value = new Set(starredData);
    
    await loadIssues();
  } catch (e) {
    console.error('Error loading initial data:', e);
  } finally {
    loading.value = false;
  }
};

const loadIssues = async () => {
  loading.value = true;
  try {
    let jql = props.currentQuery;
    if (props.showStarredOnly) {
      if (starredKeys.value.size === 0) {
        issues.value = [];
        return;
      }
      const keys = Array.from(starredKeys.value).map(k => `"${k}"`).join(',');
      jql = `issueKey in (${keys})`;
    }
    
    if (!jql && !props.showStarredOnly) {
      issues.value = [];
      return;
    }
    
    issues.value = await fetchJiraIssues(jql);
  } catch (e) {
    console.error('Error loading issues:', e);
  } finally {
    loading.value = false;
  }
};

watch(() => props.currentQuery, () => {
  loadIssues();
});

watch(() => props.showStarredOnly, () => {
  loadIssues();
});

watch(() => props.searchQuery, async (newQuery) => {
  if (!newQuery) {
    fetchedIssue.value = null;
    emit('fetched-issue', null);
    return;
  }

  const query = newQuery.trim().toUpperCase();
  const jiraKeyRegex = /^[A-Z0-9]+-[0-9]+$/;
  
  if (jiraKeyRegex.test(query)) {
    // If it's already in the list, don't fetch
    if (issues.value.some(i => i.key.toUpperCase() === query)) {
      fetchedIssue.value = null;
      emit('fetched-issue', null);
      return;
    }

    try {
      const issue = await fetchJiraIssue(query);
      if (issue) {
        fetchedIssue.value = issue;
        emit('fetched-issue', issue);
      } else {
        fetchedIssue.value = null;
        emit('fetched-issue', null);
      }
    } catch (e) {
      console.error('Error fetching single Jira issue:', e);
      fetchedIssue.value = null;
      emit('fetched-issue', null);
    }
  } else {
    fetchedIssue.value = null;
    emit('fetched-issue', null);
  }
});

const selectIssue = (issue) => {
  emit('select-issue', issue);
};

const createTask = (issue) => {
  if (!issue) return;
  emit('create-task', issue);
  closeMenu();
};

const unassignIssue = async (issue) => {
  if (!issue) return;
  if (!confirm(`Are you sure you want to unassign ${issue.key}?`)) return;
  
  closeMenu();
  try {
    const success = await unassignJiraIssue(issue.key);
    if (success) {
      loadIssues();
    } else {
      showToast('Failed to unassign issue', 'error');
    }
  } catch (error) {
    console.error('Error unassigning issue:', error);
    showToast('Error unassigning issue', 'error');
  }
};

const getStatusColor = (status) => {
  if (!status) return '#bc8cff';
  
  const trimmedStatus = status.toString().trim();
  
  // Custom palette check
  const palette = (props.colorSchemes || []).find(s => {
    const name = (s.name || s.Name)?.toString().trim().toLowerCase();
    return name === 'system-jira-status' || name === 'jira-status';
  });

  if (palette) {
    const colors = palette.colors || palette.Colors;
    if (colors) {
      const customColor = colors.find(c => {
        if (!c) return false;
        const cName = (c.name || c.Name);
        if (!cName) return false;
        const match = cName.toString().trim().toLowerCase() === trimmedStatus.toLowerCase();
        return match;
      });
      if (customColor) {
        return (customColor.color || customColor.Color);
      }
    }
  }

  const s = trimmedStatus.toLowerCase();
  if (s.includes('done') || s.includes('closed') || s.includes('resolved')) return '#3fb950';
  if (s.includes('progress')) return '#58a6ff';
  if (s.includes('todo') || s.includes('backlog')) return '#aab2bb';
  return '#bc8cff';
};

const getPriorityColor = (priority) => {
  if (!priority) return '#aab2bb';

  const trimmedPriority = priority.toString().trim();
  
  // Custom palette check
  const palette = (props.colorSchemes || []).find(s => {
    const name = (s.name || s.Name)?.toString().trim().toLowerCase();
    return name === 'system-jira-priority' || name === 'jira-priority';
  });

  if (palette) {
    const colors = palette.colors || palette.Colors;
    if (colors) {
      const customColor = colors.find(c => {
        if (!c) return false;
        const cName = (c.name || c.Name);
        if (!cName) return false;
        const match = cName.toString().trim().toLowerCase() === trimmedPriority.toLowerCase();
        return match;
      });
      if (customColor) {
        return (customColor.color || customColor.Color);
      }
    }
  }

  const p = trimmedPriority.toLowerCase();
  if (p.includes('highest') || p.includes('critical')) return '#f85149';
  if (p.includes('high')) return '#f0883e';
  if (p.includes('medium')) return '#ffa500';
  if (p.includes('low')) return '#3fb950';
  return '#aab2bb';
};

const getTypeColor = (type) => {
  if (!type) return '#aab2bb';

  const trimmedType = type.toString().trim();
  
  // Custom palette check
  const palette = (props.colorSchemes || []).find(s => {
    const name = (s.name || s.Name)?.toString().trim().toLowerCase();
    return name === 'system-jira-type' || name === 'jira-type';
  });

  if (palette) {
    const colors = palette.colors || palette.Colors;
    if (colors) {
      const customColor = colors.find(c => {
        if (!c) return false;
        const cName = (c.name || c.Name);
        if (!cName) return false;
        const match = cName.toString().trim().toLowerCase() === trimmedType.toLowerCase();
        return match;
      });
      if (customColor) {
        return (customColor.color || customColor.Color);
      }
    }
  }

  return '#bc8cff';
};

const formatPriorityColor = (priority) => {
  return getPriorityColor(priority);
};

const formatPlainTextDescription = (description) => {
  if (!description) return '';
  
  if (typeof description === 'string' && description.startsWith('{')) {
    try {
      const adf = JSON.parse(description);
      return extractTextFromADF(adf);
    } catch (e) {
      return description;
    }
  }
  return description;
};

const extractTextFromADF = (node) => {
  if (!node) return '';
  
  if (node.type === 'text') {
    return node.text || '';
  }
  
  let text = '';
  if (node.content) {
    node.content.forEach(child => {
      text += extractTextFromADF(child);
      if (['paragraph', 'heading', 'listItem'].includes(child.type)) {
        text += ' ';
      }
    });
  }
  
  return text.trim();
};

onMounted(() => {
  loadInitialData();
});

onUnmounted(() => {
  document.removeEventListener('click', closeMenu);
});
</script>

<style scoped>
.jira-issues-list {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.jira-list-header {
  display: flex;
  align-items: center;
  padding: 8px 12px;
  border-bottom: 1px solid var(--border-primary);
  font-size: var(--fs-xxs);
  text-transform: uppercase;
  font-weight: 700;
  color: var(--text-muted);
  position: sticky;
  top: 0;
  z-index: 10;
}

.jira-issue-row:hover {
  background-color: var(--bg-hover, rgba(255, 255, 255, 0.03));
}

.jira-issue-row.selected {
  background-color: var(--bg-selected, rgba(88, 166, 255, 0.1));
}

/* Column Widths */
.col-key { width: 120px; flex-shrink: 0; padding-right: 12px; }
.col-summary { flex-grow: 1; min-width: 200px; padding-right: 12px; overflow: hidden; }
.col-status { width: 60px; flex-shrink: 0; padding-right: 4px; text-align: right; }
.col-assignee { width: 140px; flex-shrink: 0; text-align: right; }
.issue-actions-spacer { width: 38px; flex-shrink: 0; }

.jira-issue-row {
  display: flex;
  flex-direction: column;
  border-bottom: 1px solid var(--border-primary);
  cursor: pointer;
  transition: background-color 0.2s;
  font-size: var(--fs-sm);
  min-height: 60px;
}

.jira-issue-main-row {
  display: flex;
  align-items: center;
  width: 100%;
  padding: 10px 12px;
}

.col-description {
  overflow: hidden;
}

.truncate-summary {
  display: block;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.uppercase-text {
  text-transform: uppercase;
}

.truncate-description {
  display: block;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 100%;
}

.type-badge {
  font-size: 0.65rem;
  line-height: 1;
  padding: 2px 6px;
  border: 1px solid currentColor;
  border-radius: 4px;
  background-color: var(--bg-badge, rgba(255, 255, 255, 0.05));
  display: inline-block;
  white-space: nowrap;
  font-weight: 600;
}

.status-badge {
  font-size: 0.65rem;
  line-height: 1;
  padding: 2px 6px;
  border: 1px solid currentColor;
  border-radius: 4px;
  background-color: var(--bg-badge, rgba(255, 255, 255, 0.05));
  white-space: nowrap;
  display: inline-block;
  font-weight: 600;
}

.issue-actions {
  width: 38px;
  flex-shrink: 0;
  display: flex;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.2s;
}

.jira-issue-row:hover .issue-actions {
  opacity: 1;
}

.issue-actions-btn {
  background: transparent;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  font-size: 1.2rem;
  line-height: 1;
  padding: 0 5px;
  border-radius: 4px;
}

.issue-actions-btn:hover {
  color: var(--text-primary);
  background-color: var(--bg-hover, rgba(255, 255, 255, 0.1));
}

.star-container {
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
}

/* Context Menu */
.context-menu {
    position: fixed;
    background: var(--bg-card);
    border: 1px solid var(--border-primary);
    border-radius: 6px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5);
    z-index: 100000;
    min-width: 150px;
    padding: 4px 0;
}

.context-menu-item {
    padding: 8px 16px;
    cursor: pointer;
    font-size: 0.9em;
    color: var(--text-primary);
    transition: background 0.2s;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.context-menu-item:hover {
    background: var(--border-primary);
}

.has-submenu {
    position: relative;
}

.has-submenu::after {
    content: '▶';
    font-size: 0.7em;
    margin-left: 10px;
    opacity: 0.5;
}

.submenu {
    display: none;
    position: absolute;
    left: 100%;
    top: -4px;
    background: var(--bg-card);
    border: 1px solid var(--border-primary);
    border-radius: 6px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5);
    min-width: 150px;
    padding: 4px 0;
}

.has-submenu:hover > .submenu {
    display: block;
}
</style>
