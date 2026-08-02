<template>
  <div :class="['vh-100 d-flex flex-row overflow-hidden app-root', themeClass]">
    <Navbar />
    <div class="flex-grow-1 overflow-hidden d-flex flex-column main-wrapper">
      <div id="app-content" class="settings-app-container flex-grow-1">
        <!-- Sidebar -->
        <div class="tasks-sidebar" :class="{ collapsed: sidebarCollapsed }" :style="sidebarStyle">
          <div class="sidebar-header d-flex align-items-center">
            <h5 v-if="!sidebarCollapsed" class="theme-text">Settings</h5>
            <div v-else class="mx-auto theme-text">
              <i class="bi bi-gear-fill"></i>
            </div>
          </div>
          <div class="project-list">
            <div v-for="cat in categories" :key="cat.id" 
                 class="project-item theme-text"
                 :class="{ active: activeCategory === cat.id }"
                 @click="activeCategory = cat.id"
                 :title="sidebarCollapsed ? cat.name : ''">
              <div class="project-icon-wrapper" :style="{ backgroundColor: cat.color }">
                <i :class="cat.icon"></i>
              </div>
              <span v-if="!sidebarCollapsed" class="project-name theme-text">{{ cat.name }}</span>
            </div>
          </div>
          <div class="sidebar-footer" :class="{ 'collapsed': sidebarCollapsed }">
            <button class="sidebar-toggle" @click="sidebarCollapsed = !sidebarCollapsed" :title="sidebarCollapsed ? 'Expand Menu' : 'Collapse Menu'">
              <i class="bi" :class="sidebarCollapsed ? 'bi-chevron-right' : 'bi-chevron-left'"></i>
            </button>
          </div>
        </div>

        <!-- Sidebar Resizer -->
        <div v-if="!sidebarCollapsed" class="sidebar-resizer" @mousedown="startSidebarResize"></div>

        <!-- Main Content -->
        <div class="main-content">
          <div class="controls-bar theme-border">
            <div class="d-flex align-items-center theme-text">
              <i :class="currentCategory.icon" class="me-2 fs-5"></i>
              <h5 class="mb-0">{{ currentCategory.name }} Settings</h5>
            </div>
            <div class="d-flex align-items-center gap-2">
              <button v-if="activeCategory === 'colorschemes'" class="btn btn-success" @click="addScheme">
                <i class="bi bi-plus-lg me-2"></i>
                Add Scheme
              </button>
              <button class="btn btn-primary" @click="save" :disabled="saving">
                <i class="bi bi-save me-2"></i>
                {{ saving ? 'Saving...' : 'Save Changes' }}
              </button>
            </div>
          </div>

          <div class="settings-container p-4">
            <div v-if="loading" class="text-center p-5">
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
            </div>

            <div v-else class="content-area-wrapper">
              <div class="settings-container p-4">
                <div class="row g-4 pb-5 content-area">
                  <div class="col-md-12">
                    <SettingsGeneral v-if="activeCategory === 'general'" :config="config" :allPages="allPages" />
                    <SettingsIntegrations v-if="activeCategory === 'integrations'" :jira="config.jira" :gitHub="config.gitHub" />
                    <SettingsMicrosoftGraph v-if="activeCategory === 'msgraph'" :microsoftGraph="config.microsoftGraph" />
                    <div v-if="activeCategory === 'colorschemes'">
                      <SettingsColorSchemes :colorSchemes="config.colorSchemes" :config="config" />
                    </div>
                    <SettingsDebug v-if="activeCategory === 'debug'" 
                                   :debug="config.debug" />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue';
import Navbar from './components/Navbar.vue';
import SettingsIntegrations from './components/SettingsIntegrations.vue';
import SettingsGeneral from './components/SettingsGeneral.vue';
import SettingsMicrosoftGraph from './components/SettingsMicrosoftGraph.vue';
import SettingsColorSchemes from './components/SettingsColorSchemes.vue';
import SettingsDebug from './components/SettingsDebug.vue';
import { fetchSettings, updateSettings } from './js/dashboard-api';
import { showToast } from './components/Toast.vue';

