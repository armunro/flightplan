<template>
  <div class="modal-overlay">
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
          <button v-if="!isNew" class="tab-btn" :class="{ active: activeTab === 'customFields' }" @click="activeTab = 'customFields'">Custom Fields</button>
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
            <ColorPicker v-model="form.color" show-text />
          </div>

          <div class="form-group mb-3">
            <label class="form-label">Description</label>
            <textarea v-model="form.description" class="form-control" rows="3" placeholder="Optional project description"></textarea>
          </div>
        </div>

        <div v-if="activeTab === 'statuses'" class="tab-content">
          <div v-for="(status, index) in form.statuses" 
               :key="status.id || index" 
               class="item-edit-row mb-2"
               draggable="true"
               @dragstart="onDragStart($event, index, 'statuses')"
               @dragover.prevent="onDragOver($event, index)"
               @dragleave="onDragLeave"
               @drop="onDrop($event, index, 'statuses')"
               :class="{ 'drag-over': dragOverIndex === index && dragOverTab === 'statuses' }">
            <div class="drag-handle">
              <i class="bi bi-grip-vertical"></i>
            </div>
            <input v-model="status.name" type="text" class="form-control form-control-sm" placeholder="Status Name">
            <ColorPicker v-model="status.color" size="sm" palette-placement="top-start" />
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
          <div v-for="(type, index) in form.taskTypes" 
               :key="type.id || index" 
               class="item-edit-row mb-2"
               draggable="true"
               @dragstart="onDragStart($event, index, 'taskTypes')"
               @dragover.prevent="onDragOver($event, index)"
               @dragleave="onDragLeave"
               @drop="onDrop($event, index, 'taskTypes')"
               :class="{ 'drag-over': dragOverIndex === index && dragOverTab === 'taskTypes' }">
            <div class="drag-handle">
              <i class="bi bi-grip-vertical"></i>
            </div>
            <input v-model="type.name" type="text" class="form-control form-control-sm" placeholder="Type Name">
            <input v-model="type.icon" type="text" class="form-control form-control-sm" placeholder="Icon (bi-tag)">
            <ColorPicker v-model="type.color" size="sm" palette-placement="top-start" />
            <button class="btn btn-sm btn-outline-danger" @click="removeTaskType(index)" title="Remove Type">
              <i class="bi bi-trash"></i>
            </button>
          </div>
          <button class="btn btn-sm btn-outline-primary mt-2" @click="addTaskType">
            <i class="bi bi-plus-lg"></i> Add Task Type
          </button>
        </div>

        <div v-if="activeTab === 'priorities'" class="tab-content">
          <div v-for="(priority, index) in form.priorities" 
               :key="priority.id || index" 
               class="item-edit-row mb-2"
               draggable="true"
               @dragstart="onDragStart($event, index, 'priorities')"
               @dragover.prevent="onDragOver($event, index)"
               @dragleave="onDragLeave"
               @drop="onDrop($event, index, 'priorities')"
               :class="{ 'drag-over': dragOverIndex === index && dragOverTab === 'priorities' }">
            <div class="drag-handle">
              <i class="bi bi-grip-vertical"></i>
            </div>
            <input v-model="priority.name" type="text" class="form-control form-control-sm" placeholder="Priority Name">
            <input v-model="priority.icon" type="text" class="form-control form-control-sm" placeholder="Icon (bi-dash-lg)">
            <ColorPicker v-model="priority.color" size="sm" palette-placement="top-start" />
            <button class="btn btn-sm btn-outline-danger" @click="removePriority(index)" title="Remove Priority">
              <i class="bi bi-trash"></i>
            </button>
          </div>
          <button class="btn btn-sm btn-outline-primary mt-2" @click="addPriority">
            <i class="bi bi-plus-lg"></i> Add Priority
          </button>
        </div>

        <div v-if="activeTab === 'customFields'" class="tab-content">
          <div v-for="(field, index) in form.customFields" 
               :key="field.id || index" 
               class="custom-field-edit-block mb-3 p-2 border rounded border-secondary">
            <div class="d-flex gap-2 mb-2 align-items-center">
              <div class="drag-handle" 
                   draggable="true"
                   @dragstart="onDragStart($event, index, 'customFields')"
                   @dragover.prevent="onDragOver($event, index)"
                   @dragleave="onDragLeave"
                   @drop="onDrop($event, index, 'customFields')"
                   :class="{ 'drag-over': dragOverIndex === index && dragOverTab === 'customFields' }">
                <i class="bi bi-grip-vertical"></i>
              </div>
              <input v-model="field.name" type="text" class="form-control form-control-sm" placeholder="Field Name">
              <select v-model="field.type" class="form-select form-select-sm w-auto" @change="onFieldTypeChange(field)">
                <option :value="0">Text</option>
                <option :value="1">Single Select</option>
                <option :value="2">Multi Select</option>
                <option value="Text" hidden>Text</option>
                <option value="SingleSelect" hidden>Single Select</option>
                <option value="MultiSelect" hidden>Multi Select</option>
              </select>
              <button class="btn btn-sm btn-outline-danger" @click="removeCustomField(index)" title="Remove Field">
                <i class="bi bi-trash"></i>
              </button>
            </div>
            
            <div v-if="field.type === 1 || field.type === 2" class="ms-4 mt-2">
              <label class="form-label small text-muted">Options (one per line)</label>
              <textarea 
                v-model="field.optionsText"
                @input="field.options = field.optionsText.split('\n')"
                @blur="onBlurOptions(field)"
                class="form-control form-control-sm" 
                rows="3" 
                placeholder="Option 1&#10;Option 2">
              </textarea>
            </div>
          </div>
          <button class="btn btn-sm btn-outline-primary mt-2" @click="addCustomField">
            <i class="bi bi-plus-lg"></i> Add Custom Field
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
import { ref, reactive, onMounted, onUnmounted, computed } from 'vue';
import ColorPicker from './ColorPicker.vue';

