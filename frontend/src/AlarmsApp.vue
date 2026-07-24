<template>
  <div class="vh-100 d-flex flex-row overflow-hidden">
    <Navbar />
    <div class="main-content">
      <div class="controls-bar">
        <div>
          <h1 class="h3 fw-bold mb-0">Alarms & Timers</h1>
        </div>
        <div class="d-flex align-items-center gap-3">
          <button class="btn btn-primary btn-sm d-flex align-items-center px-3" @click="showAddModal = true">
            <i class="bi bi-plus-lg me-2"></i>
            <span>Add New</span>
          </button>
        </div>
      </div>

      <div class="flex-grow-1 overflow-auto p-4">
        <div class="container-fluid">
          <div class="row g-3">
            <!-- Alarms List -->
            <div v-for="alarm in alarms" :key="alarm.id" class="col-12 col-md-6 col-lg-4">
              <div class="card h-100 border-0 bg-dark position-relative">
                <div class="card-body p-4">
                  <div class="d-flex justify-content-between align-items-start mb-3">
                    <div class="icon-circle" :class="alarm.type === 0 ? 'bg-info-subtle text-info' : 'bg-warning-subtle text-warning'">
                      <i class="bi" :class="alarm.type === 0 ? 'bi-stopwatch' : 'bi-calendar-event'"></i>
                    </div>
                    <div class="d-flex align-items-start gap-2">
                      <button class="btn btn-sm btn-outline-danger border-0 opacity-50 hover-opacity-100" 
                              @click.prevent="deleteAlarm(alarm.id)"
                              title="Delete Alarm">
                        <i class="bi bi-trash"></i>
                      </button>
                      <div class="dropdown">
                        <button class="btn btn-link p-0" data-bs-toggle="dropdown">
                          <i class="bi bi-three-dots-vertical"></i>
                        </button>
                        <ul class="dropdown-menu dropdown-menu-dark dropdown-menu-end">
                          <li><a class="dropdown-item" href="#" @click.prevent="deleteAlarm(alarm.id)">Delete</a></li>
                        </ul>
                      </div>
                    </div>
                  </div>

                  <h5 class="text-light mb-1">{{ alarm.title }}</h5>
                  <p class=" small mb-3">{{ alarm.type === 0 ? 'Timer' : 'Countdown' }}</p>

                  <div class="timer-display mb-4">
                    <div class="display-4 fw-mono text-center" :class="getTimerClass(alarm)">
                      {{ formatTimeRemaining(alarm) }}
                    </div>
                  </div>

                  <div class="d-flex justify-content-between align-items-center">
                     <span class="badge" :class="alarm.isCompleted ? 'bg-success' : 'bg-primary'">
                      {{ alarm.isCompleted ? 'Completed' : 'Running' }}
                    </span>
                    <button v-if="alarm.isCompleted" class="btn btn-sm btn-outline-secondary" @click="resetAlarm(alarm)">
                      Reset
                    </button>
                  </div>
                </div>
              </div>
            </div>

            <!-- Empty State -->
            <div v-if="alarms.length === 0" class="col-12 text-center py-5">
              <div class="py-5">
                <i class="bi bi-alarm display-1 mb-3 d-block"></i>
                <h4 class="text-light">No alarms yet</h4>
                <p class="">Create your first timer or countdown to get started.</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Add Modal -->
    <div v-if="showAddModal" class="modal-backdrop fade show"></div>
    <div v-if="showAddModal" class="modal fade show d-block" tabindex="-1">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content bg-dark border-secondary">
          <div class="modal-header border-bottom border-secondary">
            <h5 class="modal-title text-light">Create New Alarm</h5>
            <button type="button" class="btn-close btn-close-white" @click="showAddModal = false"></button>
          </div>
          <div class="modal-body">
            <div class="mb-3">
              <label class="form-label text-light small opacity-75">Title</label>
              <input v-model="newAlarm.title" type="text" class="form-control bg-darker border-secondary text-light custom-placeholder" placeholder="e.g. Pizza Timer or Project Launch">
            </div>
            <div class="mb-3">
              <label class="form-label text-light small opacity-75">Type</label>
              <select v-model="newAlarm.type" class="form-select bg-darker border-secondary text-light">
                <option :value="0">Timer (Countdown from Duration)</option>
                <option :value="1">Countdown (To Specific Date)</option>
              </select>
            </div>
            
            <div v-if="newAlarm.type === 0" class="row g-2">
              <div class="col-4">
                <label class="form-label text-light small opacity-75">Hours</label>
                <input v-model.number="timerInput.h" type="number" class="form-control bg-darker border-secondary text-light custom-placeholder" min="0">
              </div>
              <div class="col-4">
                <label class="form-label text-light small opacity-75">Minutes</label>
                <input v-model.number="timerInput.m" type="number" class="form-control bg-darker border-secondary text-light custom-placeholder" min="0" max="59">
              </div>
              <div class="col-4">
                <label class="form-label text-light small opacity-75">Seconds</label>
                <input v-model.number="timerInput.s" type="number" class="form-control bg-darker border-secondary text-light custom-placeholder" min="0" max="59">
              </div>
            </div>

            <div v-if="newAlarm.type === 1" class="mb-3">
              <label class="form-label text-light small opacity-75">Target Date & Time</label>
              <input v-model="newAlarm.targetTime" type="datetime-local" class="form-control bg-darker border-secondary text-light custom-placeholder">
            </div>
          </div>
          <div class="modal-footer border-top border-secondary">
            <button type="button" class="btn btn-link text-muted" @click="showAddModal = false">Cancel</button>
            <button type="button" class="btn btn-primary px-4" @click="saveAlarm">Create Alarm</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import Navbar from './components/Navbar.vue';

