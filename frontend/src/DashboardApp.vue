<template>
  <div class="dashboard-app">
    <Navbar />
    <div class="main-content">
      <div class="controls-bar">
        <div class="d-flex align-items-center">
          <h5 class="mb-0 me-3 text-primary">Dashboard</h5>
          <span class="text-muted small">{{ todayDate }}</span>
        </div>
        <div class="ms-auto">
          <button class="btn btn-secondary btn-sm" @click="loadData" :disabled="loading">
            <i class="bi bi-arrow-clockwise" :class="{ 'spin': loading }"></i> Refresh
          </button>
        </div>
      </div>

      <div class="content-body">
        <div v-if="loading && !data" class="d-flex justify-content-center mt-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <div v-else-if="data" class="dashboard-grid h-100 overflow-hidden d-flex">
          <!-- Calendar Section -->
          <div v-if="data.calendarVisible" class="dashboard-pane d-flex flex-column h-100" :style="paneStyles.calendar">
            <div class="pane-header d-flex align-items-center flex-shrink-0 px-3">
              <i class="bi bi-calendar3 me-2 text-info"></i>
              <h6 class="mb-0 text-primary fs-base">Today's Events</h6>
              <span class="badge bg-secondary ms-auto text-light small">{{ data.todaysEvents.length }}</span>
            </div>
            <div class="pane-body flex-grow-1 overflow-auto">
              <div v-if="data.todaysEvents.length === 0" class="py-3 px-4 text-center text-muted small">
                No events scheduled for today.
              </div>
              <div v-else class="list-group list-group-flush">
                <div v-for="event in data.todaysEvents" :key="event.id" class="list-group-item bg-transparent border-secondary py-3">
                  <div class="d-flex justify-content-between align-items-start">
                    <div class="event-details">
                      <div class="fw-bold fs-sm text-primary">{{ event.subject }}</div>
                      <div class="text-muted fs-xs mt-1">
                        <i class="bi bi-clock me-1"></i>
                        {{ formatTime(event.start) }} - {{ formatTime(event.end) }}
                        <span v-if="event.location" class="ms-2">
                          <i class="bi bi-geo-alt me-1"></i>{{ event.location }}
                        </span>
                      </div>
                    </div>
                    <a v-if="event.webLink" :href="event.webLink" target="_blank" class="btn btn-link btn-sm p-0 text-accent-blue">
                      <i class="bi bi-box-arrow-up-right"></i>
                    </a>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Resizer 1 -->
          <div v-if="showResizer1" class="content-resizer" @mousedown="startResize('resizer1', $event)"></div>

          <!-- Tasks Section -->
          <div class="dashboard-pane d-flex flex-column h-100" :style="paneStyles.tasks">
            <div class="pane-header d-flex align-items-center flex-shrink-0 px-3">
              <i class="bi bi-check2-square me-2 text-success"></i>
              <h6 class="mb-0 text-primary fs-base">Upcoming Deadlines</h6>
              <span class="badge bg-secondary ms-auto text-light small">{{ data.upcomingTasks.length }}</span>
            </div>
            <div class="pane-body flex-grow-1 overflow-auto">
              <div v-if="data.upcomingTasks.length === 0" class="py-3 px-4 text-center text-muted small">
                No upcoming deadlines.
              </div>
              <div v-else class="dashboard-tasks-grid">
                <div class="tasks-header-row" :style="tasksGridStyle">
                  <div class="tasks-header">Type</div>
                  <div class="tasks-header">Title</div>
                  <div class="tasks-header">Status</div>
                  <div class="tasks-header">Priority</div>
                  <div class="tasks-header">Due</div>
                  <div class="tasks-header">Est</div>
                </div>
                <div v-for="task in data.upcomingTasks" :key="task.id" class="tasks-row" :style="tasksGridStyle">
                  <div class="tasks-cell type-column">
                    <span class="type-badge" :style="{ color: task.typeColor || '#3498db' }">
                      <i :class="task.typeIcon || 'bi-briefcase'"></i>
                      <span class="ms-1">{{ task.typeName || 'Work' }}</span>
                    </span>
                  </div>
                  <div class="tasks-cell name-column">
                    <span class="task-title text-truncate" :title="task.title">{{ task.title }}</span>
                  </div>
                  <div class="tasks-cell">
                    <span class="status-badge" :style="{ color: task.statusColor || '#cccccc' }">
                      <i class="bi bi-circle-fill" style="font-size: 8px; margin-right: 4px;"></i>
                      <span>{{ task.statusName || 'Unknown' }}</span>
                    </span>
                  </div>
                  <div class="tasks-cell">
                    <span class="priority-badge" :style="{ color: task.priorityColor || '#ccc' }">
                      <i :class="task.priorityIcon || 'bi-dash-lg'" style="margin-right: 4px;"></i>
                      <span>{{ task.priorityName || '' }}</span>
                    </span>
                  </div>
                  <div class="tasks-cell date-cell">
                    <span :class="getDateColorClass(task.end, task.isCompleted)">
                      {{ formatTaskDate(task.end) }}
                    </span>
                  </div>
                  <div class="tasks-cell estimate-cell">
                    {{ formatEstimate(task.estimateMinutes) }}
                  </div>
                </div>
              </div>
            </div>
            <div class="pane-footer bg-transparent border-secondary flex-shrink-0 px-3 py-2 border-top">
              <a href="/Tasks" class="text-accent-blue small text-decoration-none">View all tasks <i class="bi bi-arrow-right"></i></a>
            </div>
          </div>

          <!-- Resizer 2 -->
          <div v-if="showResizer2" class="content-resizer" @mousedown="startResize('resizer2', $event)"></div>

          <!-- Email Section -->
          <div v-if="data.emailVisible" class="dashboard-pane d-flex flex-column h-100" :style="paneStyles.email">
            <div class="pane-header d-flex align-items-center flex-shrink-0 px-3">
              <i class="bi bi-envelope me-2 text-warning"></i>
              <h6 class="mb-0 text-primary fs-base">Recent Emails</h6>
              <span class="badge bg-secondary ms-auto text-light small">{{ data.recentEmails.length }}</span>
            </div>
            <div class="pane-body flex-grow-1 overflow-auto">
              <div v-if="data.recentEmails.length === 0" class="py-3 px-4 text-center text-muted small">
                No recent emails.
              </div>
              <div v-else class="list-group list-group-flush">
                <div v-for="email in data.recentEmails" :key="email.id" class="list-group-item bg-transparent border-secondary email-item py-2">
                  <a :href="email.webLink" target="_blank" class="text-decoration-none text-reset">
                    <div class="d-flex align-items-baseline">
                      <span class="fw-bold fs-sm text-truncate text-primary" style="max-width: 150px;">{{ email.from }}</span>
                      <span class="mx-2 text-muted fs-xs">—</span>
                      <div class="d-flex align-items-center gap-1 flex-grow-1 overflow-hidden">
                        <span v-for="rule in email.matchingRules" :key="rule.name" 
                              class="badge rounded-pill x-small" 
                              :style="{ backgroundColor: rule.color || 'var(--accent-blue)', color: 'white' }">
                          {{ rule.name }}
                        </span>
                        <span class="fs-sm text-truncate text-accent-blue">{{ email.subject }}</span>
                      </div>
                      <span class="text-muted fs-xs whitespace-nowrap ms-2">{{ formatRelativeTime(email.receivedDateTime) }}</span>
                    </div>
                    <div class="fs-xs text-muted text-truncate">{{ email.bodyPreview }}</div>
                  </a>
                </div>
              </div>
            </div>
            <div class="pane-footer bg-transparent border-secondary flex-shrink-0 px-3 py-2 border-top">
              <a href="/Email" class="text-accent-blue small text-decoration-none">Go to Inbox <i class="bi bi-arrow-right"></i></a>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, onUnmounted, watch } from 'vue';
