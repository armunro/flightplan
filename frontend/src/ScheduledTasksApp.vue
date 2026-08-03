<template>
  <div :class="['vh-100 d-flex flex-row overflow-hidden', themeClass]">
    <Navbar />
    <div class="flex-grow-1 overflow-hidden d-flex flex-column">
      <div id="app-content" class="scheduled-tasks-app-container flex-grow-1">
        <div class="tasks-sidebar d-flex flex-column" :class="{ collapsed: sidebarCollapsed }" :style="sidebarStyle">
          <div class="sidebar-header d-flex align-items-center" :class="{ 'collapsed': sidebarCollapsed }">
            <h5 v-if="!sidebarCollapsed">Schedules</h5>
            <button 
              v-if="!sidebarCollapsed"
              class="btn-icon ms-auto" 
              @click="selectedTaskId = null" 
              title="New Schedule"
            >
              <i class="bi bi-plus-lg"></i>
            </button>
            <i v-else class="bi bi-clock-history"></i>
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
          <div class="sidebar-footer" :class="{ 'collapsed': sidebarCollapsed }">
            <button class="sidebar-toggle" @click="sidebarCollapsed = !sidebarCollapsed" :title="sidebarCollapsed ? 'Expand Menu' : 'Collapse Menu'">
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
               <button v-if="selectedTaskId" class="btn btn-sm theme-btn-outline" @click="confirmDelete" title="Delete Schedule">
                 <i class="bi bi-trash"></i>
               </button>
               <button class="btn btn-primary btn-sm px-3" @click="saveTask" :disabled="!isFormValid">
                 <i class="bi bi-check-lg me-1"></i> Save Schedule
               </button>
             </div>
          </div>

          <div class="project-content">
            <div class="row">
              <div class="col-md-6">
                <div class="card theme-card mb-4">
                  <div class="card-header theme-border text-primary">
                    <h5 class="mb-0">Schedule Info</h5>
                  </div>
                  <div class="card-body">
                    <div class="mb-3">
                      <label class="form-label theme-text">Schedule Name</label>
                      <input v-model="form.name" type="text" class="form-control theme-input" placeholder="e.g. Daily Standup Prep">
                    </div>
                    <div class="mb-3">
                      <label class="form-label">Recurrence Type</label>
                      <div class="btn-group w-100" role="group">
                        <input type="radio" class="btn-check" name="recurrenceType" id="recurrenceCron" value="Cron" v-model="form.recurrenceType">
                        <label class="btn btn-outline-secondary" for="recurrenceCron">Cron Expression</label>
                        
                        <input type="radio" class="btn-check" name="recurrenceType" id="recurrenceCustom" value="Custom" v-model="form.recurrenceType">
                        <label class="btn btn-outline-secondary" for="recurrenceCustom">Custom Reoccurrence</label>
                      </div>
                    </div>

                    <div v-if="form.recurrenceType === 'Cron'" class="mb-3">
                      <label class="form-label theme-text">Cron Schedule</label>
                      <input v-model="form.cronSchedule" type="text" class="form-control theme-input" placeholder="0 0 9 ? * MON-FRI">
                      <CronEditor v-model="form.cronSchedule" />
                    </div>

                    <div v-if="form.recurrenceType === 'Custom'">
                      <div class="row mb-3">
                        <div class="col-6">
                          <label class="form-label theme-text">Start Date</label>
                          <input v-model="form.startDate" type="date" class="form-control theme-input">
                        </div>
                        <div class="col-6">
                          <label class="form-label theme-text">Start Time</label>
                          <input v-model="form.startTime" type="time" class="form-control theme-input">
                        </div>
                      </div>
                      <div class="row mb-3">
                        <div class="col-6">
                          <label class="form-label theme-text">Every X</label>
                          <input v-model.number="form.interval" type="number" min="1" class="form-control theme-input">
                        </div>
                        <div class="col-6">
                          <label class="form-label theme-text">Unit</label>
                          <select v-model="form.intervalUnit" class="form-select theme-input">
                            <option value="Days">Days</option>
                            <option value="Weeks">Weeks</option>
                            <option value="Months">Months</option>
                            <option value="Years">Years</option>
                          </select>
                        </div>
                      </div>
                    </div>

                    <div v-if="form.nextRun" class="mb-3 small text-accent">
                      Next Run: {{ formatDate(form.nextRun) }}
                    </div>
                    <div class="form-check form-switch mb-3">
                      <input v-model="form.isEnabled" class="form-check-input" type="checkbox" id="isEnabled">
                      <label class="form-check-label" for="isEnabled">Schedule Enabled</label>
                    </div>
                  </div>
                </div>
              </div>

              <div class="col-md-6">
                <div class="card theme-card mb-4">
                  <div class="card-header theme-border text-primary">
                    <h5 class="mb-0">Task Template</h5>
                  </div>
                  <div class="card-body">
                    <div class="mb-3">
                      <label class="form-label theme-text">Target Project</label>
                      <select v-model="form.projectId" class="form-select theme-input">
                        <option v-for="p in projects" :key="p.id" :value="p.id">{{ p.name }}</option>
                      </select>
                    </div>
                    <div class="mb-3">
                      <label class="form-label theme-text">Target List</label>
                      <select v-model="form.listId" class="form-select theme-input" :disabled="!selectedProject">
                        <option v-for="l in selectedProject?.lists" :key="l.id" :value="l.id">{{ l.name }}</option>
                      </select>
                    </div>
                    <div class="mb-3">
                      <label class="form-label theme-text">Task Title Template</label>
                      <input v-model="form.taskTitleTemplate" type="text" class="form-control theme-input" placeholder="e.g. Daily Standup - {{date}}">
                      <div class="form-text small theme-text-muted">Tip: Use <code>{{date}}</code> for current date</div>
                    </div>
                    <div class="mb-3">
                      <label class="form-label theme-text">Description</label>
                      <textarea v-model="form.taskDescription" class="form-control theme-input" rows="3"></textarea>
                    </div>
                    <div class="row">
                      <div class="col-md-6 mb-3">
                        <label class="form-label theme-text">Priority</label>
                        <select v-model="form.priority" class="form-select theme-input">
                          <option v-for="p in priorities" :key="p.value" :value="p.value">{{ p.label }}</option>
                        </select>
                      </div>
                      <div class="col-md-6 mb-3">
                        <label class="form-label theme-text">Status</label>
                        <select v-model="form.statusId" class="form-select theme-input" :disabled="!selectedProject">
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
import { showToast } from './components/Toast.vue';
import Navbar from './components/Navbar.vue';
import CronEditor from './components/CronEditor.vue';
import * as api from './js/scheduled-tasks-api';
import { fetchSettings } from './js/dashboard-api';

