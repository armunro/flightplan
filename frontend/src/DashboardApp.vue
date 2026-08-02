<template>
  <div :class="['dashboard-app', themeClass]">
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
            <!-- Today's Events Split -->
            <div class="d-flex flex-column" :style="paneStyles.todayEvents">
              <div class="pane-header d-flex align-items-center flex-shrink-0 px-3">
                <i class="bi bi-calendar3 me-2 text-info"></i>
                <h6 class="mb-0 theme-text fs-base">Today's Events</h6>
                <span class="badge theme-badge ms-auto small">{{ data.todaysEvents.length }}</span>
              </div>
              <div class="pane-body flex-grow-1 overflow-auto">
                <div v-if="data.todaysEvents.length === 0" class="py-3 px-4 text-center text-muted small">
                  No events scheduled for today.
                </div>
                <div v-else class="list-group list-group-flush">
                  <div v-for="event in data.todaysEvents" :key="event.id" class="list-group-item bg-transparent border-secondary py-2 px-3 event-item">
                    <div class="d-flex justify-content-between align-items-start">
                      <div class="event-details min-width-0 d-flex gap-2">
                        <div class="event-icon-wrapper mt-1" :style="{ color: getCalendarPref(event.calendarId).color || 'var(--accent-blue)' }">
                          <i class="bi" :class="getCalendarPref(event.calendarId).icon || 'bi-calendar3'"></i>
                        </div>
                        <div class="min-width-0">
                          <div class="fw-bold fs-xs text-primary text-truncate">{{ event.subject }}</div>
                          <div class="text-muted fs-xxs mt-1">
                            <i class="bi bi-clock me-1"></i>
                            {{ formatTime(event.start) }} - {{ formatTime(event.end) }}
                          </div>
                        </div>
                      </div>
                      <a v-if="event.webLink" :href="event.webLink" target="_blank" class="btn btn-link btn-sm p-0 text-accent-blue ms-2">
                        <i class="bi bi-box-arrow-up-right fs-xs"></i>
                      </a>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Horizontal Resizer -->
            <div class="horizontal-resizer" @mousedown="startResize('calendarVertical', $event)"></div>

            <!-- Upcoming Events Split -->
            <div class="d-flex flex-column" :style="paneStyles.upcomingEvents">
              <div class="pane-header d-flex align-items-center flex-shrink-0 px-3">
                <i class="bi bi-calendar-range me-2 text-info"></i>
                <h6 class="mb-0 theme-text fs-base">Upcoming Events</h6>
                <span class="badge theme-badge ms-auto small">{{ data.upcomingEvents.length }}</span>
              </div>
              <div class="pane-body flex-grow-1 overflow-auto">
                <div v-if="data.upcomingEvents.length === 0" class="py-3 px-4 text-center text-muted small">
                  No upcoming events.
                </div>
                <div v-else class="list-group list-group-flush">
                  <div v-for="event in data.upcomingEvents" :key="event.id" class="list-group-item bg-transparent border-secondary py-2 px-3 event-item">
                    <div class="d-flex justify-content-between align-items-start">
                      <div class="event-details min-width-0 d-flex gap-2">
                        <div class="event-icon-wrapper mt-1" :style="{ color: getCalendarPref(event.calendarId).color || 'var(--accent-blue)' }">
                          <i class="bi" :class="getCalendarPref(event.calendarId).icon || 'bi-calendar3'"></i>
                        </div>
                        <div class="min-width-0">
                          <div class="fw-bold fs-xs text-primary text-truncate">{{ event.subject }}</div>
                          <div class="text-muted fs-xxs mt-1">
                            <i class="bi bi-calendar3 me-1"></i>
                            {{ formatTaskDate(event.start) }} • {{ formatTime(event.start) }}
                          </div>
                        </div>
                      </div>
                      <a v-if="event.webLink" :href="event.webLink" target="_blank" class="btn btn-link btn-sm p-0 text-accent-blue ms-2">
                        <i class="bi bi-box-arrow-up-right fs-xs"></i>
                      </a>
                    </div>
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
              <h6 class="mb-0 theme-text fs-base">Upcoming Deadlines</h6>
              <span class="badge theme-badge ms-auto small">{{ data.upcomingTasks.length }}</span>
            </div>
            <div class="pane-body flex-grow-1 overflow-auto">
              <div v-if="data.upcomingTasks.length === 0" class="py-3 px-4 text-center text-muted small">
                No upcoming deadlines.
              </div>
              <div v-else class="dashboard-tasks-grid">
                <div v-for="task in data.upcomingTasks" :key="task.id" class="tasks-row py-2">
                  <div class="d-flex align-items-center w-100 px-3">
                    <div class="flex-grow-1 min-width-0">
                      <!-- First Line: Project & List -->
                      <div class="d-flex align-items-center gap-2 mb-1">
                        <span v-if="task.projectName" class="d-flex align-items-center gap-1" :style="{ color: task.projectColor || 'var(--accent-blue)' }">
                          <i v-if="task.projectIcon" class="bi fs-xxs" :class="task.projectIcon"></i>
                          <span class="fs-xxs fw-bold text-uppercase">{{ task.projectName }}</span>
                        </span>
                        <span v-if="task.listName" class="text-muted fs-xxs">
                          <i class="bi bi-chevron-right mx-1" style="font-size: 8px;"></i>
                          {{ task.listName }}
                        </span>
                      </div>

                      <!-- Second Line: Title -->
                      <div class="d-flex align-items-center">
                        <span class="task-title text-truncate fw-bold" :title="task.title">{{ task.title }}</span>
                      </div>
                      
                      <!-- Third Line: Metadata -->
                      <div class="d-flex align-items-center gap-3 mt-1 flex-wrap">
                        <span class="type-badge" :style="{ color: task.typeColor || '#3498db' }">
                          <i :class="task.typeIcon || 'bi-briefcase'"></i>
                          <span class="ms-1">{{ task.typeName || 'Work' }}</span>
                        </span>

                        <span class="status-badge" :style="{ color: task.statusColor || '#cccccc' }">
                          <i class="bi bi-circle-fill" style="font-size: 8px; margin-right: 4px;"></i>
                          <span>{{ task.statusName || 'Unknown' }}</span>
                        </span>

                        <span class="priority-badge" :style="{ color: task.priorityColor || '#ccc' }">
                          <i :class="task.priorityIcon || 'bi-dash-lg'" style="margin-right: 4px;"></i>
                          <span>{{ task.priorityName || '' }}</span>
                        </span>

                        <div class="ms-auto d-flex align-items-center gap-3">
                          <span class="date-cell" :class="getDateColorClass(task.end, task.isCompleted)">
                            <i class="bi bi-calendar3 me-1"></i>
                            {{ formatRelativeTime(task.end) }}
                          </span>
                          <span v-if="task.estimateMinutes" class="estimate-cell text-muted">
                            <i class="bi bi-hourglass-split me-1"></i>
                            {{ formatEstimate(task.estimateMinutes) }}
                          </span>
                        </div>
                      </div>
                    </div>
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
              <h6 class="mb-0 theme-text fs-base">Recent Emails</h6>
              <span class="badge theme-badge ms-auto small">{{ data.recentEmails.length }}</span>
            </div>
            <div class="pane-body flex-grow-1 overflow-auto">
              <div v-if="data.recentEmails.length === 0" class="py-3 px-4 text-center text-muted small">
                No recent emails.
              </div>
              <div v-else class="list-group list-group-flush">
                <div v-for="email in data.recentEmails" :key="email.id" class="list-group-item bg-transparent border-secondary email-item py-2 px-3">
                  <a :href="email.webLink" target="_blank" class="text-decoration-none text-reset">
                    <div class="d-flex flex-column">
                      <!-- First Line: Subject -->
                      <div class="d-flex align-items-center mb-1">
                        <span class="fs-sm fw-bold text-primary text-truncate">{{ email.subject }}</span>
                      </div>
                      
                      <!-- Second Line: Metadata -->
                      <div class="d-flex align-items-center gap-2 flex-wrap mb-1">
                        <span class="fw-bold fs-xs text-truncate text-muted" style="max-width: 150px;">{{ email.from }}</span>
                        
                        <div class="d-flex align-items-center gap-1 flex-wrap">
                          <span v-for="rule in email.matchingRules" :key="rule.name" 
                                class="badge rounded-pill x-small" 
                                :style="{ backgroundColor: rule.color || 'var(--accent-blue)', color: 'white' }">
                            {{ rule.name }}
                          </span>
                        </div>

                        <span class="text-muted fs-xs whitespace-nowrap ms-auto">
                          <i class="bi bi-clock me-1"></i>
                          {{ formatRelativeTime(email.receivedDateTime) }}
                        </span>
                      </div>

                      <!-- Third Line: Preview -->
                      <div class="fs-xs text-muted text-truncate">{{ email.bodyPreview }}</div>
                    </div>
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
import { fetchSettings } from './js/dashboard-api';

