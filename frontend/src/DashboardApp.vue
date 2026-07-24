<template>
  <div class="vh-100 d-flex flex-row overflow-hidden">
    <Navbar />
    <div class="flex-grow-1 overflow-auto bg-darker main-content p-4">
      <div class="container-fluid">
        <header class="mb-5 mt-3">
          <h1 class="display-4 fw-bold text-light">Welcome back</h1>
          <p class="text-muted lead">Here's what's happening across your projects today.</p>
        </header>

        <div class="row g-4 mb-5">
          <div v-for="item in visibleMenuItems" :key="item.id" class="col-12 col-md-6 col-lg-4 col-xl-3">
            <a :href="item.href" class="text-decoration-none h-100 d-block">
              <div class="card h-100 dashboard-card border-0">
                <div class="card-body d-flex flex-column p-4">
                  <div class="icon-wrapper mb-3" :style="{ color: item.color }">
                    <i class="bi" :class="item.icon"></i>
                  </div>
                  <h3 class="card-title h5 mb-2 text-light">{{ item.name }}</h3>
                  <p class="card-text small mb-4">{{ item.description }}</p>
                  <div class="mt-auto d-flex align-items-center text-primary-link">
                    <span class="small fw-bold">Open Module</span>
                    <i class="bi bi-arrow-right ms-2 transition-icon"></i>
                  </div>
                </div>
              </div>
            </a>
          </div>
        </div>

        <div class="row g-4">
          <div class="col-12 col-xl-8">
            <div class="card border-0 bg-dark h-100">
              <div class="card-header bg-transparent border-bottom border-secondary p-3 d-flex align-items-center">
                <i class="bi bi-lightning-charge-fill text-warning me-2"></i>
                <h5 class="mb-0 text-light">Recent Activity</h5>
              </div>
              <div class="card-body p-0">
                <div class="p-5 text-center text-muted opacity-50">
                   <i class="bi bi-clock-history display-4 mb-3 d-block"></i>
                   <p>No recent activity to show yet.</p>
                </div>
              </div>
            </div>
          </div>
          <div class="col-12 col-xl-4">
            <div class="card border-0 bg-dark h-100">
               <div class="card-header bg-transparent border-bottom border-secondary p-3 d-flex align-items-center">
                <i class="bi bi-check2-circle text-success me-2"></i>
                <h5 class="mb-0 text-light">Quick Stats</h5>
              </div>
              <div class="card-body">
                <div class="stat-item mb-3">
                  <div class="d-flex justify-content-between mb-1">
                    <span class="text-muted small">Jira Issues</span>
                    <span class="text-light small">12 Active</span>
                  </div>
                  <div class="progress" style="height: 4px;">
                    <div class="progress-bar bg-primary" style="width: 70%"></div>
                  </div>
                </div>
                <div class="stat-item mb-3">
                  <div class="d-flex justify-content-between mb-1">
                    <span class="text-muted small">Open PRs</span>
                    <span class="text-light small">4 Pending</span>
                  </div>
                  <div class="progress" style="height: 4px;">
                    <div class="progress-bar bg-success" style="width: 40%"></div>
                  </div>
                </div>
                 <div class="stat-item">
                  <div class="d-flex justify-content-between mb-1">
                    <span class="text-muted small">Unread Emails</span>
                    <span class="text-light small">8 New</span>
                  </div>
                  <div class="progress" style="height: 4px;">
                    <div class="progress-bar bg-warning" style="width: 60%"></div>
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
import { computed, ref, onMounted } from 'vue';
import Navbar from './components/Navbar.vue';
import { fetchSettings } from './js/dashboard-api';

const pageVisibilities = ref([]);

onMounted(async () => {
  try {
    const settings = await fetchSettings();
    pageVisibilities.value = settings.pageVisibilities || [];
  } catch (e) {
    console.error('Failed to load settings in Dashboard:', e);
  }
});

const menuItems = [
  { id: 'jira', name: 'Jira', href: '/Jira', icon: 'bi-kanban', color: '#0052cc', description: 'Manage issues, track progress, and organize your sprints.' },
  { id: 'github', name: 'GitHub', href: '/Github', icon: 'bi-github', color: '#f0f6fc', description: 'Review pull requests and monitor repository activity.' },
  { id: 'tasks', name: 'Tasks', href: '/Tasks', icon: 'bi-check2-square', color: '#3fb950', description: 'Keep track of your personal to-do list and reminders.' },
  { id: 'email', name: 'Email', href: '/Email', icon: 'bi-envelope', color: '#58a6ff', description: 'Stay connected with your team and clients.' },
  { id: 'calendar', name: 'Calendar', href: '/Calendar', icon: 'bi-calendar3', color: '#bc8cff', description: 'Manage your schedule and upcoming meetings.' },
  { id: 'links', name: 'Links', href: '/Links', icon: 'bi-link-45deg', color: '#f0883e', description: 'Quick access to your most important bookmarks.' },
  { id: 'notepad', name: 'Notepad', href: '/Notepad', icon: 'bi-sticky', color: '#d29922', description: 'Jot down quick thoughts and persistent notes.' },
  { id: 'schedules', name: 'Schedules', href: '/ScheduledTasks', icon: 'bi-clock-history', color: '#ff7b72', description: 'Manage recurring automated tasks and cron schedules.' },
  { id: 'debug', name: 'Diagnostics', href: '/Debug', icon: 'bi-bug', color: '#79c0ff', description: 'System information, paths, and application health data.' },
  { id: 'settings', name: 'Settings', href: '/Settings', icon: 'bi-gear', color: '#aab2bb', description: 'Configure your account and application preferences.' },
];

const visibleMenuItems = computed(() => {
  return menuItems.filter(item => {
    if (item.id === 'settings') return true;
    const visibility = pageVisibilities.value.find(p => p.id === item.id);
    return visibility ? visibility.visible : true;
  });
});
</script>

<style>
.main-content {
  scroll-behavior: smooth;
}

.card-text {
  color: var(--text-primary) !important;
}

.dashboard-card {
  background-color: var(--bg-dark);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  cursor: pointer;
  border: 1px solid transparent !important;
}

.dashboard-card:hover {
  background-color: var(--bg-card);
  transform: translateY(-5px);
  border-color: var(--border-primary) !important;
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.3);
}

.icon-wrapper {
  font-size: 2rem;
  height: 60px;
  width: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 12px;
}

.text-primary-link {
  color: var(--accent-blue);
  opacity: 0.8;
  transition: opacity 0.2s;
}

.dashboard-card:hover .text-primary-link {
  opacity: 1;
}

.transition-icon {
  transition: transform 0.2s;
}

.dashboard-card:hover .transition-icon {
  transform: translateX(5px);
}

.progress {
  background-color: rgba(255, 255, 255, 0.05);
}

/* Custom Scrollbar moved to global.css */
</style>