const loadSetting = (key, defaultValue) => {
  const val = localStorage.getItem(key);
  if (!val) return defaultValue;
  try {
    return JSON.parse(val);
  } catch (e) {
    return defaultValue;
  }
};

const config = ref({
  jira: { url: '', username: '', apiToken: '', queries: [] },
  gitHub: { organization: '', username: '', accessToken: '' },
  microsoftGraph: { tenantId: '', clientId: '' },
  pageVisibilities: [],
  colorSchemes: [],
  theme: 'Cosmic',
  debug: { demoMode: false }
});

const allPages = [
  { id: 'general', name: 'General' },
  { id: 'integrations', name: 'Integrations' },
  { id: 'tasks', name: 'Tasks' },
  { id: 'scheduledtasks', name: 'Schedules' },
  { id: 'email', name: 'Email' },
  { id: 'calendar', name: 'Calendar' },
  { id: 'links', name: 'Links' },
  { id: 'notepad', name: 'Notepad' },
  { id: 'colorschemes', name: 'Color Schemes' },
  { id: 'debug', name: 'Diagnostics' }
];

const categories = [
  { id: 'general', name: 'General', icon: 'bi bi-gear-wide-connected text-white', color: '#6c757d' },
  { id: 'integrations', name: 'Integrations', icon: 'bi bi-plug text-white', color: '#0052CC' },
  { id: 'msgraph', name: 'MS Graph', icon: 'bi bi-microsoft text-white', color: '#00a4ef' },
  { id: 'colorschemes', name: 'Color Schemes', icon: 'bi bi-palette text-white', color: '#e83e8c' },
  { id: 'debug', name: 'Debug', icon: 'bi bi-bug text-white', color: '#ffc107' }
];

const activeCategory = ref('general');
const currentCategory = computed(() => categories.find(c => c.id === activeCategory.value));

const loading = ref(true);
const saving = ref(false);

const themeClass = computed(() => `theme-${config.value.theme.toLowerCase()}`);

const sidebarCollapsed = ref(loadSetting('sidebarCollapsed', false));
const sidebarWidth = ref(loadSetting('settingsSidebarWidth', 260));
const isResizingSidebar = ref(false);
let sidebarStartX = 0;
let sidebarStartWidth = 0;

const sidebarStyle = computed(() => {
  if (sidebarCollapsed.value) return {};
  return { 
    width: sidebarWidth.value + 'px',
    transition: isResizingSidebar.value ? 'none' : 'width 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
  };
});

watch(sidebarWidth, (newVal) => {
  localStorage.setItem('settingsSidebarWidth', JSON.stringify(newVal));
});

watch(sidebarCollapsed, (newVal) => {
  localStorage.setItem('sidebarCollapsed', JSON.stringify(newVal));
});

const startSidebarResize = (e) => {
  isResizingSidebar.value = true;
  sidebarStartX = e.clientX;
  sidebarStartWidth = sidebarWidth.value;
  
  document.addEventListener('mousemove', doSidebarResize);
  document.addEventListener('mouseup', stopSidebarResize);
  document.body.style.cursor = 'col-resize';
  document.body.style.userSelect = 'none';
  
  e.preventDefault();
  e.stopPropagation();
};

const doSidebarResize = (e) => {
  if (!isResizingSidebar.value) return;
  const delta = e.clientX - sidebarStartX;
  const newWidth = sidebarStartWidth + delta;
  if (newWidth > 150 && newWidth < 600) {
    sidebarWidth.value = newWidth;
  }
};

const stopSidebarResize = () => {
  isResizingSidebar.value = false;
  document.removeEventListener('mousemove', doSidebarResize);
  document.removeEventListener('mouseup', stopSidebarResize);
  document.body.style.cursor = '';
  document.body.style.userSelect = '';
};

const addScheme = () => {
  config.value.colorSchemes.push({
    name: 'New Scheme',
    colors: [
      { name: 'Primary', color: '#007bff' }
    ]
  });
};

onMounted(async () => {
  try {
    const data = await fetchSettings();
    config.value = data;
  } catch (e) {
    console.error('Failed to load settings:', e);
  } finally {
    loading.value = false;
  }
});