const loading = ref(true);
const data = ref(null);
const theme = ref('Cosmic');
const themeClass = computed(() => `theme-${theme.value.toLowerCase()}`);

// Resize state
const splitWidths = ref(JSON.parse(localStorage.getItem('dashboardSplitWidths')) || [33.33, 33.33, 33.33]);
const calendarSplitHeight = ref(Number(localStorage.getItem('calendarSplitHeight')) || 50); // Height of today's events in %

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
    todayEvents: { height: `${calendarSplitHeight.value}%`, flex: `0 0 ${calendarSplitHeight.value}%` },
    upcomingEvents: { height: `${100 - calendarSplitHeight.value}%`, flex: `0 0 ${100 - calendarSplitHeight.value}%` },
    tasks: { width: `${widths[1]}%`, flex: `0 0 ${widths[1]}%` },
    email: { width: `${widths[2]}%`, flex: `0 0 ${widths[2]}%` }
  };
});


const loadData = async () => {
  loading.value = true;
  try {
    const [dashResponse, settings] = await Promise.all([
      fetch('/api/dashboard'),
      fetchSettings()
    ]);

    if (dashResponse.ok) {
      data.value = await dashResponse.json();
    }
    
    if (settings) {
      theme.value = settings.theme || 'Cosmic';
    }
  } catch (e) {
    console.error('Failed to load Dashboard data:', e);
  } finally {
    loading.value = false;
  }
};

