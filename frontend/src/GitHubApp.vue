<template>
  <div class="vh-100 d-flex flex-row overflow-hidden app-root">
    <Navbar />
    <div class="flex-grow-1 overflow-hidden d-flex flex-column main-wrapper">
      <div class="github-app-container flex-grow-1">
        <!-- Sidebar -->
        <div class="github-sidebar" :class="{ collapsed: sidebarCollapsed }" :style="sidebarStyle">
          <div class="sidebar-header d-flex align-items-center">
            <h5 v-if="!sidebarCollapsed">GitHub Filters</h5>
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
                   :title="sidebarCollapsed ? 'Starred PRs' : ''">
                <div class="filter-icon-wrapper">
                  <i class="bi bi-star-fill text-warning"></i>
                </div>
                <span v-if="!sidebarCollapsed" class="filter-name">Starred</span>
              </div>

              <!-- Mine Filter -->
              <div class="filter-item"
                   :class="{ active: currentQuery === null && !showStarredOnly }"
                   @click="selectQuery(null)"
                   :title="sidebarCollapsed ? 'My Open PRs' : ''">
                <div class="filter-icon-wrapper">
                  <i class="bi bi-person-fill"></i>
                </div>
                <span v-if="!sidebarCollapsed" class="filter-name">Mine</span>
              </div>

              <!-- Queries -->
              <div v-for="query in queries" :key="query.name" 
                   class="filter-item"
                   :class="{ active: currentQuery === query.query && !showStarredOnly }"
                   @click="selectQuery(query.query)"
                   :title="sidebarCollapsed ? query.name : ''">
                <div class="filter-icon-wrapper">
                  <i class="bi bi-filter"></i>
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
              <h1 class="h3 fw-bold mb-0">GitHub Pull Requests</h1>
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

      <div class="github-content-wrapper">
        <div class="d-flex h-100 overflow-hidden">
          <!-- GitHub PRs List Section -->
          <div class="h-100 overflow-auto module-list-pane" :style="{ width: contentSplitWidth + '%' }">
            <GitHubPrs 
              :selectedPrUrl="selectedPr?.url" 
              :currentQuery="currentQuery"
              :showStarredOnly="showStarredOnly"
              @select-pr="selectedPr = $event" 
            />
          </div>

          <!-- Content Resizer -->
          <div class="content-resizer" @mousedown="startContentResize"></div>

          <!-- GitHub PR Detail Section -->
          <div class="h-100 overflow-auto module-detail-pane flex-grow-1">
            <GitHubPrDetail :pr="selectedPr" />
          </div>
        </div>
      </div>
        </div>
      </div>
    </div>
    <GitHubQueriesDialog 
      v-if="showQueriesDialog" 
      @close="showQueriesDialog = false" 
      @saved="loadFilters" 
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import Navbar from './components/Navbar.vue';
import GitHubPrs from './components/GitHubPrs.vue';
import GitHubPrDetail from './components/GitHubPrDetail.vue';
import GitHubQueriesDialog from './components/GitHubQueriesDialog.vue';
import { fetchGitHubQueries } from './js/dashboard-api';

const selectedPr = ref(null);
const queries = ref([]);
const currentQuery = ref(null);
const showStarredOnly = ref(false);
const loadingFilters = ref(false);
const showQueriesDialog = ref(false);

// Sidebar state
const sidebarCollapsed = ref(localStorage.getItem('githubSidebarCollapsed') === 'true');
const sidebarWidth = ref(parseInt(localStorage.getItem('githubSidebarWidth')) || 230);
const contentSplitWidth = ref(parseInt(localStorage.getItem('githubContentSplitWidth')) || 40);

const sidebarStyle = computed(() => ({
  width: sidebarCollapsed.value ? '50px' : `${sidebarWidth.value}px`
}));

const selectedQueryName = computed(() => {
  if (showStarredOnly.value) return 'Starred';
  if (currentQuery.value === null) return 'Mine';
  const query = queries.value.find(q => q.query === currentQuery.value);
  return query ? query.name : '';
});

const selectQuery = (query) => {
  currentQuery.value = query;
  showStarredOnly.value = false;
};

const selectStarred = () => {
  showStarredOnly.value = true;
};

const loadFilters = async () => {
  loadingFilters.value = true;
  try {
    const data = await fetchGitHubQueries();
    queries.value = data;
  } catch (e) {
    console.error('Error loading GitHub filters:', e);
  } finally {
    loadingFilters.value = false;
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
    localStorage.setItem('githubSidebarWidth', sidebarWidth.value);
    document.removeEventListener('mousemove', onMouseMove);
    document.removeEventListener('mouseup', onMouseUp);
  };

  document.addEventListener('mousemove', onMouseMove);
  document.addEventListener('mouseup', onMouseUp);
};

// Content Resize logic
const startContentResize = (e) => {
  e.preventDefault();
  const container = document.querySelector('.github-content-wrapper');
  if (!container) return;
  
  const onMouseMove = (moveEvent) => {
    const containerRect = container.getBoundingClientRect();
    const newPercent = ((moveEvent.clientX - containerRect.left) / containerRect.width) * 100;
    contentSplitWidth.value = Math.max(20, Math.min(80, newPercent));
  };

  const onMouseUp = () => {
    localStorage.setItem('githubContentSplitWidth', contentSplitWidth.value);
    document.removeEventListener('mousemove', onMouseMove);
    document.removeEventListener('mouseup', onMouseUp);
  };

  document.addEventListener('mousemove', onMouseMove);
  document.addEventListener('mouseup', onMouseUp);
};

watch(sidebarCollapsed, (val) => {
  localStorage.setItem('githubSidebarCollapsed', val);
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

.github-app-container {
  display: flex;
  height: 100%;
}

.github-sidebar {
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
}

.github-sidebar.collapsed .filter-item {
  justify-content: center;
  padding: 0.8rem 0;
}

.github-sidebar.collapsed .filter-icon-wrapper {
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

.github-content-wrapper {
  flex-grow: 1;
  overflow: hidden;
}

.module-list-pane {
  background-color: var(--bg-dark);
}

.module-detail-pane {
  background-color: var(--bg-darker);
}

/* Single separator for GitHub PRs list and details */
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

.form-control::placeholder {
  color: var(--text-muted) !important;
  opacity: 0.6 !important;
}

label, .form-label {
  color: var(--text-primary) !important;
  opacity: 0.9 !important;
}

.card, .card-header, .card-footer, .badge {
  border-radius: 6px !important;
}

.list-group-item {
  border-left: none;
  border-right: none;
  transition: background-color 0.2s;
}

.list-group-item:hover {
  background-color: var(--bg-card) !important;
}

.text-info {
  color: var(--accent-blue) !important;
}
</style>
