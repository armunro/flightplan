<template>
  <div class="vh-100 d-flex flex-row overflow-hidden">
    <Navbar />
    <div class="flex-grow-1 overflow-hidden d-flex flex-column">
      <div id="app-content" class="scheduled-tasks-app-container flex-grow-1">
        <div class="tasks-sidebar" :class="{ collapsed: sidebarCollapsed }" :style="sidebarStyle">
          <div class="sidebar-header">
            <h5 v-if="!sidebarCollapsed">Schedules</h5>
            <button 
              class="btn-icon text-accent ms-auto" 
              @click="selectedTaskId = null" 
              title="New Schedule"
            >
              <i class="bi bi-plus-lg"></i>
            </button>
          </div>
          <div class="project-list">
            <div 
              v-for="task in scheduledTasks" 
              :key="task.id" 
              class="project-item" 
              :class="{ active: selectedTaskId === task.id }"
              @click="selectTask(task)"
              :title="sidebarCollapsed ? task.name : ''"
            >
              <i class="bi bi-clock-history me-2"></i>
              <span v-if="!sidebarCollapsed" class="project-name text-truncate">{{ task.name }}</span>
            </div>
          </div>
          <div class="sidebar-footer">
            <button class="btn-icon sidebar-toggle" @click="sidebarCollapsed = !sidebarCollapsed" :title="sidebarCollapsed ? 'Expand Menu' : 'Collapse Menu'">
              <i class="bi" :class="sidebarCollapsed ? 'bi-chevron-right' : 'bi-chevron-left'"></i>
            </button>
          </div>
        </div>

        <div v-if="!sidebarCollapsed" class="sidebar-resizer" @mousedown="startSidebarResize"></div>

        <div class="main-content">
          <div class="controls-bar">
             <div class="project-title-area">
               <h2>{{ selectedTaskId ? 'Edit Scheduled Task' : 'Create New Scheduled Task' }}</h2>
             </div>
             <div class="d-flex gap-2">
               <button v-if="selectedTaskId" class="btn btn-danger btn-sm" @click="confirmDelete">
                 <i class="bi bi-trash me-1"></i> Delete
               </button>
               <button class="btn btn-primary btn-sm" @click="saveTask" :disabled="!isFormValid">
                 <i class="bi bi-check-lg me-1"></i> Save Schedule
               </button>
             </div>
          </div>

          <div class="project-content">
            <div class="row">
              <div class="col-md-6">
                <div class="card bg-dark border-secondary mb-4">
                  <div class="card-header border-secondary text-primary">
                    <h5 class="mb-0">Schedule Info</h5>
                  </div>
                  <div class="card-body">
                    <div class="mb-3">
                      <label class="form-label">Schedule Name</label>
                      <input v-model="form.name" type="text" class="form-control bg-dark text-light border-secondary" placeholder="e.g. Daily Standup Prep">
                    </div>
                    <div class="mb-3">
                      <label class="form-label">Cron Schedule</label>
                      <input v-model="form.cronSchedule" type="text" class="form-control bg-dark text-light border-secondary" placeholder="0 0 9 ? * MON-FRI">
                      <CronEditor v-model="form.cronSchedule" />
                      <div v-if="form.nextRun" class="mt-2 small text-accent">
                        Next Run: {{ formatDate(form.nextRun) }}
                      </div>
                    </div>
                    <div class="form-check form-switch mb-3">
                      <input v-model="form.isEnabled" class="form-check-input" type="checkbox" id="isEnabled">
                      <label class="form-check-label" for="isEnabled">Schedule Enabled</label>
                    </div>
                  </div>
                </div>
              </div>

              <div class="col-md-6">
                <div class="card bg-dark border-secondary mb-4">
                  <div class="card-header border-secondary text-primary">
                    <h5 class="mb-0">Task Template</h5>
                  </div>
                  <div class="card-body">
                    <div class="mb-3">
                      <label class="form-label">Target Project</label>
                      <select v-model="form.projectId" class="form-select bg-dark text-light border-secondary">
                        <option v-for="p in projects" :key="p.id" :value="p.id">{{ p.name }}</option>
                      </select>
                    </div>
                    <div class="mb-3">
                      <label class="form-label">Target List</label>
                      <select v-model="form.listId" class="form-select bg-dark text-light border-secondary" :disabled="!selectedProject">
                        <option v-for="l in selectedProject?.lists" :key="l.id" :value="l.id">{{ l.name }}</option>
                      </select>
                    </div>
                    <div class="mb-3">
                      <label class="form-label">Task Title Template</label>
                      <input v-model="form.taskTitleTemplate" type="text" class="form-control bg-dark text-light border-secondary" placeholder="e.g. Daily Standup - {{date}}">
                      <div class="form-text small opacity-75">Tip: Use <code>{{date}}</code> for current date</div>
                    </div>
                    <div class="mb-3">
                      <label class="form-label">Description</label>
                      <textarea v-model="form.taskDescription" class="form-control bg-dark text-light border-secondary" rows="3"></textarea>
                    </div>
                    <div class="row">
                      <div class="col-md-6 mb-3">
                        <label class="form-label">Priority</label>
                        <select v-model="form.priority" class="form-select bg-dark text-light border-secondary">
                          <option v-for="p in priorities" :key="p.value" :value="p.value">{{ p.label }}</option>
                        </select>
                      </div>
                      <div class="col-md-6 mb-3">
                        <label class="form-label">Status</label>
                        <select v-model="form.statusId" class="form-select bg-dark text-light border-secondary" :disabled="!selectedProject">
                          <option :value="null">Default</option>
                          <option v-for="s in selectedProject?.statuses" :key="s.id" :value="s.id">{{ s.name }}</option>
                        </select>
                      </div>
                    </div>
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
import CronEditor from './components/CronEditor.vue';
import * as api from './js/scheduled-tasks-api';