const startResize = (resizer, e) => {
  e.preventDefault();
  
  if (resizer === 'calendarVertical') {
    const container = e.target.parentElement;
    if (!container) return;

    const onMouseMove = (moveEvent) => {
      const rect = container.getBoundingClientRect();
      const percent = ((moveEvent.clientY - rect.top) / rect.height) * 100;
      calendarSplitHeight.value = Math.max(10, Math.min(percent, 90));
    };

    const onMouseUp = () => {
      localStorage.setItem('calendarSplitHeight', calendarSplitHeight.value.toString());
      document.removeEventListener('mousemove', onMouseMove);
      document.removeEventListener('mouseup', onMouseUp);
    };

    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);
    return;
  }

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

const getCalendarPref = (calendarId) => {
  if (!data.value?.calendarPreferences || !calendarId) return { color: '', icon: '' };
  const pref = data.value.calendarPreferences[calendarId];
  if (!pref) return { color: '', icon: '' };
  return {
    color: pref.color,
    icon: pref.customIcon
  };
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
  window.addEventListener('task-added', loadData);
});

onUnmounted(() => {
  window.removeEventListener('task-added', loadData);
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

.horizontal-resizer {
  height: 4px;
  cursor: row-resize;
  background: transparent;
  transition: background 0.2s;
  z-index: 5;
  margin-top: -2px;
  margin-bottom: -2px;
  position: relative;
  flex-shrink: 0;
}

.horizontal-resizer::after {
  content: "";
  position: absolute;
  top: 50%;
  left: 0;
  right: 0;
  height: 1px;
  background-color: var(--border-primary);
  transform: translateY(-50%);
  transition: background-color 0.2s;
}

.horizontal-resizer:hover::after, .horizontal-resizer:active::after {
  background-color: var(--accent-blue);
  height: 2px;
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

.task-item:hover, .email-item:hover, .event-item:hover {
  background-color: rgba(255, 255, 255, 0.03) !important;
}

.event-icon-wrapper {
  width: 16px;
  height: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.85rem;
  flex-shrink: 0;
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

.tasks-row {
  display: flex;
  background-color: transparent;
  transition: background-color 0.2s;
  border-bottom: 1px solid var(--border-primary);
}

.tasks-row:hover {
  background-color: rgba(255, 255, 255, 0.02);
}

.task-title {
  color: var(--text-primary);
  font-size: 0.9rem;
}

.type-badge, .status-badge, .priority-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  white-space: nowrap;
  font-size: 0.75rem;
  font-weight: 500;
}

.date-cell, .estimate-cell {
  font-size: 0.75rem;
  white-space: nowrap;
}

/* Date colors from utils.js logic */
.date-overdue { color: #ff4d4d !important; font-weight: 500; }
.date-today { color: #ffcc00 !important; }
.date-this-week { color: #3399ff !important; }
.date-muted { color: var(--text-muted) !important; opacity: 0.7; }

</style>
