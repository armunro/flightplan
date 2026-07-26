<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content jira-queries-dialog">
      <div class="modal-header">
        <h3><i class="bi bi-kanban me-2"></i>Jira Queries</h3>
        <button class="close-btn" @click="$emit('close')">&times;</button>
      </div>
      <div class="modal-body">
        <div v-if="loading" class="text-center p-4">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>
        <div v-else>
          <div class="mb-3 d-flex justify-content-between align-items-center">
            <h6 class="mb-0 text-muted">Manage your custom JQL queries</h6>
            <button class="btn btn-sm btn-outline-success" @click="addQuery">
              <i class="bi bi-plus-lg me-1"></i> Add Query
            </button>
          </div>
          
          <div class="queries-list">
            <div v-for="(query, index) in localQueries" :key="index" class="query-card mb-3 p-3 border border-secondary rounded">
              <div class="d-flex justify-content-between mb-2 align-items-center">
                <input v-model="query.name" type="text" class="form-control form-control-sm bg-dark text-light border-secondary me-2" placeholder="Query Name">
                <button class="btn btn-sm btn-outline-danger" @click="removeQuery(index)">
                  <i class="bi bi-trash"></i>
                </button>
              </div>
              <div>
                <textarea v-model="query.jql" class="form-control form-control-sm bg-dark text-light border-secondary" rows="2" placeholder="JQL query"></textarea>
              </div>
            </div>
            
            <div v-if="localQueries.length === 0" class="text-center p-4 text-muted border border-secondary border-dashed rounded">
              No custom queries defined.
            </div>
          </div>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn btn-outline-secondary" @click="$emit('close')">Cancel</button>
        <button class="btn btn-primary" :disabled="loading || saving" @click="save">
          {{ saving ? 'Saving...' : 'Save Changes' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { showToast } from './Toast.vue';
import { fetchSettings, updateSettings } from '../js/dashboard-api';

const emit = defineEmits(['close', 'saved']);

const loading = ref(true);
const saving = ref(false);
const config = ref(null);
const localQueries = ref([]);

onMounted(async () => {
  try {
    const data = await fetchSettings();
    config.value = data;
    localQueries.value = data.jira.queries ? JSON.parse(JSON.stringify(data.jira.queries)) : [];
  } catch (e) {
    console.error('Failed to load settings in JiraQueriesDialog:', e);
  } finally {
    loading.value = false;
  }
});

const addQuery = () => {
  localQueries.value.push({ name: '', jql: '' });
};

const removeQuery = (index) => {
  localQueries.value.splice(index, 1);
};

const save = async () => {
  saving.value = true;
  try {
    config.value.jira.queries = localQueries.value;
    await updateSettings(config.value);
    emit('saved');
    emit('close');
  } catch (e) {
    console.error('Failed to save settings:', e);
    showToast('Failed to save settings: ' + e.message, 'error');
  } finally {
    saving.value = false;
  }
};
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 2000;
}

.modal-content.jira-queries-dialog {
  background: #161b22;
  border: 1px solid var(--border-primary);
  border-radius: 8px;
  width: 500px;
  max-width: 90vw;
  max-height: 85vh;
  box-shadow: 0 10px 25px rgba(0,0,0,0.5);
  display: flex;
  flex-direction: column;
}

.modal-header {
  padding: 16px;
  border-bottom: 1px solid var(--border-primary);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-header h3 {
  margin: 0;
  font-size: 1.2rem;
  color: var(--text-primary);
}

.close-btn {
  background: none;
  border: none;
  color: var(--text-muted);
  font-size: 1.5rem;
  cursor: pointer;
}

.modal-body {
  padding: 20px;
  overflow-y: auto;
}

.queries-list {
  max-height: 400px;
  overflow-y: auto;
}

.query-card {
  background-color: rgba(255, 255, 255, 0.02);
}

.form-control:focus {
  background: var(--bg-dark);
  border-color: var(--accent-blue);
  color: var(--text-primary);
  box-shadow: none;
}

.modal-footer {
  padding: 16px;
  border-top: 1px solid var(--border-primary);
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.btn-primary {
  background: var(--accent-blue);
  border-color: var(--accent-blue);
}

.border-dashed {
  border-style: dashed !important;
}
</style>