import Navbar from './components/Navbar.vue';
import { formatFriendlyDate, formatEstimate, getDateColorClass } from './js/utils';
import { updateTask } from './js/tasks-api';

const loading = ref(true);
const data = ref(null);

// Resize state
const splitWidths = ref(JSON.parse(localStorage.getItem('dashboardSplitWidths')) || [33.33, 33.33, 33.33]);

const todayDate = computed(() => {
  return new Date().toLocaleDateString(undefined, { 
    weekday: 'long', 
    year: 'numeric', 
    month: 'long', 
    day: 'numeric' 
  });
});

const showResizer1 = computed(() => {
  return data.value && data.value.calendarVisible;
});

const showResizer2 = computed(() => {
  return data.value && data.value.emailVisible;
});

const paneStyles = computed(() => {
  if (!data.value) return {};

  const visibleCount = (data.value.calendarVisible ? 1 : 0) + 1 + (data.value.emailVisible ? 1 : 0);
  
  // Calculate relative widths based on visible panes
  let widths = [0, 0, 0];
  if (visibleCount === 3) {
    widths = splitWidths.value;
  } else if (visibleCount === 2) {
    if (!data.value.calendarVisible) {
      // Tasks and Email
      const total = splitWidths.value[1] + splitWidths.value[2];
      widths = [0, (splitWidths.value[1] / total) * 100, (splitWidths.value[2] / total) * 100];
    } else if (!data.value.emailVisible) {
      // Calendar and Tasks
      const total = splitWidths.value[0] + splitWidths.value[1];
      widths = [(splitWidths.value[0] / total) * 100, (splitWidths.value[1] / total) * 100, 0];
    }
  } else {
    // Only Tasks
    widths = [0, 100, 0];
  }

  return {
    calendar: { width: `${widths[0]}%`, flex: `0 0 ${widths[0]}%` },
    tasks: { width: `${widths[1]}%`, flex: `0 0 ${widths[1]}%` },
    email: { width: `${widths[2]}%`, flex: `0 0 ${widths[2]}%` }
  };
});

