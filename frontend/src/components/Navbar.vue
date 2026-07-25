<template>
  <div class="main-navbar d-flex flex-column flex-shrink-0" :class="{ 'collapsed': isCollapsed }">
    <Toast />
    <div class="navbar-header d-flex align-items-center" :class="{ 'collapsed': isCollapsed }">
      <a href="/Dashboard" class="navbar-brand d-flex align-items-center">
        <i class="bi bi-airplane-engines-fill brand-icon"></i>
        <span v-if="!isCollapsed" class="brand-text">FlightPlan</span>
      </a>
    </div>
    
    <div class="nav-sections overflow-auto flex-grow-1">
      <ul class="nav nav-pills flex-column mb-auto">
        <li v-for="item in visibleNavItems" :key="item.id" class="nav-item">
          <a :href="item.href" 
             class="nav-link" 
             :class="{ active: currentPath === item.id }"
             :title="isCollapsed ? item.name : ''">
            <i class="bi" :class="item.icon"></i>
            <span v-if="!isCollapsed" class="nav-text">{{ item.name }}</span>
          </a>
        </li>
      </ul>
    </div>

    <div class="navbar-footer d-flex flex-column mt-auto">
      <div class="footer-links">
        <ul class="nav nav-pills flex-column">
          <li class="nav-item">
            <a href="/Settings" 
               class="nav-link settings-link" 
               :class="{ active: currentPath === 'settings' }"
               :title="isCollapsed ? 'Settings' : ''">
              <i class="bi bi-gear"></i>
              <span v-if="!isCollapsed" class="nav-text">Settings</span>
            </a>
          </li>
        </ul>
      </div>
      <div class="sidebar-footer" :class="{ 'collapsed': isCollapsed }">
        <button class="sidebar-toggle" @click="toggleNavbar" :title="isCollapsed ? 'Expand' : 'Collapse'">
          <i class="bi" :class="isCollapsed ? 'bi-chevron-right' : 'bi-chevron-left'"></i>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, onMounted } from 'vue';
import Toast from './Toast.vue';
import { fetchSettings } from '../js/dashboard-api';

const isCollapsed = ref(localStorage.getItem('navbar-collapsed') === 'true');
const pageVisibilities = ref([]);

onMounted(async () => {
  try {
    const settings = await fetchSettings();
    pageVisibilities.value = settings.pageVisibilities || [];
  } catch (e) {
    console.error('Failed to load settings in Navbar:', e);
  }
});

const toggleNavbar = () => {
  isCollapsed.value = !isCollapsed.value;
  localStorage.setItem('navbar-collapsed', isCollapsed.value);
};

const navItems = [
  { id: 'dashboard', name: 'Dashboard', href: '/Dashboard', icon: 'bi-grid-1x2-fill' },
  { id: 'myday', name: 'My Day', href: '/MyDay', icon: 'bi-sun' },
  { id: 'jira', name: 'Jira', href: '/Jira', icon: 'bi-kanban' },
  { id: 'github', name: 'Github', href: '/Github', icon: 'bi-github' },
  { id: 'tasks', name: 'Tasks', href: '/Tasks', icon: 'bi-check2-square' },
  { id: 'scheduledtasks', name: 'Schedules', href: '/ScheduledTasks', icon: 'bi-clock-history' },
  { id: 'email', name: 'Email', href: '/Email', icon: 'bi-envelope' },
  { id: 'calendar', name: 'Calendar', href: '/Calendar', icon: 'bi-calendar3' },
  { id: 'links', name: 'Links', href: '/Links', icon: 'bi-link-45deg' },
  { id: 'notepad', name: 'Notepad', href: '/Notepad', icon: 'bi-sticky' },
  { id: 'debug', name: 'Debug', href: '/Debug', icon: 'bi-bug' },
];

const visibleNavItems = computed(() => {
  return navItems.filter(item => {
    if (item.id === 'dashboard') return true;
    const visibility = pageVisibilities.value.find(p => p.id === item.id);
    return visibility ? visibility.visible : true;
  });
});

const currentPath = computed(() => {
  const path = window.location.pathname.toLowerCase().replace(/\/$/, '');
  if (path === '' || path === '/dashboard') return 'dashboard';
  if (path === '/jira') return 'jira';
  if (path === '/github') return 'github';
  if (path === '/settings') return 'settings';
  if (path === '/tasks') return 'tasks';
  if (path === '/scheduledtasks') return 'scheduledtasks';
  if (path === '/email') return 'email';
  if (path === '/calendar') return 'calendar';
  if (path === '/links') return 'links';
  if (path === '/notepad') return 'notepad';
  if (path === '/debug') return 'debug';
  return '';
});
</script>

<style scoped>
.main-navbar {
  width: 230px;
  height: 100vh;
  background-color: var(--bg-dark);
  border-right: 1px solid var(--border-primary);
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  padding: 0;
  z-index: 100;
}

.main-navbar.collapsed {
  width: 50px;
}

.navbar-header {
  height: 50px;
  padding: 0 15px;
  border-bottom: 1px solid var(--border-primary);
  display: flex;
  align-items: center;
  flex-shrink: 0;
  box-sizing: border-box;
}

.navbar-header.collapsed {
  padding: 0;
  justify-content: center;
}

.navbar-brand {
  text-decoration: none;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
}

.brand-icon {
  font-size: 1.2rem;
  color: var(--accent-blue);
  min-width: 20px;
}

.brand-text {
  font-weight: 700;
  margin-left: 10px;
  letter-spacing: 0.5px;
}

.toggle-btn {
  padding: 8px;
  color: var(--text-muted);
  text-decoration: none;
  font-size: 1.2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: color 0.2s;
  border: none;
  background: transparent;
  box-shadow: none;
}

.toggle-btn:focus {
  outline: none;
  box-shadow: none;
}

.toggle-wrapper {
  margin-bottom: 5px;
}

.toggle-btn:hover {
  color: var(--text-primary);
}

.nav-sections {
  padding: 15px 0;
}

.nav-item {
  margin: 2px 8px;
}

.main-navbar.collapsed .nav-item {
  margin: 2px 0;
}

.nav-link {
  display: flex;
  align-items: center;
  padding: 10px;
  border-radius: 8px;
  color: var(--text-muted);
  transition: all 0.2s ease;
  white-space: nowrap;
  overflow: hidden;
  text-decoration: none;
}

.main-navbar.collapsed .nav-link {
  justify-content: center;
  padding: 10px 0;
  border-radius: 0;
}

.nav-link i {
  font-size: 1.15rem;
  min-width: 20px;
  display: flex;
  justify-content: center;
}

.nav-text {
  margin-left: 10px;
  font-size: 0.85rem;
}

.nav-link:hover {
  background-color: rgba(255, 255, 255, 0.08);
  color: var(--text-primary);
  transform: translateX(2px);
}

.main-navbar.collapsed .nav-link:hover {
  transform: none;
}

.nav-link.active {
  background: rgba(88, 166, 255, 0.1);
  color: var(--accent-blue);
  font-weight: 600;
  border-left: 3px solid var(--accent-blue);
  border-radius: 0 4px 4px 0;
  margin-left: -8px;
  padding-left: 17px;
}

.main-navbar.collapsed .nav-link.active {
  margin-left: 0;
  padding-left: 0;
  border-radius: 0;
  border-left-width: 4px;
}

.navbar-footer {
  background-color: var(--bg-dark);
}

.footer-links {
  padding: 8px 0;
}

.settings-link {
  margin: 0;
}

/* Hide scrollbar for nav-sections */
.nav-sections::-webkit-scrollbar {
  display: none;
}
.nav-sections {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
</style>
