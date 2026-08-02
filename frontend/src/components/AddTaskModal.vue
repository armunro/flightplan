<template>
  <div v-if="isOpen" class="modal-overlay">
    <div class="add-task-modal theme-card border-primary">
      <div class="modal-header theme-border">
        <h5 class="theme-text mb-0">Quick Add Task</h5>
        <button class="close-btn theme-text-muted" @click="close">
          <i class="bi bi-x-lg"></i>
        </button>
      </div>
      <div class="modal-body p-4">
        <div class="row g-3">
          <div class="col-12">
            <label class="form-label theme-text-muted small text-uppercase fw-bold">Title <span class="text-danger">*</span></label>
            <input v-if="!isMultiple" v-model="form.title" type="text" class="form-control theme-input" :class="{ 'is-invalid': showValidation && !form.title.trim() }" placeholder="What needs to be done?" ref="titleInput" @keydown.enter="submit(false)">
            <div v-else class="form-control theme-input bg-opacity-10 d-flex align-items-center justify-content-between">
              <span class="text-info fw-bold">{{ form.titles.length > 1 ? 'Multiple (' + form.titles.length + ' tasks)' : form.titles[0] }}</span>
              <button class="btn btn-sm btn-link p-0 text-muted" @click="form.titles = []">Clear</button>
            </div>
          </div>

          <div class="col-md-6">
            <label class="form-label theme-text-muted small text-uppercase fw-bold">Project <span class="text-danger">*</span></label>
            <div class="dropdown custom-dropdown" ref="projectDropdown">
              <button class="btn theme-input w-100 text-start d-flex align-items-center justify-content-between" :class="{ 'is-invalid': showValidation && !form.projectId }" type="button" data-bs-toggle="dropdown" data-bs-boundary="viewport">
                <div v-if="selectedProject" class="d-flex align-items-center overflow-hidden">
                  <div class="project-icon-wrapper mini" :style="{ backgroundColor: selectedProject.color }">
                    <i :class="getProjectIconClass(selectedProject)"></i>
                  </div>
                  <span class="text-truncate">{{ selectedProject.name }}</span>
                </div>
                <span v-else class="text-muted">Select Project...</span>
                <i class="bi bi-chevron-down small opacity-50 ms-2"></i>
              </button>
              <ul class="dropdown-menu dropdown-menu-dark w-100 shadow border-primary custom-dropdown-menu">
                <li v-for="p in projects" :key="p.id">
                  <a class="dropdown-item d-flex align-items-center py-2" href="#" @click.prevent="selectProject(p)">
                    <div class="project-icon-wrapper mini" :style="{ backgroundColor: p.color }">
                      <i :class="getProjectIconClass(p)"></i>
                    </div>
                    <span>{{ p.name }}</span>
                  </a>
                </li>
              </ul>
            </div>
          </div>
          
          <div class="col-md-6">
            <label class="form-label theme-text-muted small text-uppercase fw-bold">List <span class="text-danger">*</span></label>
            <div class="dropdown custom-dropdown" ref="listDropdown">
              <button class="btn theme-input w-100 text-start d-flex align-items-center justify-content-between" :class="{ 'is-invalid': showValidation && !form.listId }" type="button" data-bs-toggle="dropdown" :disabled="!form.projectId" data-bs-boundary="viewport">
                <div v-if="selectedList" class="d-flex align-items-center overflow-hidden">
                  <i class="bi bi-list-task me-2 text-muted"></i>
                  <span class="text-truncate">{{ selectedList.name }}</span>
                </div>
                <span v-else class="text-muted">Select List...</span>
                <i class="bi bi-chevron-down small opacity-50 ms-2"></i>
              </button>
              <ul class="dropdown-menu dropdown-menu-dark w-100 shadow border-primary custom-dropdown-menu">
                <li v-for="l in availableLists" :key="l.id">
                  <a class="dropdown-item d-flex align-items-center py-2" href="#" @click.prevent="selectList(l)">
                    <i class="bi bi-list-task me-2 text-muted"></i>
                    <span>{{ l.name }}</span>
                  </a>
                </li>
              </ul>
            </div>
          </div>

          <div class="col-md-6">
            <label class="form-label theme-text-muted small text-uppercase fw-bold">Status</label>
            <div class="dropdown custom-dropdown" ref="statusDropdown">
              <button class="btn theme-input w-100 text-start d-flex align-items-center justify-content-between" type="button" data-bs-toggle="dropdown" :disabled="!form.projectId" data-bs-boundary="viewport">
                <div v-if="selectedStatus" class="d-flex align-items-center overflow-hidden">
                  <span class="text-truncate">{{ selectedStatus.name }}</span>
                </div>
                <span v-else class="text-muted">Select Status...</span>
                <i class="bi bi-chevron-down small opacity-50 ms-2"></i>
              </button>
              <ul class="dropdown-menu dropdown-menu-dark w-100 shadow border-primary custom-dropdown-menu">
                <li v-for="s in availableStatuses" :key="s.id">
                  <a class="dropdown-item d-flex align-items-center py-2" href="#" @click.prevent="selectStatus(s)">
                    <span>{{ s.name }}</span>
                  </a>
                </li>
              </ul>
            </div>
          </div>

          <div class="col-md-6">
            <label class="form-label theme-text-muted small text-uppercase fw-bold">Priority</label>
            <div class="dropdown custom-dropdown" ref="priorityDropdown">
              <button class="btn theme-input w-100 text-start d-flex align-items-center justify-content-between" type="button" data-bs-toggle="dropdown" :disabled="!form.projectId" data-bs-boundary="viewport">
                <div v-if="selectedPriority" class="d-flex align-items-center overflow-hidden">
                  <i v-if="selectedPriority.icon" :class="selectedPriority.icon" :style="{ color: selectedPriority.color }" class="me-2"></i>
                  <span class="text-truncate">{{ selectedPriority.name }}</span>
                </div>
                <span v-else class="text-muted">Select Priority...</span>
                <i class="bi bi-chevron-down small opacity-50 ms-2"></i>
              </button>
              <ul class="dropdown-menu dropdown-menu-dark w-100 shadow border-primary custom-dropdown-menu">
                <li v-for="p in availablePriorities" :key="p.id">
                  <a class="dropdown-item d-flex align-items-center py-2" href="#" @click.prevent="selectPriority(p)">
                    <i v-if="p.icon" :class="p.icon" :style="{ color: p.color }" class="me-2"></i>
                    <span>{{ p.name }}</span>
                  </a>
                </li>
              </ul>
            </div>
          </div>

          <div class="col-12">
            <label class="form-label theme-text-muted small text-uppercase fw-bold">Parent Task (Optional)</label>
            <div class="dropdown custom-dropdown parent-task-dropdown" ref="parentTaskDropdown">
              <button class="btn theme-input w-100 text-start d-flex align-items-center justify-content-between" type="button" data-bs-toggle="dropdown" :disabled="!form.listId" data-bs-boundary="viewport">
                <div v-if="selectedParentTask" class="d-flex align-items-center overflow-hidden">
                  <i class="bi bi-arrow-return-right me-2 text-muted"></i>
                  <span class="text-truncate">{{ selectedParentTask.title }}</span>
                </div>
                <span v-else class="text-muted">None (Top-level task)</span>
                <i class="bi bi-chevron-down small opacity-50 ms-2"></i>
              </button>
              <ul class="dropdown-menu dropdown-menu-dark w-100 shadow border-primary custom-dropdown-menu" style="max-height: 250px; overflow-y: auto;" data-bs-boundary="viewport">
                <li>
                  <a class="dropdown-item d-flex align-items-center py-2" href="#" @click.prevent="selectParentTask(null)">
                    <span class="small">None (Top-level task)</span>
                  </a>
                </li>
                <li v-if="availableTasks.length > 0"><hr class="dropdown-divider border-primary opacity-25"></li>
                <li v-for="t in availableTasks" :key="t.id">
                  <a class="dropdown-item d-flex align-items-center py-2" href="#" @click.prevent="selectParentTask(t)">
                    <i class="bi bi-arrow-return-right me-2 text-muted small"></i>
                    <span class="small">{{ t.title }}</span>
                  </a>
                </li>
              </ul>
            </div>
          </div>

          <div class="col-md-6">
            <label class="form-label theme-text-muted small text-uppercase fw-bold">Start Date</label>
            <date-time-selector v-model="form.start" placeholder="No start date"></date-time-selector>
          </div>

          <div class="col-md-6">
            <label class="form-label theme-text-muted small text-uppercase fw-bold">End Date</label>
            <date-time-selector v-model="form.end" placeholder="No end date"></date-time-selector>
          </div>

          <div class="col-md-6">
            <label class="form-label theme-text-muted small text-uppercase fw-bold">Estimate (min)</label>
            <input v-model.number="form.estimateMinutes" type="number" class="form-control theme-input" placeholder="0">
          </div>

        </div>
      </div>
      <div class="modal-footer theme-border p-3 d-flex align-items-center">
        <button class="btn btn-subtle" @click="close">Cancel</button>
        <div class="ms-auto d-flex gap-2">
          <button class="btn btn-outline-primary" @click="submit(true)" :disabled="!isFormValid">Create Another</button>
          <button class="btn btn-primary px-4" @click="submit(false)" :disabled="!isFormValid">
            {{ isMultiple ? 'Add Tasks' : 'Add Task' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted, onBeforeUnmount } from 'vue';
import { addTask, addSubtask } from '../js/tasks-api';
import DateTimeSelector from './DateTimeSelector.vue';
import { showToast } from './Toast.vue';

const props = defineProps({
  isOpen: Boolean
});

const emit = defineEmits(['close', 'taskAdded', 'update:isOpen']);

const projects = ref([]);
const titleInput = ref(null);
const showValidation = ref(false);

const projectDropdown = ref(null);
const listDropdown = ref(null);
const parentTaskDropdown = ref(null);
const statusDropdown = ref(null);
const priorityDropdown = ref(null);

const form = reactive({
  title: '',
  titles: [],
  projectId: null,
  listId: null,
  parentId: null,
  statusId: null,
  priorityId: null,
  taskTypeId: null,
  start: null,
  end: null,
  estimateMinutes: 0
});

const isMultiple = computed(() => form.titles && form.titles.length > 0);

const selectedProject = computed(() => {
  return projects.value.find(p => p.id === form.projectId) || null;
});

const selectedList = computed(() => {
  if (!selectedProject.value) return null;
  return selectedProject.value.lists.find(l => l.id === form.listId) || null;
});

const selectedParentTask = computed(() => {
  if (!form.parentId || !selectedProject.value) return null;
  // Look through lists to find the task
  for (const list of selectedProject.value.lists) {
    const task = list.tasks.find(t => t.id === form.parentId);
    if (task) return task;
  }
  return null;
});

const selectedStatus = computed(() => {
  if (!selectedProject.value) return null;
  return selectedProject.value.statuses.find(s => s.id === form.statusId) || null;
});

const selectedPriority = computed(() => {
  if (!selectedProject.value) return null;
  return selectedProject.value.priorities.find(p => p.id === form.priorityId) || null;
});

const availableLists = computed(() => {
  return selectedProject.value ? selectedProject.value.lists : [];
});

const availableStatuses = computed(() => {
  return selectedProject.value ? selectedProject.value.statuses : [];
});

const availablePriorities = computed(() => {
  return selectedProject.value ? selectedProject.value.priorities : [];
});

const availableTasks = computed(() => {
  if (!form.listId || !selectedProject.value) return [];
  const list = selectedProject.value.lists.find(l => l.id === form.listId);
  if (!list) return [];
  
  return list.tasks || [];
});

const getProjectIconClass = (project) => {
  if (!project) return 'bi bi-folder';
  const icon = project.icon || 'bi-folder';
  return icon.startsWith('bi-') ? `bi ${icon}` : `bi bi-${icon}`;
};

const closeDropdown = (dropdownRef) => {
  if (!dropdownRef) return;
  const toggle = dropdownRef.querySelector('[data-bs-toggle="dropdown"]');
  if (toggle && window.bootstrap && window.bootstrap.Dropdown) {
    const dropdown = window.bootstrap.Dropdown.getInstance(toggle) || new window.bootstrap.Dropdown(toggle);
    dropdown.hide();
  }
};

const selectProject = (project) => {
  form.projectId = project.id;
  onProjectChange();
  closeDropdown(projectDropdown.value);
};

const selectList = (list) => {
  form.listId = list.id;
  onListChange();
  closeDropdown(listDropdown.value);
};

const selectParentTask = (task) => {
  form.parentId = task ? task.id : null;
  closeDropdown(parentTaskDropdown.value);
};

const selectStatus = (status) => {
  form.statusId = status.id;
  closeDropdown(statusDropdown.value);
};

const selectPriority = (priority) => {
  form.priorityId = priority.id;
  closeDropdown(priorityDropdown.value);
};

const isFormValid = computed(() => {
  if (isMultiple.value) return form.projectId && form.listId;
  return form.title.trim() && form.projectId && form.listId;
});

const fetchProjects = async () => {
  try {
    const response = await fetch('/api/projects');
    if (response.ok) {
      projects.value = await response.json();
    }
  } catch (error) {
    console.error('Error fetching projects:', error);
  }
};

const onProjectChange = () => {
  form.listId = null;
  form.parentId = null;
  
  const project = projects.value.find(p => p.id === form.projectId);
  if (project) {
    if (project.lists.length > 0) {
      form.listId = project.lists[0].id;
      onListChange();
    }
    
    // Set default status and priority
    const defaultStatus = project.statuses.find(s => s.isDefault) || project.statuses[0];
    if (defaultStatus) form.statusId = defaultStatus.id;
    
    const defaultPriority = project.priorities[0]; // Usually first is normal?
    if (defaultPriority) form.priorityId = defaultPriority.id;
  }
};

const onListChange = () => {
  form.parentId = null;
};

const submit = async (keepOpen = false) => {
  showValidation.value = true;
  if (!isFormValid.value) return;
  
  try {
    const taskData = {
      start: form.start,
      end: form.end,
      estimateMinutes: form.estimateMinutes
    };

    const titlesToCreate = isMultiple.value ? form.titles : [form.title];

    for (const title of titlesToCreate) {
      if (!title.trim()) continue;
      
      if (form.parentId) {
        await addSubtask(form.parentId, title, form.statusId, form.priorityId, taskData);
      } else {
        await addTask(form.listId, title, form.statusId, form.priorityId, taskData);
      }
    }
    
    // Remember last project, list, and parent task
    localStorage.setItem('lastProjectId', form.projectId);
    localStorage.setItem('lastListId', form.listId);
    if (form.parentId) {
      localStorage.setItem('lastParentId', form.parentId);
    } else {
      localStorage.removeItem('lastParentId');
    }
    
    emit('taskAdded');
    window.dispatchEvent(new CustomEvent('task-added'));
    
    const message = titlesToCreate.length > 1 
      ? `Successfully created ${titlesToCreate.length} tasks` 
      : 'Task created successfully';
    showToast(message, 'success');
    
    if (keepOpen) {
      form.title = '';
      form.titles = [];
      showValidation.value = false;
      setTimeout(() => {
        if (titleInput.value) titleInput.value.focus();
      }, 100);
    } else {
      close();
    }
  } catch (error) {
    console.error('Error adding task:', error);
  }
};

const close = () => {
  emit('close');
};

const handleKeyDown = (e) => {
  if (e.key === 'Escape' && props.isOpen) {
    close();
  }
};

const handleOpenEvent = (e) => {
  if (e.detail) {
    const { title, titles, projectId, listId, parentId, statusId, priorityId, start, end, estimateMinutes } = e.detail;
    
    // If the modal isn't open, open it first
    if (!props.isOpen) {
      emit('update:isOpen', true);
      // Wait a bit for the watch isOpen to run and fetch projects
    }

    // Use a small timeout to ensure fetchProjects completes if needed, 
    // although fetchProjects is called in watch isOpen
    setTimeout(async () => {
      // Clear any existing titles before setting new ones
      form.title = '';
      form.titles = [];
      
      if (title !== undefined) form.title = title;
      if (titles !== undefined && Array.isArray(titles)) form.titles = titles;
      if (projectId !== undefined) {
        form.projectId = projectId;
        // Trigger project change logic to populate lists/status/priority
        onProjectChange();
        
        // Then override with specific values if provided
        if (listId !== undefined) {
          form.listId = listId;
          onListChange();
        }
        if (parentId !== undefined) form.parentId = parentId;
        if (statusId !== undefined) form.statusId = statusId;
        if (priorityId !== undefined) form.priorityId = priorityId;
      }
      
      if (start !== undefined) form.start = start;
      if (end !== undefined) form.end = end;
      if (estimateMinutes !== undefined) form.estimateMinutes = estimateMinutes;

      // If prefilled data makes the form valid, don't show validation errors immediately
      if (form.title && form.projectId && form.listId) {
        showValidation.value = false;
      }
      
      if (titleInput.value) titleInput.value.focus();
    }, 100);
  }
};

watch(() => props.isOpen, async (newVal) => {
  if (newVal) {
    showValidation.value = false;
    await fetchProjects();
    
    // Reset form fields
    form.title = '';
    form.titles = [];
    form.start = null;
    form.end = null;
    form.estimateMinutes = 0;
    form.parentId = null;

    // Load last used project, list, and parent task
    const lastProjectId = localStorage.getItem('lastProjectId');
    const lastListId = localStorage.getItem('lastListId');
    const lastParentId = localStorage.getItem('lastParentId');

    if (lastProjectId) {
      const project = projects.value.find(p => p.id === lastProjectId);
      if (project) {
        form.projectId = lastProjectId;
        
        // Load default status/priority for the project
        const defaultStatus = project.statuses.find(s => s.isDefault) || project.statuses[0];
        if (defaultStatus) form.statusId = defaultStatus.id;
        
        const defaultPriority = project.priorities[0];
        if (defaultPriority) form.priorityId = defaultPriority.id;

        if (lastListId) {
          const list = project.lists.find(l => l.id === lastListId);
          if (list) {
            form.listId = lastListId;
            
            // Prefill parent task if it belongs to this list
            if (lastParentId) {
              const parentTask = list.tasks.find(t => t.id === lastParentId);
              if (parentTask) {
                form.parentId = lastParentId;
              }
            }
          } else if (project.lists.length > 0) {
            form.listId = project.lists[0].id;
          }
        } else if (project.lists.length > 0) {
          form.listId = project.lists[0].id;
        }

        // IMPORTANT: Ensure showValidation remains false if we prefilled valid data
        if (form.title && form.projectId && form.listId) {
          showValidation.value = false;
        }
      }
    }
    
    setTimeout(() => {
      if (titleInput.value) titleInput.value.focus();
    }, 100);
  }
});

onMounted(() => {
  window.addEventListener('keydown', handleKeyDown);
  window.addEventListener('open-add-task-modal', handleOpenEvent);
});

onBeforeUnmount(() => {
  window.removeEventListener('keydown', handleKeyDown);
  window.removeEventListener('open-add-task-modal', handleOpenEvent);
});
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.7);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1100;
  backdrop-filter: blur(2px);
}

