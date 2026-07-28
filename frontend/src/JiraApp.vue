<template>
  <div class="vh-100 d-flex flex-row overflow-hidden app-root">
    <Navbar />
    <div class="flex-grow-1 overflow-hidden d-flex flex-column main-wrapper">
      <div class="jira-app-container flex-grow-1">
        <!-- Sidebar -->
        <div class="jira-sidebar" :class="{ collapsed: sidebarCollapsed }" :style="sidebarStyle">
          <div class="sidebar-header d-flex align-items-center">
            <h5 v-if="!sidebarCollapsed">Jira Filters</h5>
          </div>
          
          <div class="filter-list">
            <div v-if="loadingFilters" class="sidebar-loading">
              <div class="spinner"></div>
              <span v-if="!sidebarCollapsed">Loading filters...</span>
            </div>
            <template v-else>
              <!-- Starred Filter -->
              <div class="filter-item"
                   :class="{ active: showStarredOnly }"
                   @click="selectStarred"
                   :title="sidebarCollapsed ? 'Starred Issues' : ''">
                <div class="filter-icon-wrapper starred">
                  <i class="bi bi-star-fill"></i>
                </div>
                <span v-if="!sidebarCollapsed" class="filter-name">Starred</span>
              </div>

              <!-- JQL Queries -->
              <div v-for="query in queries" :key="query.name" 
                   class="filter-item"
                   :class="{ active: currentQuery === query.jql && !showStarredOnly }"
                   @click="selectQuery(query)"
                   :title="sidebarCollapsed ? query.name : ''">
                <div class="filter-icon-wrapper">
                  <i class="bi" :class="query.icon || 'bi-filter'" :style="query.color ? { color: query.color } : {}"></i>
                </div>
                <span v-if="!sidebarCollapsed" class="filter-name text-truncate">{{ query.name }}</span>
              </div>
            </template>
          </div>

          <div class="sidebar-footer" :class="sidebarCollapsed ? 'justify-content-center' : 'justify-content-end'">
            <button class="sidebar-toggle" @click="sidebarCollapsed = !sidebarCollapsed" :title="sidebarCollapsed ? 'Expand Menu' : 'Collapse Menu'">
              <i class="bi" :class="sidebarCollapsed ? 'bi-chevron-right' : 'bi-chevron-left'"></i>
            </button>
          </div>
        </div>

        <div v-if="!sidebarCollapsed" class="sidebar-resizer" @mousedown="startSidebarResize"></div>

        <!-- Main Content -->
        <div class="main-content">
          <div class="controls-bar">
            <div>
              <h1 class="h3 fw-bold mb-0">Jira Issues</h1>
            </div>
            <div class="d-flex align-items-center gap-3">
              <div class="text-muted small" v-if="selectedQueryName">
                Filter: <span class="text-info fw-bold">{{ selectedQueryName }}</span>
              </div>
              <button class="btn btn-sm btn-outline-secondary" @click="showQueriesDialog = true" title="Edit Queries">
                <i class="bi bi-pencil-square me-1"></i> Edit Queries
              </button>
            </div>
          </div>

      <div class="jira-content-wrapper">
        <div class="d-flex h-100 overflow-hidden">
          <!-- Jira Issues List Section -->
          <div class="h-100 overflow-auto module-list-pane" :style="{ width: contentSplitWidth + '%' }">
            <JiraIssues 
              :selectedIssueKey="selectedIssue?.key" 
              :currentQuery="currentQuery"
              :showStarredOnly="showStarredOnly"
              :projects="projects"
              @select-issue="selectedIssue = $event" 
              @create-task="openCreateTaskDialog"
            />
          </div>

          <!-- Content Resizer -->
          <div class="content-resizer" @mousedown="startContentResize"></div>

          <!-- Jira Issue Detail Section -->
          <div class="h-100 overflow-auto module-detail-pane flex-grow-1">
            <JiraIssueDetail :issue="selectedIssue" />
          </div>
        </div>
      </div>
        </div>
      </div>
    </div>
    
    <JiraQueriesDialog 
      v-if="showQueriesDialog" 
      @close="showQueriesDialog = false" 
      @saved="loadFilters" 
    />

    <CreateJiraTaskDialog
      v-if="showCreateTaskDialog"
      :issue="issueToCreate"
      :projects="projects"
      @close="showCreateTaskDialog = false"
      @create="handleCreateTask"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { showToast } from './components/Toast.vue';