const scheduledTasks = ref([]);
const projects = ref([]);
const selectedTaskId = ref(null);
const sidebarCollapsed = ref(localStorage.getItem('scheduledTasksSidebarCollapsed') === 'true');
const sidebarWidth = ref(parseInt(localStorage.getItem('scheduledTasksSidebarWidth')) || 250);

const isResizingSidebar = ref(false);
let sidebarStartWidth = 0;
let sidebarStartX = 0;

const startSidebarResize = (e) => {
  isResizingSidebar.value = true;
  sidebarStartWidth = sidebarWidth.value;
  sidebarStartX = e.clientX;
  document.addEventListener('mousemove', doSidebarResize);
  document.addEventListener('mouseup', stopSidebarResize);
  document.body.style.cursor = 'col-resize';
  document.body.style.userSelect = 'none';
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
  localStorage.setItem('scheduledTasksSidebarWidth', sidebarWidth.value.toString());
};

watch(sidebarCollapsed, (newVal) => {
  localStorage.setItem('scheduledTasksSidebarCollapsed', newVal.toString());
});

const form = ref({
  id: null,
  name: '',
  cronSchedule: '0 0 9 ? * MON-FRI',
  isEnabled: true,
  projectId: '',
  listId: '',
  taskTitleTemplate: '',
  taskDescription: '',
  priority: 2,
  statusId: null,
  taskTypeId: null,
  nextRun: null
});

/* legacy priorities */
const priorities = [
  { value: 0, label: 'Lowest' },
  { value: 1, label: 'Low' },
  { value: 2, label: 'Medium' },
  { value: 3, label: 'High' },
  { value: 4, label: 'Highest' },
  { value: 5, label: 'Critical' }
];

const selectedProject = computed(() => {
  return projects.value.find(p => p.id === form.value.projectId);
});

const isFormValid = computed(() => {
  return form.value.name && 
         form.value.cronSchedule && 
         form.value.projectId && 
         form.value.listId && 
         form.value.taskTitleTemplate;
});

const sidebarStyle = computed(() => {
  return { width: sidebarCollapsed.value ? '50px' : `${sidebarWidth.value}px` };
});

const fetchTasks = async () => {
  scheduledTasks.value = await api.getScheduledTasks();
};

const fetchProjects = async () => {
  projects.value = await api.getProjects();
};

