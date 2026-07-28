<template>
  <div class="modal-overlay">
    <div class="modal-content create-dialog">
      <div class="modal-header">
        <h3>Create Task from Jira: {{ issue?.key }}</h3>
        <button class="close-btn" @click="$emit('close')">&times;</button>
      </div>
      <div class="modal-body">
        <div class="project-selector">
          <label>Select Destination Project:</label>
          <select v-model="selectedProjectId" class="form-select">
            <option v-for="project in projects" :key="project.id" :value="project.id">
              {{ project.name }}
            </option>
          </select>
        </div>
        
        <div class="list-selector" v-if="selectedProject">
          <label>Select Destination List:</label>
          <div class="list-grid">
            <div v-for="list in selectedProject.lists" 
                 :key="list.id" 
                 class="list-option"
                 :class="{ selected: selectedListId === list.id }"
                 @click="selectedListId = list.id">
              <span class="list-name">{{ list.name }}</span>
              <span class="task-count">({{ list.tasks?.length || 0 }} tasks)</span>
            </div>
          </div>
          <div v-if="!selectedProject.lists || selectedProject.lists.length === 0" class="empty-lists">
            No lists available in this project.
          </div>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" @click="$emit('close')">Cancel</button>
        <button class="btn-primary" 
                :disabled="!selectedListId" 
                @click="onCreate">Create Task</button>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue';

export default {
  name: 'CreateJiraTaskDialog',
  props: {
    issue: { type: Object, required: true },
    projects: { type: Array, required: true }
  },
  emits: ['close', 'create'],
  setup(props, { emit }) {
    const selectedProjectId = ref(null);
    const selectedListId = ref(null);

    const findJiraProject = () => {
        const jiraProject = props.projects.find(p => p.name.toLowerCase() === 'jira');
        return jiraProject ? jiraProject.id : props.projects[0]?.id;
    };

    const onKeyDown = (e) => {
      if (e.key === 'Escape') {
        emit('close');
      }
    };

    onMounted(() => {
        selectedProjectId.value = findJiraProject();
        window.addEventListener('keydown', onKeyDown);
    });

    onUnmounted(() => {
      window.removeEventListener('keydown', onKeyDown);
    });

    const selectedProject = computed(() => {
      return props.projects.find(p => p.id === selectedProjectId.value);
    });

    watch(selectedProjectId, (newProjId) => {
      selectedListId.value = null;
      if (newProjId) {
          const proj = props.projects.find(p => p.id === newProjId);
          if (proj && proj.lists && proj.lists.length > 0) {
              // Try to find "Inbox" or just pick the first one
              const inbox = proj.lists.find(l => l.name.toLowerCase() === 'inbox');
              selectedListId.value = inbox ? inbox.id : proj.lists[0].id;
          }
      }
    });

    const onCreate = () => {
      if (selectedListId.value) {
        emit('create', {
          issue: props.issue,
          targetListId: selectedListId.value
        });
      }
    };

    return {
      selectedProjectId,
      selectedListId,
      selectedProject,
      onCreate
    };
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

.modal-content.create-dialog {
  background: var(--bg-darker);
  border: 1px solid var(--border-primary);
  border-radius: 8px;
  width: 500px;
  max-width: 90vw;
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
  max-height: 400px;
  overflow-y: auto;
}

.project-selector {
  margin-bottom: 20px;
}

.form-select {
  width: 100%;
  padding: 8px;
  background: var(--bg-dark);
  border: 1px solid var(--border-primary);
  color: var(--text-primary);
  border-radius: 4px;
  margin-top: 8px;
}

.list-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 8px;
  margin-top: 8px;
}

.list-option {
  padding: 10px 15px;
  background: var(--bg-dark);
  border: 1px solid var(--border-primary);
  border-radius: 4px;
  cursor: pointer;
  display: flex;
  justify-content: space-between;
  transition: all 0.2s;
}

.list-option:hover {
  background: var(--bg-card);
  border-color: var(--accent-blue);
}

.list-option.selected {
  background: rgba(88, 166, 255, 0.2);
  border-color: var(--accent-blue);
}

.task-count {
  color: var(--text-muted);
  font-size: 0.85rem;
}

.empty-lists {
  padding: 20px;
  text-align: center;
  color: var(--text-muted);
  font-style: italic;
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
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-secondary {
  background: var(--bg-card);
  color: var(--text-primary);
  border: 1px solid var(--border-primary);
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
}
</style>