import Navbar from './components/Navbar.vue';
import JiraIssues from './components/JiraIssues.vue';
import JiraIssueDetail from './components/JiraIssueDetail.vue';
import JiraQueriesDialog from './components/JiraQueriesDialog.vue';
import CreateJiraTaskDialog from './components/CreateJiraTaskDialog.vue';
import { fetchJiraQueries } from './js/dashboard-api';

const selectedIssue = ref(null);
const queries = ref([]);
const currentQuery = ref('');
const showStarredOnly = ref(true);
const loadingFilters = ref(false);
const showQueriesDialog = ref(false);
const showCreateTaskDialog = ref(false);
const issueToCreate = ref(null);
const projects = ref([]);

// Sidebar state
const sidebarCollapsed = ref(localStorage.getItem('jiraSidebarCollapsed') === 'true');
const sidebarWidth = ref(parseInt(localStorage.getItem('jiraSidebarWidth')) || 230);
const contentSplitWidth = ref(parseInt(localStorage.getItem('jiraContentSplitWidth')) || 50);

const sidebarStyle = computed(() => ({
  width: sidebarCollapsed.value ? '50px' : `${sidebarWidth.value}px`
}));

const selectedQueryName = computed(() => {
  if (showStarredOnly.value) return 'Starred';
  const query = queries.value.find(q => q.jql === currentQuery.value);
  return query ? query.name : '';
});

const selectStarred = () => {
  showStarredOnly.value = true;
};

const selectQuery = (query) => {
  showStarredOnly.value = false;
  currentQuery.value = query.jql;
};

const loadFilters = async () => {
  loadingFilters.value = true;
  try {
    const [queriesData, projectsData] = await Promise.all([
      fetchJiraQueries(),
      fetch('/api/projects').then(res => res.json())
    ]);
    queries.value = queriesData;
    projects.value = projectsData;
    if (queries.value.length > 0 && !currentQuery.value) {
      currentQuery.value = queries.value[0].jql;
    }
  } catch (e) {
    console.error('Error loading Jira filters or projects:', e);
  } finally {
    loadingFilters.value = false;
  }
};

const openCreateTaskDialog = (issue) => {
    issueToCreate.value = issue;
    showCreateTaskDialog.value = true;
};

const handleCreateTask = async ({ issue, targetListId }) => {
    try {
        const response = await fetch(`/api/tasks/from-jira?key=${encodeURIComponent(issue.key)}&summary=${encodeURIComponent(issue.summary)}&link=${encodeURIComponent(issue.url)}&listId=${targetListId}`, {
            method: 'POST'
        });

        if (response.ok) {
            showCreateTaskDialog.value = false;
            showToast('Task created successfully!', 'success');
        } else {
            const error = await response.text();
            showToast(`Failed to create task: ${error}`, 'error');
        }
    } catch (error) {
        console.error('Error creating task:', error);
        showToast('Error creating task', 'error');
    }
};

// Resize logic
const startSidebarResize = (e) => {
  e.preventDefault();
  const startX = e.clientX;
  const startWidth = sidebarWidth.value;

  const onMouseMove = (moveEvent) => {
    const delta = moveEvent.clientX - startX;
    const newWidth = Math.max(150, Math.min(500, startWidth + delta));
    sidebarWidth.value = newWidth;
  };

  const onMouseUp = () => {
    localStorage.setItem('jiraSidebarWidth', sidebarWidth.value);
    document.removeEventListener('mousemove', onMouseMove);
    document.removeEventListener('mouseup', onMouseUp);
  };

  document.addEventListener('mousemove', onMouseMove);
  document.addEventListener('mouseup', onMouseUp);
};