const tasksGridStyle = computed(() => {
  return {
    gridTemplateColumns: '100px 1fr 100px 100px 100px 60px'
  };
});

const loadData = async () => {
  loading.value = true;
  try {
    const response = await fetch('/api/dashboard');
    if (response.ok) {
      data.value = await response.json();
    }
  } catch (e) {
    console.error('Failed to load Dashboard data:', e);
  } finally {
    loading.value = false;
  }
};

const startResize = (resizer, e) => {
  e.preventDefault();
  const container = document.querySelector('.dashboard-grid');
  if (!container) return;

  const onMouseMove = (moveEvent) => {
    const containerRect = container.getBoundingClientRect();
    const percent = ((moveEvent.clientX - containerRect.left) / containerRect.width) * 100;
    
    const visibleCount = (data.value.calendarVisible ? 1 : 0) + 1 + (data.value.emailVisible ? 1 : 0);
    
    if (resizer === 'resizer1') {
      // Moving resizer between Calendar and Tasks
      if (visibleCount === 3) {
        const newWidth1 = Math.max(10, Math.min(percent, 80));
        const remaining = 100 - newWidth1;
        const ratio = splitWidths.value[2] / (splitWidths.value[1] + splitWidths.value[2]);
        const newWidth2 = remaining * (1 - ratio);
        const newWidth3 = remaining * ratio;
        splitWidths.value = [newWidth1, newWidth2, newWidth3];
      } else if (visibleCount === 2 && data.value.calendarVisible) {
        // Calendar and Tasks
        const newWidth1 = Math.max(10, Math.min(percent, 90));
        splitWidths.value = [newWidth1, 100 - newWidth1, 0];
      }
    } else if (resizer === 'resizer2') {
      // Moving resizer between Tasks and Email
      if (visibleCount === 3) {
        const newWidth3 = Math.max(10, Math.min(100 - percent, 80));
        const remaining = 100 - newWidth3;
        const ratio = splitWidths.value[0] / (splitWidths.value[0] + splitWidths.value[1]);
        const newWidth1 = remaining * ratio;
        const newWidth2 = remaining * (1 - ratio);
        splitWidths.value = [newWidth1, newWidth2, newWidth3];
      } else if (visibleCount === 2 && data.value.emailVisible) {
        if (!data.value.calendarVisible) {
          // Tasks and Email
          const newWidth2 = Math.max(10, Math.min(percent, 90));
          splitWidths.value = [0, newWidth2, 100 - newWidth2];
        } else {
          // Calendar and Email? (Shouldn't happen with current logic, but for safety)
          const newWidth3 = Math.max(10, Math.min(100 - percent, 90));
          splitWidths.value = [100 - newWidth3, 0, newWidth3];
        }
      }
    }
  };

  const onMouseUp = () => {
    localStorage.setItem('dashboardSplitWidths', JSON.stringify(splitWidths.value));
    document.removeEventListener('mousemove', onMouseMove);
    document.removeEventListener('mouseup', onMouseUp);
  };

  document.addEventListener('mousemove', onMouseMove);
  document.addEventListener('mouseup', onMouseUp);
};

