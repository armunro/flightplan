<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content project-dialog">
      <div class="modal-header">
        <h3>{{ isNew ? 'Add Project' : 'Edit Project' }}</h3>
        <button class="close-btn" @click="$emit('close')">&times;</button>
      </div>
      <div class="modal-body">
        <div class="tabs">
          <button class="tab-btn" :class="{ active: activeTab === 'general' }" @click="activeTab = 'general'">General</button>
          <button v-if="!isNew" class="tab-btn" :class="{ active: activeTab === 'statuses' }" @click="activeTab = 'statuses'">Statuses</button>
          <button v-if="!isNew" class="tab-btn" :class="{ active: activeTab === 'types' }" @click="activeTab = 'types'">Task Types</button>
          <button v-if="!isNew" class="tab-btn" :class="{ active: activeTab === 'priorities' }" @click="activeTab = 'priorities'">Priorities</button>
        </div>

        <div v-if="activeTab === 'general'" class="tab-content">
          <div class="form-group mb-3">
            <label class="form-label">Project Name</label>
            <input v-model="form.name" type="text" class="form-control" placeholder="Enter project name" ref="nameInput">
          </div>
          
          <div class="form-group mb-3">
            <label class="form-label">Icon (Bootstrap Icon class)</label>
            <div class="input-group">
              <span class="input-group-text preview-icon-box">
                <i :class="[displayIconClass]" :style="{ color: form.color }"></i>
              </span>
              <input v-model="form.icon" type="text" class="form-control" placeholder="e.g. bi-star">
            </div>
            <small class="text-muted">Use any <a href="https://icons.getbootstrap.com/" target="_blank" class="text-info">Bootstrap Icon</a> class name.</small>
          </div>

          <div class="form-group mb-3">
            <label class="form-label">Project Color</label>
            <div class="d-flex align-items-center gap-2">
              <input v-model="form.color" type="color" class="form-color-input">
              <input v-model="form.color" type="text" class="form-control" placeholder="#000000">
            </div>
          </div>

          <div class="form-group mb-3">
            <label class="form-label">Description</label>
            <textarea v-model="form.description" class="form-control" rows="3" placeholder="Optional project description"></textarea>
          </div>
        </div>

        <div v-if="activeTab === 'statuses'" class="tab-content">
          <div v-for="(status, index) in form.statuses" :key="status.id || index" class="item-edit-row mb-2">
            <input v-model="status.name" type="text" class="form-control form-control-sm" placeholder="Status Name">
            <input v-model="status.color" type="color" class="form-color-input-sm">
            <div class="form-check">
              <input class="form-check-input" type="checkbox" v-model="status.isCompletedState" :id="'completed-' + index">
              <label class="form-check-label text-nowrap" :for="'completed-' + index">Done</label>
            </div>
            <button class="btn btn-sm btn-outline-danger" @click="removeStatus(index)" title="Remove Status">
              <i class="bi bi-trash"></i>
            </button>
          </div>
          <button class="btn btn-sm btn-outline-primary mt-2" @click="addStatus">
            <i class="bi bi-plus-lg"></i> Add Status
          </button>
        </div>

        <div v-if="activeTab === 'types'" class="tab-content">
          <div v-for="(type, index) in form.taskTypes" :key="type.id || index" class="item-edit-row mb-2">
            <input v-model="type.name" type="text" class="form-control form-control-sm" placeholder="Type Name">
            <input v-model="type.icon" type="text" class="form-control form-control-sm" placeholder="Icon (bi-tag)">
            <input v-model="type.color" type="color" class="form-color-input-sm">
            <button class="btn btn-sm btn-outline-danger" @click="removeTaskType(index)" title="Remove Type">
              <i class="bi bi-trash"></i>
            </button>
          </div>
          <button class="btn btn-sm btn-outline-primary mt-2" @click="addTaskType">
            <i class="bi bi-plus-lg"></i> Add Task Type
          </button>
        </div>

        <div v-if="activeTab === 'priorities'" class="tab-content">
          <div v-for="(priority, index) in form.priorities" :key="priority.id || index" class="item-edit-row mb-2">
            <input v-model="priority.name" type="text" class="form-control form-control-sm" placeholder="Priority Name">
            <input v-model="priority.icon" type="text" class="form-control form-control-sm" placeholder="Icon (bi-dash-lg)">
            <input v-model="priority.color" type="color" class="form-color-input-sm">
            <button class="btn btn-sm btn-outline-danger" @click="removePriority(index)" title="Remove Priority">
              <i class="bi bi-trash"></i>
            </button>
          </div>
          <button class="btn btn-sm btn-outline-primary mt-2" @click="addPriority">
            <i class="bi bi-plus-lg"></i> Add Priority
          </button>
        </div>
      </div>
      <div class="modal-footer">
        <div class="footer-left">
          <button v-if="!isNew" class="btn btn-outline-danger" @click="onDelete">
            <i class="bi bi-trash"></i> Delete Project
          </button>
        </div>
        <div class="footer-right">
          <button class="btn btn-outline-secondary" @click="$emit('close')">Cancel</button>
          <button class="btn btn-primary" :disabled="!form.name" @click="onSave">
            {{ isNew ? 'Create Project' : 'Save Changes' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, reactive, onMounted, computed } from 'vue';

export default {
  name: 'ProjectDialog',
  props: {
    project: {
      type: Object,
      default: () => ({})
    },
    isNew: {
      type: Boolean,
      default: false
    }
  },
  emits: ['close', 'save', 'delete'],
  setup(props, { emit }) {
    const nameInput = ref(null);
    const activeTab = ref('general');
    const form = reactive({
      name: props.project.name || '',
      icon: props.project.icon || 'bi-folder',
      color: props.project.color || '#58a6ff',
      description: props.project.description || '',
      statuses: props.project.statuses ? JSON.parse(JSON.stringify(props.project.statuses)) : [],
      taskTypes: props.project.taskTypes ? JSON.parse(JSON.stringify(props.project.taskTypes)) : [],
      priorities: props.project.priorities ? JSON.parse(JSON.stringify(props.project.priorities)) : []
    });

    onMounted(() => {
      nameInput.value?.focus();
    });

    const addStatus = () => {
      form.statuses.push({
        id: null,
        name: 'New Status',
        color: '#cccccc',
        isCompletedState: false,
        order: form.statuses.length
      });
    };

    const removeStatus = (index) => {
      form.statuses.splice(index, 1);
    };

    const addTaskType = () => {
      form.taskTypes.push({
        id: null,
        name: 'New Type',
        color: '#cccccc',
        icon: 'bi-tag'
      });
    };

    const removeTaskType = (index) => {
      form.taskTypes.splice(index, 1);
    };
    
    const addPriority = () => {
      form.priorities.push({
        id: null,
        name: 'New Priority',
        color: '#cccccc',
        icon: 'bi-dash-lg',
        order: form.priorities.length
      });
    };

    const removePriority = (index) => {
      form.priorities.splice(index, 1);
    };

    const onSave = () => {
      if (form.name.trim()) {
        const data = JSON.parse(JSON.stringify(form));
        emit('save', data);
      }
    };

    const onDelete = () => {
      if (confirm(`Are you sure you want to delete the project "${props.project.name}"? This will delete all tasks and lists within it.`)) {
        emit('delete', props.project.id);
      }
    };

    const displayIconClass = computed(() => {
      const icon = form.icon || 'bi-folder';
      return icon.startsWith('bi-') ? `bi ${icon}` : `bi bi-${icon}`;
    });

    return {
      form,
      nameInput,
      activeTab,
      addStatus,
      removeStatus,
      addTaskType,
      removeTaskType,
      addPriority,
      removePriority,
      onSave,
      onDelete,
      displayIconClass
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

.modal-content.project-dialog {
  background: #161b22;
  border: 1px solid var(--border-primary);
  border-radius: 8px;
  width: 450px;
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
}

.form-label {
  color: var(--text-primary);
  font-weight: 500;
  font-size: 0.9rem;
  margin-bottom: 0.5rem;
}

.form-control {
  background: var(--bg-dark) !important;
  border: 1px solid var(--border-primary);
  color: var(--text-primary) !important;
}

.form-control::placeholder {
  color: #6e7681;
}

.form-control:focus {
  background: var(--bg-dark);
  border-color: var(--accent-blue);
  color: var(--text-primary);
  box-shadow: none;
}

.tabs {
  display: flex;
  border-bottom: 1px solid var(--border-primary);
  margin-bottom: 15px;
}

.tab-btn {
  padding: 8px 16px;
  background: none;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  border-bottom: 2px solid transparent;
}

.tab-btn:hover {
  color: var(--text-primary);
}

.tab-btn.active {
  color: var(--accent-blue);
  border-bottom-color: var(--accent-blue);
}

.item-edit-row {
  display: flex;
  gap: 8px;
  align-items: center;
}

.form-color-input-sm {
  width: 30px;
  height: 30px;
  padding: 0;
  border: 1px solid var(--border-primary);
  background: none;
  cursor: pointer;
}

.form-color-input {
  width: 40px;
  height: 38px;
  padding: 0;
  border: 1px solid var(--border-primary);
  background: none;
  cursor: pointer;
}

.preview-icon-box {
  background: var(--bg-dark);
  border: 1px solid var(--border-primary);
  min-width: 45px;
  display: flex;
  justify-content: center;
  align-items: center;
}

.preview-icon-box i {
  font-size: 1.2rem;
}

.modal-footer {
  padding: 16px;
  border-top: 1px solid var(--border-primary);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.footer-right {
  display: flex;
  gap: 12px;
}

.btn-primary {
  background: var(--accent-blue);
  border-color: var(--accent-blue);
}

.btn-primary:hover {
  background: #005a9e;
  border-color: #005a9e;
}
</style>