export default {
  name: 'ProjectDialog',
  components: {
    ColorPicker
  },
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
    const dragOverIndex = ref(-1);
    const dragOverTab = ref(null);
    const draggedIndex = ref(-1);

    const form = reactive({
      name: props.project.name || '',
      icon: props.project.icon || 'bi-folder',
      color: props.project.color || '#58a6ff',
      description: props.project.description || '',
      statuses: props.project.statuses ? JSON.parse(JSON.stringify(props.project.statuses)) : [],
      taskTypes: props.project.taskTypes ? JSON.parse(JSON.stringify(props.project.taskTypes)) : [],
      priorities: props.project.priorities ? JSON.parse(JSON.stringify(props.project.priorities)) : [],
      customFields: (props.project.customFields || []).map(f => {
        const type = typeof f.type === 'string' 
          ? (f.type === 'Text' ? 0 : f.type === 'SingleSelect' ? 1 : f.type === 'MultiSelect' ? 2 : parseInt(f.type, 10) || 0)
          : f.type;
        return {
          ...JSON.parse(JSON.stringify(f)),
          type,
          optionsText: (f.options || []).join('\n')
        };
      })
    });

    const onKeyDown = (e) => {
      if (e.key === 'Escape') {
        emit('close');
      }
    };

    onMounted(() => {
      nameInput.value?.focus();
      window.addEventListener('keydown', onKeyDown);
    });

    onUnmounted(() => {
      window.removeEventListener('keydown', onKeyDown);
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

    const addCustomField = () => {
      form.customFields.push({
        id: crypto.randomUUID(),
        name: 'New Field',
        type: 0, // Text
        options: [],
        optionsText: ''
      });
    };

    const removeCustomField = (index) => {
      form.customFields.splice(index, 1);
    };

    const onFieldTypeChange = (field) => {
      // Convert string types to numbers if they come from the select
      if (field.type === 'Text') field.type = 0;
      else if (field.type === 'SingleSelect') field.type = 1;
      else if (field.type === 'MultiSelect') field.type = 2;

      if (field.type === 0) {
        field.options = [];
        field.optionsText = '';
      } else if (!field.options) {
        field.options = [];
        field.optionsText = '';
      }
    };

    const onBlurOptions = (field) => {
      field.options = field.optionsText.split('\n').filter(o => o.trim());
      field.optionsText = field.options.join('\n');
    };

    const onSave = () => {
      if (form.name.trim()) {
        // Ensure options are correctly synchronized and initialized
        form.customFields.forEach(f => {
          if (f.type === 1 || f.type === 2 || f.type === 'SingleSelect' || f.type === 'MultiSelect') {
            if (f.optionsText !== undefined) {
              f.options = f.optionsText.split('\n').filter(o => o.trim());
            }
          }
          if (!f.options) f.options = [];
          
          // Ensure type is a number for the backend DTO
          if (typeof f.type === 'string') {
            if (f.type === 'Text') f.type = 0;
            else if (f.type === 'SingleSelect') f.type = 1;
            else if (f.type === 'MultiSelect') f.type = 2;
            else f.type = parseInt(f.type, 10) || 0;
          }
        });
        
        // Strip out frontend-only properties like optionsText before sending
        const data = JSON.parse(JSON.stringify(form));
        if (data.customFields) {
          data.customFields.forEach(f => {
            delete f.optionsText;
          });
        }
        
        // Final sanity check for backend expectation
        data.customFields = data.customFields.map(f => ({
          ...f,
          type: typeof f.type === 'string' 
            ? (f.type === 'Text' ? 0 : f.type === 'SingleSelect' ? 1 : f.type === 'MultiSelect' ? 2 : parseInt(f.type, 10) || 0)
            : f.type
        }));
        
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

    const onDragStart = (e, index, tab) => {
      draggedIndex.value = index;
      dragOverTab.value = tab;
      e.dataTransfer.effectAllowed = 'move';
      // Set a ghost image if needed, or just rely on default
    };

    const onDragOver = (e, index) => {
      dragOverIndex.value = index;
    };

    const onDragLeave = () => {
      dragOverIndex.value = -1;
    };

    const onDrop = (e, index, tab) => {
      if (draggedIndex.value === -1 || draggedIndex.value === index || dragOverTab.value !== tab) {
        dragOverIndex.value = -1;
        dragOverTab.value = null;
        draggedIndex.value = -1;
        return;
      }

      const list = form[tab];
      const item = list.splice(draggedIndex.value, 1)[0];
      list.splice(index, 0, item);

      // Update orders if they exist
      list.forEach((item, i) => {
        if (Object.prototype.hasOwnProperty.call(item, 'order')) {
          item.order = i;
        }
      });

      dragOverIndex.value = -1;
      dragOverTab.value = null;
      draggedIndex.value = -1;
    };

    return {
      form,
      nameInput,
      activeTab,
      dragOverIndex,
      dragOverTab,
      onDragStart,
      onDragOver,
      onDragLeave,
      onDrop,
      addStatus,
      removeStatus,
      addTaskType,
      removeTaskType,
      addPriority,
      removePriority,
      addCustomField,
      removeCustomField,
      onFieldTypeChange,
      onBlurOptions,
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
  width: 700px;
  max-width: 95vw;
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
  padding: 4px;
  border-radius: 4px;
  transition: background-color 0.2s;
}

.item-edit-row.drag-over {
  background-color: rgba(88, 166, 255, 0.1);
  outline: 1px dashed var(--accent-blue);
}

.drag-handle {
  cursor: grab;
  color: var(--text-muted);
  display: flex;
  align-items: center;
  font-size: 1.2rem;
  padding: 0 4px;
}

.item-edit-row[draggable="true"]:active {
  cursor: grabbing;
}

.item-edit-row[draggable="true"]:active .drag-handle {
  cursor: grabbing;
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