const selectTask = (task) => {
  selectedTaskId.value = task.id;
  form.value = { ...task };
};

watch(selectedTaskId, (newId) => {
  if (!newId) {
    resetForm();
  }
});

const resetForm = () => {
  form.value = {
    id: null,
    name: '',
    cronSchedule: '0 0 9 ? * MON-FRI',
    isEnabled: true,
    projectId: projects.value[0]?.id || '',
    listId: projects.value[0]?.lists[0]?.id || '',
    taskTitleTemplate: '',
    taskDescription: '',
    priority: 2,
    statusId: null,
    taskTypeId: null,
    nextRun: null
  };
};

const saveTask = async () => {
  if (selectedTaskId.value) {
    await api.updateScheduledTask(selectedTaskId.value, form.value);
  } else {
    const newTask = await api.createScheduledTask({ ...form.value, id: '00000000-0000-0000-0000-000000000000' });
    if (newTask) selectedTaskId.value = newTask.id;
  }
  await fetchTasks();
  // If we just created, update the form with the one from server (which has ID and nextRun)
  if (selectedTaskId.value) {
    const updated = scheduledTasks.value.find(t => t.id === selectedTaskId.value);
    if (updated) form.value = { ...updated };
  }
};

const confirmDelete = async () => {
  if (confirm('Are you sure you want to delete this scheduled task?')) {
    await api.deleteScheduledTask(selectedTaskId.value);
    selectedTaskId.value = null;
    await fetchTasks();
  }
};

const formatDate = (date) => {
  if (!date) return '';
  return new Date(date).toLocaleString();
};

onMounted(async () => {
  await fetchProjects();
  await fetchTasks();
  if (scheduledTasks.value.length > 0) {
    selectTask(scheduledTasks.value[0]);
  } else if (projects.value.length > 0) {
    resetForm();
  }
});

</script>

<style scoped>
@import './tasks-style.css';

.scheduled-tasks-app-container {
    display: flex;
    height: 100vh;
    overflow: hidden;
}

.tasks-sidebar {
  background-color: var(--bg-dark);
  border-right: 1px solid var(--border-primary);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  flex-shrink: 0;
  z-index: 1;
  transition: width 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.tasks-sidebar.collapsed {
  width: 50px !important;
}

.sidebar-header {
  padding: 0 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid var(--border-primary);
  height: 60px;
  flex-shrink: 0;
  background-color: var(--bg-dark);
  box-sizing: border-box;
}

.tasks-sidebar.collapsed .sidebar-header {
  padding: 0;
  justify-content: center;
}

.sidebar-header h5 {
  margin: 0;
  font-weight: 600;
  white-space: nowrap;
  color: var(--text-primary);
}

.project-list {
  flex-grow: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 0.5rem 0;
}

.project-item {
  position: relative;
  padding: 0.75rem 1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: background-color 0.2s;
  border-left: 3px solid transparent;
  min-width: 0;
  color: var(--text-primary);
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

.project-item .bi {
  color: var(--accent-blue);
  flex-shrink: 0;
}

.project-name {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  font-size: 0.95rem;
}

.sidebar-resizer {
  width: 4px;
  cursor: col-resize;
  background-color: transparent;
  transition: background-color 0.2s;
  z-index: 10;
  margin-left: -2px;
  margin-right: -2px;
  flex-shrink: 0;
}

.sidebar-resizer:hover, .sidebar-resizer:active {
  background-color: var(--accent-blue);
}

.sidebar-footer {
  padding: 0.5rem;
  display: flex;
  justify-content: flex-end;
  background-color: var(--bg-dark);
  flex-shrink: 0;
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

.sidebar-toggle:focus {
  outline: none;
  box-shadow: none;
}

.main-content {
  flex-grow: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.text-accent {
  color: var(--accent-blue);
}

.form-text {
  font-size: 0.8rem;
}

.card {
  box-shadow: 0 4px 6px rgba(0,0,0,0.1);
}

.card-header {
  background-color: rgba(255,255,255,0.03);
  font-weight: 600;
}

.cron-help-box {
  background-color: rgba(0,0,0,0.2);
  color: #c9d1d9;
}
</style>
