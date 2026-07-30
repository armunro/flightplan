<template>
  <div class="main-navbar d-flex flex-column flex-shrink-0" :class="{ 'collapsed': isCollapsed }">
    <Toast />
    <HelpModal :isOpen="isHelpModalOpen" @close="isHelpModalOpen = false" />
    <AddTaskModal :isOpen="isAddTaskModalOpen" @close="isAddTaskModalOpen = false" />
    <div class="navbar-header d-flex align-items-center" :class="{ 'collapsed': isCollapsed }">
      <a href="/Dashboard" class="navbar-brand d-flex align-items-center">
        <i class="bi bi-send brand-icon"></i>
        <span v-if="!isCollapsed" class="brand-text">FlightPlan</span>
      </a>
    </div>
    
    <div class="nav-sections overflow-auto flex-grow-1">
      <ul class="nav nav-pills flex-column mb-auto">
        <li v-for="item in visibleNavItems" :key="item.id" class="nav-item">
          <a :href="item.href" 
             class="nav-link" 
             :class="{ active: currentPath === item.id }"
             :title="isCollapsed ? item.name + ' (Alt+' + item.hotkey.toUpperCase() + ')' : 'Alt+' + item.hotkey.toUpperCase()">
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
            <button class="nav-link help-link w-100" 
                    @click="isHelpModalOpen = true"
                    :title="isCollapsed ? 'Help (Alt+/)' : 'Alt+/'">
              <i class="bi bi-question-circle"></i>
              <span v-if="!isCollapsed" class="nav-text">Help</span>
            </button>
          </li>
          <li class="nav-item">
            <a href="/Settings" 
               class="nav-link settings-link" 
               :class="{ active: currentPath === 'settings' }"
               :title="isCollapsed ? 'Settings (Alt+,)' : 'Alt+,'">
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
import { computed, ref, onMounted, onBeforeUnmount } from 'vue';
import Toast from './Toast.vue';
import HelpModal from './HelpModal.vue';
import AddTaskModal from './AddTaskModal.vue';
import { fetchSettings } from '../js/dashboard-api';

const isCollapsed = ref(localStorage.getItem('navbar-collapsed') === 'true');
const pageVisibilities = ref([]);
const isHelpModalOpen = ref(false);
const isAddTaskModalOpen = ref(false);

const handleHotkeys = (e) => {
  // Use Alt key for navigation hotkeys to avoid common conflicts
  if (!e.altKey) return;

  // Ignore if user is typing in an input or textarea
  if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA' || e.target.isContentEditable) {
    return;
  }

  const hotkeyMap = {
    'd': '/Dashboard',
    'j': '/Jira',
    'g': '/Github',
    't': '/Tasks',
    's': '/ScheduledTasks',
    'e': '/Email',
    'c': '/Calendar',
    'l': '/Links',
    'n': '/Notepad',
    'b': '/Debug', // 'b' for debug/bug
    ',': '/Settings', // Alt + , is a common settings shortcut
    '/': 'help',
    'a': 'add-task'
  };

  const action = hotkeyMap[e.key.toLowerCase()];
  if (action === 'help') {
    e.preventDefault();
    isHelpModalOpen.value = !isHelpModalOpen.value;
  } else if (action === 'add-task') {
    e.preventDefault();
    isAddTaskModalOpen.value = !isAddTaskModalOpen.value;
  } else if (action) {
    e.preventDefault();
    window.location.href = action;
  }
};

const handleOpenAddTaskModal = () => {
  isAddTaskModalOpen.value = true;
};

onMounted(async () => {
  window.addEventListener('keydown', handleHotkeys);
  window.addEventListener('open-add-task-modal', handleOpenAddTaskModal);
  try {
    const settings = await fetchSettings();
    pageVisibilities.value = settings.pageVisibilities || [];
  } catch (e) {
    console.error('Failed to load settings in Navbar:', e);
  }
});

onBeforeUnmount(() => {
  window.removeEventListener('keydown', handleHotkeys);
  window.removeEventListener('open-add-task-modal', handleOpenAddTaskModal);
});

const toggleNavbar = () => {
  isCollapsed.value = !isCollapsed.value;
  localStorage.setItem('navbar-collapsed', isCollapsed.value);
};

const navItems = [
  { id: 'dashboard', name: 'Dashboard', href: '/Dashboard', icon: 'bi-speedometer2', hotkey: 'd' },
  { id: 'jira', name: 'Jira', href: '/Jira', icon: 'bi-kanban', hotkey: 'j' },
  { id: 'github', name: 'Github', href: '/Github', icon: 'bi-github', hotkey: 'g' },
  { id: 'tasks', name: 'Tasks', href: '/Tasks', icon: 'bi-check2-square', hotkey: 't' },
  { id: 'scheduledtasks', name: 'Schedules', href: '/ScheduledTasks', icon: 'bi-clock-history', hotkey: 's' },
  { id: 'email', name: 'Email', href: '/Email', icon: 'bi-envelope', hotkey: 'e' },
  { id: 'calendar', name: 'Calendar', href: '/Calendar', icon: 'bi-calendar3', hotkey: 'c' },
  { id: 'links', name: 'Links', href: '/Links', icon: 'bi-link-45deg', hotkey: 'l' },
  { id: 'notepad', name: 'Notepad', href: '/Notepad', icon: 'bi-sticky', hotkey: 'n' },
  { id: 'debug', name: 'Debug', href: '/Debug', icon: 'bi-bug', hotkey: 'b' },
];

const visibleNavItems = computed(() => {
  return navItems.filter(item => {
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

.help-link {
  background: none;
  border: none;
  margin: 0;
  cursor: pointer;
  text-align: left;
}

.help-link:focus {
  outline: none;
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