.add-task-modal {
  width: 95%;
  max-width: 650px;
  background-color: var(--bg-dark);
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5);
  border: 1px solid var(--border-primary);
  display: flex;
  flex-direction: column;
}

.modal-body {
  flex: 1;
}


.modal-header, .modal-footer {
  padding: 1rem 1.5rem;
  background-color: rgba(255, 255, 255, 0.02);
  flex-shrink: 0;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid var(--border-primary);
  border-top-left-radius: 12px;
  border-top-right-radius: 12px;
  overflow: hidden;
}

.modal-footer {
  border-top: 1px solid var(--border-primary);
  border-bottom-left-radius: 12px;
  border-bottom-right-radius: 12px;
  overflow: hidden;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  padding: 0;
  line-height: 1;
  transition: color 0.2s;
}

.close-btn:hover {
  color: var(--text-primary) !important;
}

.theme-input {
  background-color: rgba(255, 255, 255, 0.05);
  border: 1px solid var(--border-primary);
  color: var(--text-primary);
}

.theme-input:focus {
  background-color: rgba(255, 255, 255, 0.08);
  border-color: var(--accent-blue);
  box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
  color: var(--text-primary);
}

.theme-input:disabled {
  background-color: rgba(255, 255, 255, 0.02);
  opacity: 0.6;
}