const scheduledTasks = ref([]);
const projects = ref([]);
const selectedTaskId = ref(null);
const sidebarCollapsed = ref(localStorage.getItem('scheduledTasksSidebarCollapsed') === 'true');
const sidebarWidth = ref(parseInt(localStorage.getItem('scheduledTasksSidebarWidth')) || 250);

const theme = ref('Cosmic');
const themeClass = computed(() => `theme-${theme.value.toLowerCase()}`);

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
  recurrenceType: 'Cron',
  startDate: new Date().toISOString().split('T')[0],
  startTime: '09:00',
  interval: 1,
  intervalUnit: 'Days',
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
  const basicValid = form.value.name && 
         form.value.projectId && 
         form.value.listId && 
         form.value.taskTitleTemplate;

  if (form.value.recurrenceType === 'Cron') {
    return basicValid && form.value.cronSchedule;
  } else {
    return basicValid && form.value.startDate && form.value.interval > 0;
  }
});

const sidebarStyle = computed(() => {
  return { 
    width: sidebarCollapsed.value ? '50px' : `${sidebarWidth.value}px`,
    transition: isResizingSidebar.value ? 'none' : 'width 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
  };
});

const fetchTasks = async () => {
  scheduledTasks.value = await api.getScheduledTasks();
};

const fetchProjects = async () => {
  projects.value = await api.getProjects();
};

const selectTask = (task) => {
  selectedTaskId.value = task.id;
  // Format dates for the date input (YYYY-MM-DD)
  const taskToSelect = { ...task };
  if (taskToSelect.startDate) {
    taskToSelect.startDate = new Date(taskToSelect.startDate).toISOString().split('T')[0];
  }
  form.value = taskToSelect;
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
    recurrenceType: 'Cron',
    startDate: new Date().toISOString().split('T')[0],
    startTime: '09:00',
    interval: 1,
    intervalUnit: 'Days',
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
  const taskToSave = { ...form.value };
  // Ensure startDate is correctly handled for the API
  if (taskToSave.startDate) {
    // If it's just a date string from input, convert to full ISO or let backend parse it
    // HTML date input gives YYYY-MM-DD which .NET should handle if mapped to DateTime?
  }
  
  if (selectedTaskId.value) {
    try {
      await api.updateScheduledTask(selectedTaskId.value, taskToSave);
      showToast('Scheduled task updated successfully');
    } catch (e) {
      showToast('Failed to update scheduled task', 'error');
    }
  } else {
    try {
      const newTask = await api.createScheduledTask({ ...taskToSave, id: '00000000-0000-0000-0000-000000000000' });
      if (newTask) {
        selectedTaskId.value = newTask.id;
        showToast('Scheduled task created successfully');
      }
    } catch (e) {
      showToast('Failed to create scheduled task', 'error');
    }
  }
  await fetchTasks();
  // If we just created, update the form with the one from server (which has ID and nextRun)
  if (selectedTaskId.value) {
    const updated = scheduledTasks.value.find(t => t.id === selectedTaskId.value);
    if (updated) selectTask(updated);
  }
};

const confirmDelete = async () => {
  if (confirm('Are you sure you want to delete this scheduled task?')) {
    try {
      await api.deleteScheduledTask(selectedTaskId.value);
      showToast('Scheduled task deleted');
      selectedTaskId.value = null;
      await fetchTasks();
    } catch (e) {
      showToast('Failed to delete scheduled task', 'error');
    }
  }
};

const formatDate = (date) => {
  if (!date) return '';
  return new Date(date).toLocaleString();
};

onMounted(async () => {
  await Promise.all([
    fetchProjects(),
    fetchTasks(),
    loadSettings()
  ]);
  if (scheduledTasks.value.length > 0) {
    selectTask(scheduledTasks.value[0]);
  } else if (projects.value.length > 0) {
    resetForm();
  }
});

const loadSettings = async () => {
  try {
    const settings = await fetchSettings();
    if (settings) {
      theme.value = settings.theme || 'Cosmic';
    }
  } catch (e) {
    console.error('Failed to load settings:', e);
  }
};

</script>

<style scoped>
@import './tasks-style.css';

.scheduled-tasks-app-container {
    display: flex;
    height: 100%;
    overflow: hidden;
}

.tasks-sidebar {
  width: 230px;
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

.main-content {
  flex-grow: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  background-color: var(--bg-darker);
}

.project-list {
  flex-grow: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 0.5rem 0;
}

.project-item {
  position: relative;
  padding: 8px 12px;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: background-color 0.2s;
  border-left: 3px solid transparent;
  min-width: 0;
  color: var(--text-primary);
  font-size: 0.9rem;
}

.tasks-sidebar.collapsed .project-item {
  padding: 10px 0;
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
  font-size: 0.9rem;
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