// Content Resize logic
const startContentResize = (e) => {
  e.preventDefault();
  const container = document.querySelector('.jira-content-wrapper');
  if (!container) return;
  
  const onMouseMove = (moveEvent) => {
    const containerRect = container.getBoundingClientRect();
    const newPercent = ((moveEvent.clientX - containerRect.left) / containerRect.width) * 100;
    contentSplitWidth.value = Math.max(20, Math.min(80, newPercent));
  };

  const onMouseUp = () => {
    localStorage.setItem('jiraContentSplitWidth', contentSplitWidth.value);
    document.removeEventListener('mousemove', onMouseMove);
    document.removeEventListener('mouseup', onMouseUp);
  };

  document.addEventListener('mousemove', onMouseMove);
  document.addEventListener('mouseup', onMouseUp);
};

watch(sidebarCollapsed, (val) => {
  localStorage.setItem('jiraSidebarCollapsed', val);
});

onMounted(() => {
  loadFilters();
});
</script>

<style scoped>
.app-root {
  background-color: var(--bg-darker);
}

.main-wrapper {
  background-color: var(--bg-darker);
}

.jira-app-container {
  display: flex;
  height: 100%;
}

.jira-sidebar {
  background-color: var(--bg-dark);
  border-right: 1px solid var(--border-primary);
  display: flex;
  flex-direction: column;
  transition: width 0.2s ease;
  flex-shrink: 0;
}

.sidebar-header {
  /* height and other properties moved to global.css */
}

.sidebar-header h5 {
  white-space: nowrap;
}

.filter-list {
  flex-grow: 1;
  overflow-y: auto;
  padding: 0.5rem;
}

.filter-item {
  display: flex;
  align-items: center;
  padding: 0.6rem 0.8rem;
  border-radius: 6px;
  cursor: pointer;
  margin-bottom: 2px;
  color: var(--text-muted);
  transition: all 0.2s;
}

.filter-item:hover {
  background-color: rgba(255, 255, 255, 0.05);
  color: var(--text-primary);
}

.filter-item.active {
  background-color: rgba(88, 166, 255, 0.1);
  color: var(--accent-blue);
}

.filter-icon-wrapper {
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 12px;
  font-size: 1.1rem;
  border-radius: 4px;
}

.filter-icon-wrapper.starred {
  color: var(--accent-yellow);
}

.jira-sidebar.collapsed .filter-item {
  justify-content: center;
  padding: 0.8rem 0;
}

.jira-sidebar.collapsed .filter-icon-wrapper {
  margin-right: 0;
}

.sidebar-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent !important;
  border: none !important;
  color: var(--text-muted);
  font-size: 1.2rem;
  padding: 8px;
  box-shadow: none !important;
}

.sidebar-toggle:hover {
  color: var(--text-primary);
}

.jira-content-wrapper {
  flex-grow: 1;
  overflow: hidden;
}

.module-list-pane {
  background-color: var(--bg-dark);
}

.module-detail-pane {
  background-color: var(--bg-darker);
}

/* Single separator for Jira issues list and details */
:deep(.content-resizer) {
  width: 4px;
  background-color: transparent;
  border: none;
  margin-left: -2px;
  margin-right: -2px;
  position: relative;
}

:deep(.content-resizer::after) {
  content: "";
  position: absolute;
  left: 50%;
  top: 0;
  bottom: 0;
  width: 1px;
  background-color: var(--border-primary);
  transform: translateX(-50%);
  transition: background-color 0.2s;
}

:deep(.content-resizer:hover::after), :deep(.content-resizer:active::after) {
  background-color: var(--accent-blue);
  width: 2px;
}

.sidebar-loading {
  display: flex;
  align-items: center;
  padding: 1rem;
  color: var(--text-muted);
  gap: 10px;
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.1);
  border-top-color: var(--accent-blue);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