.theme-input.is-invalid {
  border-color: #ff4444 !important;
  box-shadow: 0 0 0 1px #ff4444 !important;
}

.theme-input.is-invalid:focus {
  box-shadow: 0 0 0 0.25rem rgba(255, 68, 68, 0.25) !important;
}

.custom-dropdown .btn {
  border-radius: 6px;
  padding: 0.5rem 0.75rem;
  transition: all 0.2s;
}

.custom-dropdown .btn:after {
  display: none;
}

.custom-dropdown-menu {
  padding: 0.5rem;
  border-radius: 8px;
  margin-top: 5px !important;
  z-index: 1060;
  min-width: 100%;
}

.parent-task-dropdown .custom-dropdown-menu {
  width: max-content;
  min-width: 100%;
  max-width: 610px; /* 650px modal - padding */
  overflow-x: auto;
}

.datetime-selector {
  width: 100%;
}

:deep(.display-value) {
  background-color: rgba(255, 255, 255, 0.05);
  border: 1px solid var(--border-primary);
  border-radius: 6px;
  padding: 0.5rem 0.75rem;
  color: var(--text-primary);
  cursor: pointer;
  height: 38px;
  display: flex;
  align-items: center;
}

.custom-dropdown-menu .dropdown-item {
  border-radius: 4px;
  transition: background-color 0.15s;
}

.project-icon-wrapper.mini {
  width: 20px;
  height: 20px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 8px;
  flex-shrink: 0;
}

.project-icon-wrapper.mini i {
  color: white;
  font-size: 0.75rem;
}

.btn-subtle {
  background: transparent;
  border: 1px solid transparent;
  color: var(--text-muted);
  padding: 6px 16px;
  border-radius: 4px;
  font-size: 0.9rem;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  cursor: pointer;
}

.btn-subtle:hover {
  background-color: var(--bg-card);
  color: var(--text-primary);
  border-color: var(--border-primary);
}

.custom-dropdown-menu::-webkit-scrollbar {
  width: 6px;
}

.custom-dropdown-menu::-webkit-scrollbar-track {
  background: transparent;
}

.custom-dropdown-menu::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 3px;
}

.custom-dropdown-menu::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.2);
}
</style>
