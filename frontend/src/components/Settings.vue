<template>
  <div class="settings-container p-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h3><i class="bi bi-gear-fill me-2"></i>Settings</h3>
      <div>
        <button class="btn btn-primary" @click="save" :disabled="saving">
          {{ saving ? 'Saving...' : 'Save Changes' }}
        </button>
      </div>
    </div>

    <div v-if="loading" class="text-center p-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div v-else class="row g-4 pb-5">
      <!-- Left Column: Jira Settings -->
      <div class="col-md-7">
        <div class="card bg-dark text-light border-secondary">
          <div class="card-header border-secondary d-flex justify-content-between align-items-center">
            <h5 class="mb-0"><i class="bi bi-jira me-2"></i>Jira Configuration</h5>
          </div>
          <div class="card-body">
            <div class="row">
              <div class="col-md-12 mb-3">
                <label class="form-label">Jira URL</label>
                <input v-model="config.jira.url" type="text" class="form-control bg-dark text-light border-secondary" placeholder="https://your-domain.atlassian.net/">
              </div>
              <div class="col-md-6 mb-3">
                <label class="form-label">Username (Email)</label>
                <input v-model="config.jira.username" type="email" class="form-control bg-dark text-light border-secondary" placeholder="user@example.com">
              </div>
              <div class="col-md-6 mb-3">
                <label class="form-label">API Token</label>
                <input v-model="config.jira.apiToken" type="password" class="form-control bg-dark text-light border-secondary">
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Right Column: GitHub, MS Graph and Page Visibility Settings -->
      <div class="col-md-5">
        <div class="row g-4">
          <!-- Page Visibility Settings -->
          <div class="col-md-12">
            <div class="card bg-dark text-light border-secondary">
              <div class="card-header border-secondary">
                <h5 class="mb-0"><i class="bi bi-eye me-2"></i>Page Visibility</h5>
              </div>
              <div class="card-body">
                <p class="small text-muted mb-3">Choose which modules are visible in the sidebar and dashboard.</p>
                <div class="row">
                  <div v-for="page in allPages" :key="page.id" class="col-6 mb-2">
                    <div class="form-check form-switch">
                      <input class="form-check-input" type="checkbox" 
                             :id="'vis-' + page.id" 
                             :checked="getPageVisibility(page.id)"
                             @change="togglePageVisibility(page.id)">
                      <label class="form-check-label" :for="'vis-' + page.id">{{ page.name }}</label>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- GitHub Settings -->
          <div class="col-md-12">
            <div class="card bg-dark text-light border-secondary">
              <div class="card-header border-secondary">
                <h5 class="mb-0"><i class="bi bi-github me-2"></i>GitHub Configuration</h5>
              </div>
              <div class="card-body">
                <div class="mb-3">
                  <label class="form-label">Organization</label>
                  <input v-model="config.gitHub.organization" type="text" class="form-control bg-dark text-light border-secondary" placeholder="your-org">
                </div>
                <div class="mb-3">
                  <label class="form-label">Username</label>
                  <input v-model="config.gitHub.username" type="text" class="form-control bg-dark text-light border-secondary" placeholder="your-username">
                </div>
                <div class="mb-3">
                  <label class="form-label">Access Token</label>
                  <input v-model="config.gitHub.accessToken" type="password" class="form-control bg-dark text-light border-secondary">
                </div>
              </div>
            </div>
          </div>

          <!-- Microsoft Graph Settings -->
          <div class="col-md-12">
            <div class="card bg-dark text-light border-secondary">
              <div class="card-header border-secondary">
                <h5 class="mb-0"><i class="bi bi-microsoft me-2"></i>Microsoft Graph Configuration</h5>
              </div>
              <div class="card-body">
                <div class="mb-3">
                  <label class="form-label">Tenant ID</label>
                  <input v-model="config.microsoftGraph.tenantId" type="text" class="form-control bg-dark text-light border-secondary">
                </div>
                <div class="mb-3">
                  <label class="form-label">Client ID</label>
                  <input v-model="config.microsoftGraph.clientId" type="text" class="form-control bg-dark text-light border-secondary">
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
import { ref, onMounted } from 'vue';
import { fetchSettings, updateSettings } from '../js/dashboard-api';

const emit = defineEmits(['saved']);
const config = ref({
  jira: { url: '', username: '', apiToken: '', queries: [] },
  gitHub: { organization: '', username: '', accessToken: '' },
  microsoftGraph: { tenantId: '', clientId: '' },
  pageVisibilities: []
});
const allPages = [
  { id: 'jira', name: 'Jira' },
  { id: 'github', name: 'GitHub' },
  { id: 'tasks', name: 'Tasks' },
  { id: 'scheduledtasks', name: 'Schedules' },
  { id: 'alarms', name: 'Alarms' },
  { id: 'email', name: 'Email' },
  { id: 'calendar', name: 'Calendar' },
  { id: 'links', name: 'Links' },
  { id: 'notepad', name: 'Notepad' },
  { id: 'debug', name: 'Diagnostics' }
];

const getPageVisibility = (id) => {
  const page = config.value.pageVisibilities.find(p => p.id === id);
  return page ? page.visible : true;
};

const togglePageVisibility = (id) => {
  let page = config.value.pageVisibilities.find(p => p.id === id);
  if (!page) {
    page = { id, visible: true };
    config.value.pageVisibilities.push(page);
  }
  page.visible = !page.visible;
};
const saving = ref(false);

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
    alert('Settings saved successfully!');
    emit('saved');
  } catch (e) {
    console.error('Failed to save settings:', e);
    alert('Failed to save settings: ' + e.message);
  } finally {
    saving.value = false;
  }
};
</script>

<style scoped>
.settings-container {
  color: var(--text-primary);
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
</style>
