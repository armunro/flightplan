<template>
  <div class="my-day-app">
    <Navbar />
    <div class="main-content">
      <div class="controls-bar">
        <div class="d-flex align-items-center">
          <h5 class="mb-0 me-3 text-primary">My Day</h5>
          <span class="text-muted small">{{ todayDate }}</span>
        </div>
        <div class="ms-auto">
          <button class="btn btn-secondary btn-sm" @click="loadData" :disabled="loading">
            <i class="bi bi-arrow-clockwise" :class="{ 'spin': loading }"></i> Refresh
          </button>
        </div>
      </div>

      <div class="content-body p-4">
        <div v-if="loading && !data" class="d-flex justify-content-center mt-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <div v-else-if="data" class="sections-stack h-100 d-flex flex-column">
          <!-- Calendar Section -->
          <div v-if="data.calendarVisible" class="section-container mb-4 d-flex flex-column">
            <div class="card d-flex flex-column shadow-sm">
              <div class="card-header d-flex align-items-center flex-shrink-0">
                <i class="bi bi-calendar3 me-2 text-info"></i>
                <h6 class="mb-0 text-primary fs-base">Today's Events</h6>
                <span class="badge bg-secondary ms-auto text-light small">{{ data.todaysEvents.length }}</span>
              </div>
              <div class="card-body p-0 overflow-auto">
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
          </div>

          <!-- Tasks Section -->
          <div v-if="data.tasksDueToday" class="section-container mb-4 d-flex flex-column">
            <div class="card d-flex flex-column shadow-sm">
              <div class="card-header d-flex align-items-center flex-shrink-0">
                <i class="bi bi-check2-square me-2 text-success"></i>
                <h6 class="mb-0 text-primary fs-base">Tasks Due Today</h6>
                <span class="badge bg-secondary ms-auto text-light small">{{ data.tasksDueToday.length }}</span>
              </div>
              <div class="card-body p-0 overflow-auto">
                <div v-if="data.tasksDueToday.length === 0" class="py-3 px-4 text-center text-muted small">
                  No tasks due today.
                </div>
                <div v-else class="list-group list-group-flush">
                  <div v-for="task in data.tasksDueToday" :key="task.id" class="list-group-item bg-transparent border-secondary task-item py-3">
                    <div class="d-flex align-items-center">
                      <input type="checkbox" class="form-check-input me-3" @change="completeTask(task)">
                      <div class="task-info overflow-hidden">
                        <div class="text-truncate fs-sm text-primary" :title="task.title">{{ task.title }}</div>
                        <div class="text-muted fs-xs mt-1">
                          <span v-if="task.estimateMinutes" class="me-2">
                            <i class="bi bi-hourglass-split me-1"></i>{{ formatEstimate(task.estimateMinutes) }}
                          </span>
                          <span :class="getPriorityClass(task.priority)" class="priority-tag">
                            {{ task.priority }}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div class="card-footer bg-transparent border-secondary flex-shrink-0">
                <a href="/Tasks" class="text-accent-blue small text-decoration-none">View all tasks <i class="bi bi-arrow-right"></i></a>
              </div>
            </div>
          </div>

          <!-- Email Section -->
          <div v-if="data.emailVisible" class="section-container mb-0 d-flex flex-column">
            <div class="card d-flex flex-column shadow-sm">
              <div class="card-header d-flex align-items-center flex-shrink-0">
                <i class="bi bi-envelope me-2 text-warning"></i>
                <h6 class="mb-0 text-primary fs-base">Recent Emails</h6>
                <span class="badge bg-secondary ms-auto text-light small">{{ data.recentEmails.length }}</span>
              </div>
              <div class="card-body p-0 overflow-auto">
                <div v-if="data.recentEmails.length === 0" class="py-3 px-4 text-center text-muted small">
                  No recent emails.
                </div>
                <div v-else class="list-group list-group-flush">
                  <div v-for="email in data.recentEmails" :key="email.id" class="list-group-item bg-transparent border-secondary email-item py-2">
                    <a :href="email.webLink" target="_blank" class="text-decoration-none text-reset">
                      <div class="d-flex align-items-baseline">
                        <span class="fw-bold fs-sm text-truncate text-primary" style="max-width: 200px;">{{ email.from }}</span>
                        <span class="mx-2 text-muted fs-xs">—</span>
                        <span class="fs-sm text-truncate text-accent-blue flex-grow-1">{{ email.subject }}</span>
                        <span class="text-muted fs-xs whitespace-nowrap ms-2">{{ formatRelativeTime(email.receivedDateTime) }}</span>
                      </div>
                      <div class="fs-xs text-muted text-truncate">{{ email.bodyPreview }}</div>
                    </a>
                  </div>
                </div>
              </div>
              <div class="card-footer bg-transparent border-secondary flex-shrink-0">
                <a href="/Email" class="text-accent-blue small text-decoration-none">Go to Inbox <i class="bi bi-arrow-right"></i></a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import Navbar from './components/Navbar.vue';
import { formatFriendlyDate, formatEstimate } from './js/utils';
import { updateTask } from './js/tasks-api';

const loading = ref(true);
const data = ref(null);

const todayDate = computed(() => {
  return new Date().toLocaleDateString(undefined, { 
    weekday: 'long', 
    year: 'numeric', 
    month: 'long', 
    day: 'numeric' 
  });
});

const loadData = async () => {
  loading.value = true;
  try {
    const response = await fetch('/api/myday');
    if (response.ok) {
      data.value = await response.json();
    }
  } catch (e) {
    console.error('Failed to load My Day data:', e);
  } finally {
    loading.value = false;
  }
};

const formatTime = (dateStr) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return date.toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' });
};

const formatRelativeTime = (dateStr) => {
  return formatFriendlyDate(dateStr, false, true);
};

const getPriorityClass = (priority) => {
  return `priority-${priority}`;
};

const completeTask = async (task) => {
  // Simple completion logic for My Day view
  try {
    await updateTask(task.id, { isCompleted: true });
    // Remove from local list
    data.value.tasksDueToday = data.value.tasksDueToday.filter(t => t.id !== task.id);
  } catch (e) {
    console.error('Failed to complete task:', e);
  }
};

onMounted(() => {
  loadData();
});
</script>

<style scoped>
.my-day-app {
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
  /* height and background moved to global.css */
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
  overflow: auto;
}

.sections-stack {
  display: flex;
  flex-direction: column;
}

.section-container {
  flex: 0 1 auto;
}

.card {
  background-color: var(--bg-dark);
  border: 1px solid var(--border-primary);
}

.card-header {
  background-color: rgba(255, 255, 255, 0.03);
  border-bottom: 1px solid var(--border-primary);
  padding: 12px 16px;
}

.extra-small {
  font-size: 0.75rem;
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
</style>
