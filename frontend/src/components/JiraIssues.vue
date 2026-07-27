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
      <div v-else class="list-group list-group-flush">
        <div v-for="issue in issues" :key="issue.key" 
             class="list-group-item list-group-item-action bg-dark text-light border-secondary d-flex justify-content-between align-items-center jira-issue-item"
             :class="{ selected: selectedIssueKey === issue.key }"
             @contextmenu.prevent="onMenu($event, issue)"
             @click="selectIssue(issue)"
             style="cursor: pointer;">
          <div class="me-2 star-container" @click.stop="toggleStar(issue)">
            <i :class="starredKeys.has(issue.key) ? 'bi bi-star-fill text-warning' : 'bi bi-star text-muted'"></i>
          </div>
          <div class="flex-grow-1">
            <div class="d-flex w-100 justify-content-between align-items-center">
              <div class="d-flex align-items-center flex-grow-1 overflow-hidden">
                <h6 class="mb-1 text-info me-2 fw-bold text-nowrap fs-base">{{ issue.key }}</h6>
                <h7 class="mb-1 text-light text-truncate fs-base">{{ issue.summary }}</h7>
              </div>
              <small class="badge bg-secondary text-light fw-bold fs-xs" :style="{ backgroundColor: getStatusColor(issue.status) + ' !important' }">{{ issue.status }}</small>
            </div>
            <div class="d-flex w-100 justify-content-between align-items-center mt-1">
              <small class="text-secondary fs-xs">Priority: <span :style="{ color: getPriorityColor(issue.priority) }">{{ issue.priority }}</span></small>
              <small class="text-secondary fs-xs">
                <i class="bi bi-person"></i> {{ issue.assignee || 'Unassigned' }}
              </small>
            </div>
          </div>
          
          <div class="issue-actions-dropdown">
            <button class="issue-actions-btn" @click.stop="onMenu($event, issue)" title="Issue Actions">...</button>
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
import { ref, onMounted, onUnmounted, watch } from 'vue';
import { showToast } from './Toast.vue';
import { fetchJiraIssues, fetchJiraQueries, fetchJiraStarred, toggleJiraStar } from '../js/dashboard-api';
import { unassignJiraIssue } from '../js/tasks-api';

const props = defineProps({
  selectedIssueKey: {
    type: String,
    default: null
  },
  currentQuery: {
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
  }
});

const emit = defineEmits(['select-issue', 'create-task']);

const issues = ref([]);
const starredKeys = ref(new Set());
const loading = ref(true);

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
  const s = status.toLowerCase();
  if (s.includes('done') || s.includes('closed') || s.includes('resolved')) return '#3fb950';
  if (s.includes('progress')) return '#58a6ff';
  if (s.includes('todo') || s.includes('backlog')) return '#aab2bb';
  return '#bc8cff';
};

const getPriorityColor = (priority) => {
  const p = priority.toLowerCase();
  if (p.includes('highest') || p.includes('critical')) return '#f85149';
  if (p.includes('high')) return '#f0883e';
  if (p.includes('medium')) return '#ffa500';
  if (p.includes('low')) return '#3fb950';
  return '#aab2bb';
};

onMounted(() => {
  loadInitialData();
});

onUnmounted(() => {
  document.removeEventListener('click', closeMenu);
});
</script>

<style scoped>
.jira-issue-item {
  padding: 12px 16px;
  cursor: pointer;
  border-bottom: 1px solid var(--border-primary);
  transition: background-color 0.15s ease;
  background-color: var(--bg-dark);
  color: var(--text-primary);
}

.jira-issue-item.selected {
    background-color: rgba(88, 166, 255, 0.1);
    border-left: 4px solid var(--accent-blue);
}

.jira-issue-item:hover {
  background-color: var(--bg-card);
}

.star-container {
    cursor: pointer;
    z-index: 10;
    padding: 5px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.star-container:hover .bi-star {
    color: var(--text-primary);
}

.issue-actions-dropdown {
    position: relative;
    margin-left: 10px;
}

.issue-actions-btn {
    background: var(--bg-card);
    border: 1px solid var(--border-primary);
    border-radius: 4px;
    color: var(--text-muted);
    cursor: pointer;
    padding: 2px 8px;
    font-size: 1.2rem;
    line-height: 1;
    transition: all 0.2s;
    display: flex;
    align-items: center;
    justify-content: center;
    height: 28px;
    width: 32px;
    font-weight: bold;
}

.issue-actions-btn:hover {
    color: var(--text-primary);
    background-color: var(--border-primary);
    border-color: var(--text-muted);
}

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