const alarms = ref([]);
const showAddModal = ref(false);
const timerInput = ref({ h: 0, m: 0, s: 0 });
const newAlarm = ref({
  title: '',
  type: 0,
  targetTime: '',
  duration: null
});

const now = ref(new Date());
let interval = null;

onMounted(() => {
  fetchAlarms();
  interval = setInterval(() => {
    now.value = new Date();
    checkAlarms();
  }, 1000);
});

onUnmounted(() => {
  if (interval) clearInterval(interval);
});

const fetchAlarms = async () => {
  try {
    const response = await fetch('/api/alarms');
    alarms.value = await response.json();
  } catch (error) {
    console.error('Failed to fetch alarms', error);
  }
};

const saveAlarm = async () => {
  if (!newAlarm.value.title) return;

  const alarmToSave = { ...newAlarm.value };
  
  if (alarmToSave.type === 0) {
    const totalSeconds = (timerInput.value.h * 3600) + (timerInput.value.m * 60) + timerInput.value.s;
    if (totalSeconds <= 0) return;
    
    // Format TimeSpan as HH:mm:ss for C#
    const h = String(timerInput.value.h).padStart(2, '0');
    const m = String(timerInput.value.m).padStart(2, '0');
    const s = String(timerInput.value.s).padStart(2, '0');
    alarmToSave.duration = `${h}:${m}:${s}`;
    alarmToSave.targetTime = new Date(Date.now() + totalSeconds * 1000).toISOString();
  } else {
    if (!alarmToSave.targetTime) return;
    alarmToSave.targetTime = new Date(alarmToSave.targetTime).toISOString();
  }

  try {
    const response = await fetch('/api/alarms', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(alarmToSave)
    });
    
    if (response.ok) {
      showAddModal.value = false;
      newAlarm.value = { title: '', type: 0, targetTime: '', duration: null };
      timerInput.value = { h: 0, m: 0, s: 0 };
      fetchAlarms();
    }
  } catch (error) {
    console.error('Failed to save alarm', error);
  }
};

const deleteAlarm = async (id) => {
  try {
    await fetch(`/api/alarms/${id}`, { method: 'DELETE' });
    fetchAlarms();
  } catch (error) {
    console.error('Failed to delete alarm', error);
  }
};

const resetAlarm = (alarm) => {
  // Simple reset: if timer, restart from duration
  if (alarm.type === 0 && alarm.duration) {
    const parts = alarm.duration.split(':').map(Number);
    const seconds = (parts[0] * 3600) + (parts[1] * 60) + parts[2];
    alarm.targetTime = new Date(Date.now() + seconds * 1000).toISOString();
    alarm.isCompleted = false;
    updateAlarm(alarm);
  } else {
      alarm.isCompleted = false;
      updateAlarm(alarm);
  }
};

const updateAlarm = async (alarm) => {
    try {
        await fetch('/api/alarms', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(alarm)
        });
        fetchAlarms();
    } catch (error) {
        console.error('Failed to update alarm', error);
    }
}

const checkAlarms = () => {
    let changed = false;
    alarms.value.forEach(alarm => {
        if (!alarm.isCompleted && new Date(alarm.targetTime) <= now.value) {
            alarm.isCompleted = true;
            changed = true;
            // Notify or play sound? For now just mark completed
        }
    });
    if (changed) {
        // Optionally save the state to backend
    }
}

const formatTimeRemaining = (alarm) => {
  const target = new Date(alarm.targetTime);
  const diff = target - now.value;
  
  if (diff <= 0) return '00:00:00';
  
  const h = Math.floor(diff / 3600000);
  const m = Math.floor((diff % 3600000) / 60000);
  const s = Math.floor((diff % 60000) / 1000);
  
  return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
};

const getTimerClass = (alarm) => {
    if (alarm.isCompleted) return 'text-danger fw-bold blink';
    const target = new Date(alarm.targetTime);
    const diff = target - now.value;
    if (diff < 60000) return 'text-warning'; // Less than a minute
    return 'text-light';
}
</script>

<style scoped>
.icon-circle {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
}

.fw-mono {
  font-family: 'JetBrains Mono', 'Courier New', monospace;
}

.timer-display {
  padding: 1rem;
  background: var(--bg-darker);
  border-radius: 8px;
  border: 1px solid var(--border-primary);
}

.blink {
    animation: blinker 1s linear infinite;
}

@keyframes blinker {
    50% { opacity: 0.5; }
}

.custom-placeholder::placeholder {
    color: rgba(255, 255, 255, 0.5);
    opacity: 1;
}

.hover-opacity-100:hover {
    opacity: 1 !important;
}
</style>