const save = async () => {
  saving.value = true;
  try {
    await updateSettings(config.value);
    showToast('Settings saved successfully!', 'success');
  } catch (e) {
    console.error('Failed to save settings:', e);
    showToast('Failed to save settings: ' + e.message, 'error');
  } finally {
    saving.value = false;
  }
};
</script>

<style>
.app-root {
  background-color: var(--bg-darker);
}

.main-wrapper {
  background-color: var(--bg-darker);
}

.settings-app-container {
  display: flex;
  flex-direction: row;
  overflow: hidden;
  height: 100%;
}

.tasks-sidebar {
  width: 260px;
  background-color: var(--bg-dark);
  border-right: 1px solid var(--border-primary);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  flex-shrink: 0;
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.tasks-sidebar.collapsed {
  width: 50px !important;
}

.sidebar-header {
  padding: 1rem;
  border-bottom: 1px solid var(--border-primary);
  height: 60px;
  flex-shrink: 0;
}

.sidebar-header h5 {
  margin: 0;
  font-weight: 600;
  white-space: nowrap;
}

.project-list {
  flex-grow: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 0.5rem 0;
}

.project-item {
  padding: 0.75rem 1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: background-color 0.2s, padding 0.3s ease;
  border-left: 3px solid transparent;
  min-width: 0;
  overflow: hidden;
}

.tasks-sidebar.collapsed .project-item {
  padding: 0.75rem 0;
  justify-content: center;
}

.project-item:hover {
  background-color: var(--bg-card);
}

.project-item.active {
  background-color: rgba(88, 166, 255, 0.1);
  border-left-color: var(--accent-blue);
}

.project-icon-wrapper {
  width: 24px;
  height: 24px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 12px;
  font-size: 0.9rem;
  color: white;
  flex-shrink: 0;
}

.tasks-sidebar.collapsed .project-icon-wrapper {
  margin-right: 0;
}

.project-name {
  flex-grow: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  font-size: 0.95rem;
}

.sidebar-footer {
  padding: 0.5rem;
  border-top: 1px solid var(--border-primary);
  display: flex;
  justify-content: flex-end;
}

.sidebar-footer.collapsed {
  justify-content: center;
}

.sidebar-toggle {
  background: transparent;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 4px;
}

.sidebar-toggle:hover {
  background-color: var(--bg-card);
  color: var(--text-primary);
}

.main-content {
  flex-grow: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  background-color: var(--bg-darker);
}

.settings-container {
  max-width: 1000px;
  margin: 0 auto;
  width: 100%;
  flex-grow: 1;
  overflow-y: auto;
}

.content-area-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
}

.settings-content-header {
  background-color: var(--bg-dark);
  flex-shrink: 0;
}

.category-icon-wrapper {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  box-shadow: 0 4px 10px rgba(0,0,0,0.2);
}

.card {
  box-shadow: 0 4px 6px rgba(0,0,0,0.3);
}

.form-control:focus {
  background-color: var(--bg-dark);
  color: white;
  border-color: var(--accent-blue);
  box-shadow: 0 0 0 0.25rem rgba(88, 166, 255, 0.25);
}

body {
  background-color: var(--bg-darker) !important;
  color: var(--text-primary) !important;
  font-family: 'Noto Sans', sans-serif !important;
  height: 100vh;
  margin: 0;
  padding: 0;
  overflow: hidden;
}

.form-control::placeholder {
  color: var(--text-muted) !important;
  opacity: 0.6 !important;
}

label, .form-label {
  color: var(--text-primary) !important;
  opacity: 0.9 !important;
}

/* Custom Scrollbar */
::-webkit-scrollbar {
  width: 10px;
  height: 10px;
}
::-webkit-scrollbar-track {
  background: var(--bg-darker);
}
::-webkit-scrollbar-thumb {
  background: var(--border-primary);
  border-radius: 5px;
  border: 2px solid var(--bg-darker);
}
::-webkit-scrollbar-thumb:hover {
  background: var(--text-muted);
}
</style>