const formatTime = (dateStr) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return date.toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' });
};

const formatRelativeTime = (dateStr) => {
  return formatFriendlyDate(dateStr, false, true);
};

const formatTaskDate = (dateStr) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
};

const getPriorityClass = (priority) => {
  return `priority-${priority}`;
};

const completeTask = async (task) => {
  try {
    await updateTask(task.id, { isCompleted: true });
    data.value.upcomingTasks = data.value.upcomingTasks.filter(t => t.id !== task.id);
  } catch (e) {
    console.error('Failed to complete task:', e);
  }
};

onMounted(() => {
  loadData();
});
</script>

<style scoped>
.dashboard-app {
  display: flex;
  height: 100vh;
  background-color: var(--bg-darker);
  color: var(--text-primary);
}

.main-content {
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.controls-bar {
  padding: 0 20px;
  color: var(--text-primary);
}

.text-primary {
  color: var(--text-primary) !important;
}

.text-accent-blue {
  color: var(--accent-blue) !important;
}

.text-accent-blue:hover {
  text-decoration: underline !important;
}

.content-body {
  flex-grow: 1;
  overflow: hidden;
}

.dashboard-grid {
  height: 100%;
}

.dashboard-pane {
  background-color: var(--bg-dark);
  min-width: 0;
}

.pane-header {
  height: 48px;
  background-color: rgba(255, 255, 255, 0.03);
  border-bottom: 1px solid var(--border-primary);
}

.pane-footer {
  background-color: rgba(255, 255, 255, 0.02);
}

.pane-body {
  padding: 0;
}

.priority-tag {
  font-size: var(--fs-xxs);
  padding: 1px 4px;
  border-radius: 3px;
  text-transform: uppercase;
  font-weight: bold;
}

.priority-Critical { background: var(--accent-red); color: white; }
.priority-Highest { background: #d73a49; color: white; }
.priority-High { background: #f97316; color: white; }
.priority-Medium { background: #eab308; color: black; }
.priority-Low { background: #22c55e; color: white; }
.priority-Lowest { background: #6366f1; color: white; }

.task-item:hover, .email-item:hover {
  background-color: rgba(255, 255, 255, 0.03) !important;
}

.spin {
  animation: fa-spin 2s infinite linear;
}

@keyframes fa-spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(359deg); }
}

.whitespace-nowrap {
  white-space: nowrap;
}

/* Scrollbar tweaks */
.x-small {
  font-size: 0.7rem;
}

.pane-body::-webkit-scrollbar {
  width: 6px;
}

.pane-body::-webkit-scrollbar-track {
  background: transparent;
}

.pane-body::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 3px;
}

.pane-body::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.2);
}

.list-group-item {
  border-left: none;
  border-right: none;
}

/* Tasks Grid Styling */
.dashboard-tasks-grid {
  display: flex;
  flex-direction: column;
  width: 100%;
}

.tasks-header-row {
  display: grid;
  background-color: rgba(255, 255, 255, 0.05);
  border-bottom: 1px solid var(--border-primary);
  font-weight: bold;
  font-size: 0.75rem;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.tasks-header {
  padding: 8px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.tasks-row {
  display: grid;
  background-color: transparent;
  transition: background-color 0.2s;
  border-bottom: 1px solid var(--border-primary);
  overflow: visible;
}

.tasks-row:hover {
  background-color: rgba(255, 255, 255, 0.02);
}

.tasks-cell {
  padding: 8px;
  display: flex;
  align-items: center;
  min-width: 0;
  font-size: 0.8rem;
}

.task-title {
  color: var(--text-primary);
}

.type-badge, .status-badge, .priority-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  font-weight: 500;
}

.date-cell {
  color: var(--text-muted);
}

.estimate-cell {
  color: var(--text-muted);
  justify-content: flex-end;
}

/* Date colors from utils.js logic */
.date-overdue { color: #f85149 !important; font-weight: bold; }
.date-today { color: #d29922 !important; font-weight: bold; }
.date-this-week { color: #3fb950 !important; }
.date-muted { color: var(--text-muted) !important; }

</style>
