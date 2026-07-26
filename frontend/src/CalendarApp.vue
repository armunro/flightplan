<template>
  <div class="vh-100 d-flex flex-row overflow-hidden">
    <Navbar />
    <div class="flex-grow-1 overflow-hidden d-flex flex-column">
      <div id="app-content" class="calendar-app-container flex-grow-1" :class="{ 'editing-folders': isEditingFolders }">
      <div class="calendar-sidebar d-flex flex-column" :class="{ collapsed: sidebarCollapsed }" :style="sidebarStyle">
        <div class="sidebar-header d-flex align-items-center" :class="{ 'collapsed': sidebarCollapsed }">
          <h5 v-if="!sidebarCollapsed">Calendars</h5>
          <div v-if="!sidebarCollapsed" class="d-flex align-items-center gap-1 ms-auto">
            <button class="btn-icon ms-0" @click="isEditingFolders = !isEditingFolders" :title="isEditingFolders ? 'Save Calendars' : 'Edit Calendars'">
              <i class="bi" :class="isEditingFolders ? 'bi-check-lg text-success' : 'bi-pencil-square'"></i>
            </button>
          </div>
          <i v-else class="bi bi-calendar3"></i>
        </div>
        <div class="folder-list">
          <div v-if="loading" class="sidebar-loading">
            <div class="spinner"></div>
            <span v-if="!sidebarCollapsed">Loading calendars...</span>
          </div>
          <template v-else v-for="(folder, folderIndex) in folderTree" :key="folder.id">
            <div v-if="!folder.hidden || isEditingFolders" 
                 class="folder-item" 
                 :class="{ active: selectedFolderIds.includes(folder.id), 'opacity-50': folder.hidden }" 
                 @click="isEditingFolders ? null : toggleFolder(folder)" 
                 :title="sidebarCollapsed ? folder.displayName : ''">
              <div class="folder-icon-wrapper" :style="folder.color ? { color: folder.color } : {}">
                <i class="bi" :class="getFolderIcon(folder)"></i>
              </div>
              <div v-if="!sidebarCollapsed" class="d-flex align-items-center flex-grow-1 min-w-0">
                <span v-if="!isEditingFolders" class="folder-name">{{ folder.displayName }}</span>
                <input v-else-if="isEditingFolders" 
                       class="folder-name-input" 
                       :value="folder.displayName"
                       @click.stop
                       @input="renameFolder(folder.id, $event.target.value)" />
              </div>
              
              <div v-if="isEditingFolders" class="folder-edit-controls d-flex gap-1 ms-auto">
                <div v-if="!sidebarCollapsed" class="dropdown icon-selector">
                  <button class="btn btn-sm btn-icon p-0 border-0 dropdown-toggle" type="button" data-bs-toggle="dropdown" @click.stop title="Set Icon">
                    <i class="bi bi-grid-3x3-gap"></i>
                  </button>
                  <div class="dropdown-menu p-2 icon-grid" @click.stop>
                    <div class="d-flex flex-wrap gap-2">
                      <i v-for="icon in availableIcons" 
                         :key="icon" 
                         class="bi icon-option" 
                         :class="icon"
                         @click="setIcon(folder.id, icon)"></i>
                    </div>
                  </div>
                </div>
                <ColorPicker v-if="!sidebarCollapsed" 
                             :modelValue="folder.color" 
                             @update:modelValue="setColor(folder.id, $event)"
                             size="sm" 
                             palette-placement="bottom-end" />
                <template v-if="!sidebarCollapsed">
                  <button class="btn btn-sm btn-icon p-0 border-0" @click.stop="moveFolder(folder, -1)" :disabled="folderIndex === 0" title="Move Up">
                    <i class="bi bi-chevron-up"></i>
                  </button>
                  <button class="btn btn-sm btn-icon p-0 border-0" @click.stop="moveFolder(folder, 1)" :disabled="folderIndex === folderTree.length - 1" title="Move Down">
                    <i class="bi bi-chevron-down"></i>
                  </button>
                </template>
                <button class="btn btn-sm btn-icon p-0 border-0" @click.stop="toggleFolderVisibility(folder.id)" :title="folder.hidden ? 'Show Calendar' : 'Hide Calendar'">
                  <i class="bi" :class="folder.hidden ? 'bi-eye-slash' : 'bi-eye'"></i>
                </button>
              </div>
            </div>
          </template>
        </div>
        <div class="sidebar-footer" :class="{ 'collapsed': sidebarCollapsed }">
          <button class="sidebar-toggle" @click="sidebarCollapsed = !sidebarCollapsed" :title="sidebarCollapsed ? 'Expand Menu' : 'Collapse Menu'">
            <i class="bi" :class="sidebarCollapsed ? 'bi-chevron-right' : 'bi-chevron-left'"></i>
          </button>
        </div>
      </div>

      <div v-if="!sidebarCollapsed" class="sidebar-resizer" @mousedown="startSidebarResize"></div>

      <div class="main-content">
        <div class="calendar-container">
          <div v-if="loading && folders.length === 0" class="d-flex align-items-center justify-content-center h-100">
            <div class="text-center">
              <div class="spinner-border text-info mb-2" role="status"></div>
              <div class="text-light">Loading calendars...</div>
            </div>
          </div>
          <div v-else-if="folders.length === 0" class="d-flex align-items-center justify-content-center h-100">
            <div class="text-center text-muted">
              No calendars found.
            </div>
          </div>
          <div v-else class="calendar-full-wrapper">
            <FullCalendar ref="fullCalendar" :options="calendarOptions" />
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
import ColorPicker from './components/ColorPicker.vue';
import FullCalendar from '@fullcalendar/vue3';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import listPlugin from '@fullcalendar/list';
import interactionPlugin from '@fullcalendar/interaction';

const loadSetting = (key, defaultValue) => {
  const val = localStorage.getItem(key);
  if (!val) return defaultValue;
  try {
    return JSON.parse(val);
  } catch (e) {
    return defaultValue;
  }
};

const events = ref([]);
const folders = ref([]);
const loading = ref(true);
const isEditingFolders = ref(false);
const folderPreferences = ref({}); // { folderId: { order: number, hidden: boolean, customName: string, customIcon: string, color: string } }
const saveTimeout = ref(null);
const sidebarCollapsed = ref(loadSetting('calendarSidebarCollapsed', false));
const sidebarWidth = ref(loadSetting('calendarSidebarWidth', 260));
const isResizingSidebar = ref(false);
let sidebarStartX = 0;
let sidebarStartWidth = 0;

const fullCalendar = ref(null);

const calendarOptions = computed(() => ({
  plugins: [dayGridPlugin, timeGridPlugin, listPlugin, interactionPlugin],
  initialView: 'dayGridMonth',
  headerToolbar: {
    left: 'prev,next today',
    center: 'title',
    right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
  },
  themeSystem: 'bootstrap5',
  height: '100%',
  events: fetchEventsForFullCalendar,
  eventClick: (info) => {
    if (info.event.url) {
      window.open(info.event.url, '_blank');
      info.jsEvent.preventDefault();
    }
  },
  nowIndicator: true,
  firstDay: 1, // Monday
  editable: false,
  selectable: true,
  dayMaxEvents: true
}));

const sidebarStyle = computed(() => {
  if (sidebarCollapsed.value) return {};
  return { 
    width: sidebarWidth.value + 'px',
    transition: isResizingSidebar.value ? 'none' : 'width 0.3s ease'
  };
});

watch(sidebarWidth, (newVal) => {
  localStorage.setItem('calendarSidebarWidth', JSON.stringify(newVal));
});

const startSidebarResize = (e) => {
  isResizingSidebar.value = true;
  sidebarStartX = e.clientX;
  sidebarStartWidth = sidebarWidth.value;
  
  document.addEventListener('mousemove', doSidebarResize);
  document.addEventListener('mouseup', stopSidebarResize);
  document.body.style.cursor = 'col-resize';
  document.body.style.userSelect = 'none';
  
  e.preventDefault();
  e.stopPropagation();
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
};
const selectedFolderIds = ref(loadSetting('calendarSelectedFolderIds', []));

watch(selectedFolderIds, (newVal) => {
  localStorage.setItem('calendarSelectedFolderIds', JSON.stringify(newVal));
}, { deep: true });
const currentFolderName = ref('Upcoming Events');

const folderTree = computed(() => {
  const prefs = folderPreferences.value;
  const list = folders.value.map(f => ({
    ...f,
    displayName: prefs[f.id]?.customName || f.displayName,
    order: prefs[f.id]?.order ?? 999,
    hidden: prefs[f.id]?.hidden ?? false,
    customIcon: prefs[f.id]?.customIcon,
    color: prefs[f.id]?.color
  }));

  return list.sort((a, b) => {
    if (a.order !== 999 || b.order !== 999) return a.order - b.order;
    return a.displayName.localeCompare(b.displayName);
  });
});

watch(sidebarCollapsed, (newVal) => {
  localStorage.setItem('calendarSidebarCollapsed', JSON.stringify(newVal));
});

watch(folderPreferences, (newVal) => {
  if (saveTimeout.value) clearTimeout(saveTimeout.value);
  saveTimeout.value = setTimeout(async () => {
    try {
      await fetch('/api/calendar/preferences', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newVal)
      });
    } catch (e) {
      console.error('Error saving calendar preferences:', e);
    }
  }, 500);
}, { deep: true });

async function fetchEventsForFullCalendar(info, successCallback, failureCallback) {
  try {
    const start = info.start.toISOString();
    const end = info.end.toISOString();
    
    // If no folders selected, we might want to show default calendar or nothing.
    // Based on previous logic, if nothing was selected, it fetched all.
    // Let's keep that behavior if nothing is selected, or maybe just fetch selected ones.
    
    let urls = [];
    if (selectedFolderIds.value.length === 0) {
      urls.push(`/api/calendar?start=${start}&end=${end}&top=500`);
    } else {
      selectedFolderIds.value.forEach(id => {
        urls.push(`/api/calendar?calendarId=${id}&start=${start}&end=${end}&top=500`);
      });
    }
    
    const results = await Promise.all(urls.map(url => fetch(url)));
    const allMappedEvents = [];
    
    for (const response of results) {
      if (response.ok) {
        const data = await response.json();
        const mappedEvents = data.map(ev => {
          const calendarId = ev.calendarId || 'default';
          return {
            id: ev.id,
            title: ev.subject,
            start: ev.start,
            end: ev.end,
            url: ev.webLink,
            extendedProps: {
              location: ev.location,
              calendarId: calendarId
            },
            backgroundColor: folderPreferences.value[calendarId]?.color || 'var(--accent-blue)',
            borderColor: folderPreferences.value[calendarId]?.color || 'var(--accent-blue)'
          };
        });
        allMappedEvents.push(...mappedEvents);
      }
    }
    
    successCallback(allMappedEvents);
  } catch (error) {
    console.error('Error fetching events for FullCalendar:', error);
    failureCallback(error);
  }
}

const fetchCalendars = async () => {
  loading.value = true;
  try {
    const response = await fetch('/api/calendar/folders');
    if (response.ok) {
      folders.value = await response.json();
    }
  } catch (error) {
    console.error('Error fetching calendars:', error);
  } finally {
    loading.value = false;
  }
};

const fetchPreferences = async () => {
  try {
    const response = await fetch('/api/calendar/preferences');
    if (response.ok) {
      folderPreferences.value = await response.json();
    }
  } catch (error) {
    console.error('Error fetching preferences:', error);
  }
};

const toggleFolder = (folder) => {
  if (isEditingFolders.value) return;
  const index = selectedFolderIds.value.indexOf(folder.id);
  if (index === -1) {
    selectedFolderIds.value.push(folder.id);
  } else {
    selectedFolderIds.value.splice(index, 1);
  }
  
  if (fullCalendar.value) {
    const calendarApi = fullCalendar.value.getApi();
    calendarApi.refetchEvents();
  }
};

const moveFolder = (folder, direction) => {
  if (sidebarCollapsed.value) sidebarCollapsed.value = false;
  const list = folderTree.value;
  const idx = list.findIndex(f => f.id === folder.id);
  if (idx === -1) return;

  const targetIdx = idx + direction;
  if (targetIdx < 0 || targetIdx >= list.length) return;

  const targetFolder = list[targetIdx];
  const newPrefs = { ...folderPreferences.value };
  
  list.forEach((f, i) => {
    if (!newPrefs[f.id]) newPrefs[f.id] = { order: i, hidden: false };
  });

  const oldOrder = newPrefs[folder.id].order;
  newPrefs[folder.id].order = newPrefs[targetFolder.id].order;
  newPrefs[targetFolder.id].order = oldOrder;

  folderPreferences.value = newPrefs;
};

const toggleFolderVisibility = (id) => {
  const newPrefs = { ...folderPreferences.value };
  if (!newPrefs[id]) newPrefs[id] = { order: 999, hidden: true };
  else newPrefs[id].hidden = !newPrefs[id].hidden;
  folderPreferences.value = newPrefs;
};

const renameFolder = (id, name) => {
  const newPrefs = { ...folderPreferences.value };
  if (!newPrefs[id]) newPrefs[id] = { order: 999, hidden: false, customName: name };
  else newPrefs[id].customName = name;
  folderPreferences.value = newPrefs;
};

const setIcon = (id, icon) => {
  const newPrefs = { ...folderPreferences.value };
  if (!newPrefs[id]) newPrefs[id] = { order: 999, hidden: false, customIcon: icon };
  else newPrefs[id].customIcon = icon;
  folderPreferences.value = newPrefs;
};

const setColor = (id, color) => {
  const newPrefs = { ...folderPreferences.value };
  if (!newPrefs[id]) newPrefs[id] = { order: 999, hidden: false, color: color };
  else newPrefs[id].color = color;
  folderPreferences.value = newPrefs;
};

const availableIcons = [
  'bi-calendar', 'bi-calendar-event', 'bi-calendar-check', 'bi-calendar-date', 'bi-calendar-week',
  'bi-calendar-month', 'bi-calendar-range', 'bi-calendar3', 'bi-clock', 'bi-alarm', 'bi-stopwatch',
  'bi-person', 'bi-people', 'bi-briefcase', 'bi-house', 'bi-heart', 'bi-star', 'bi-flag'
];

const getFolderIcon = (folder) => {
  if (folder.customIcon) return folder.customIcon;
  return 'bi-calendar';
};

const formatDate = (dateString, format) => {
  if (!dateString) return 'N/A';
  const date = new Date(dateString);
  if (format === 'HH:mm') {
    return date.toLocaleString('en-US', { hour: '2-digit', minute: '2-digit', hour12: false });
  }
  return date.toLocaleString('en-US', { month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit', hour12: false });
};

onMounted(() => {
  fetchPreferences();
  fetchCalendars();
});
</script>

<style>
.form-control::placeholder {
  color: #aab2bb !important;
  opacity: 0.6 !important;
}
</style>

<style scoped>
.calendar-app-container {
  display: flex;
  height: 100%;
  background-color: var(--bg-darker);
  color: var(--text-primary);
  overflow: hidden;
}

.calendar-sidebar {
  width: 230px;
  border-right: 1px solid var(--border-primary);
  display: flex;
  flex-direction: column;
  background-color: var(--bg-dark);
  overflow: hidden;
  flex-shrink: 0;
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.editing-folders .calendar-sidebar {
  border-right-color: var(--accent-blue);
  box-shadow: 2px 0 10px rgba(0, 123, 255, 0.2);
}

.calendar-sidebar.collapsed {
  width: 50px !important;
}

.sidebar-header {
  /* height and other properties moved to global.css */
}

.calendar-sidebar.collapsed .sidebar-header {
  /* alignment handled by global.css */
}

.sidebar-header h5 {
  margin: 0;
  font-weight: 600;
  white-space: nowrap;
}

.folder-list {
  flex-grow: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 0.5rem 0;
}

.folder-item {
  position: relative;
  padding: 0.5rem 0.75rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: background-color 0.2s;
  border-left: 3px solid transparent;
  min-width: 0;
}

.calendar-sidebar.collapsed .folder-item {
  padding: 0.5rem 0;
  justify-content: center;
}

.folder-item:hover {
  background-color: var(--bg-card);
}

.folder-item.active {
  background-color: rgba(88, 166, 255, 0.1);
  border-left-color: var(--accent-blue);
}

.folder-edit-controls {
  opacity: 0;
  transition: opacity 0.2s;
}

.folder-item:hover .folder-edit-controls {
  opacity: 1;
}

.calendar-sidebar.collapsed .folder-edit-controls {
  position: absolute;
  right: 0;
  top: 0;
  bottom: 0;
  background: var(--bg-dark);
  z-index: 5;
  padding: 0 4px;
}

.folder-edit-controls .btn-icon:hover {
  background-color: var(--bg-darker);
  color: var(--accent-blue);
}

.folder-name-input {
  background: var(--bg-darker);
  border: 1px solid var(--accent-blue);
  color: var(--text-primary);
  font-size: 0.9rem;
  padding: 0 4px;
  border-radius: 4px;
  width: 100%;
}

.icon-grid {
  min-width: 150px;
  max-width: 250px;
}

.icon-option {
  cursor: pointer;
  font-size: 1.2rem;
  padding: 4px;
  border-radius: 4px;
  transition: background 0.2s;
}

.icon-option:hover {
  background: var(--bg-darker);
  color: var(--accent-blue);
}

.icon-selector .dropdown-toggle::after {
  display: none;
}

.folder-icon-wrapper {
  width: 24px;
  height: 24px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 12px;
  font-size: 0.9rem;
  color: white;
  flex-shrink: 0;
}

.calendar-sidebar.collapsed .folder-icon-wrapper {
  margin-right: 0;
}

.folder-name {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  font-size: 0.95rem;
  min-width: 0;
}

.main-content {
  flex-grow: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.calendar-container {
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  padding: 0px;
}

.calendar-full-wrapper {
  flex-grow: 1;
  background-color: var(--bg-dark);
  padding: 15px;
  overflow: hidden;
}

.sidebar-footer {
  /* properties moved to global.css */
}

.sidebar-toggle {
  /* properties moved to global.css */
}

:deep(.fc) {
  --fc-border-color: var(--border-primary);
  --fc-daygrid-event-dot-width: 8px;
  --fc-list-event-dot-width: 10px;
  --fc-neutral-bg-color: var(--bg-darker);
  --fc-page-bg-color: var(--bg-dark);
  --fc-today-bg-color: rgba(88, 166, 255, 0.05);
}

:deep(.fc-header-toolbar) {
  margin-bottom: 1.5rem !important;
}

:deep(.fc-toolbar-title) {
  font-size: 1.25rem !important;
  font-weight: 700;
  color: var(--text-primary);
}

:deep(.fc-button) {
  background-color: var(--bg-card) !important;
  border: 1px solid var(--border-primary) !important;
  color: var(--text-primary) !important;
  box-shadow: none !important;
  text-transform: capitalize;
}

:deep(.fc-button-primary:not(:disabled).fc-button-active),
:deep(.fc-button-primary:not(:disabled):active) {
  background-color: var(--accent-blue) !important;
  border-color: var(--accent-blue) !important;
  color: #000 !important;
}

:deep(.fc-col-header-cell-cushion) {
  color: var(--text-muted);
  text-decoration: none;
  font-weight: 600;
  font-size: 0.9rem;
}

:deep(.fc-daygrid-day-number) {
  color: var(--text-muted);
  text-decoration: none;
  padding: 4px 8px !important;
}

:deep(.fc-day-today .fc-daygrid-day-number) {
  color: var(--accent-blue);
  font-weight: 700;
}

:deep(.fc-list-day-side-text),
:deep(.fc-list-day-text) {
  color: var(--text-primary);
  text-decoration: none;
}

:deep(.fc-list-event:hover td) {
  background-color: var(--bg-darker) !important;
}

:deep(.fc-theme-bootstrap5 a) {
  color: inherit;
  text-decoration: none;
}

.cal-grid {
    display: flex;
    flex-direction: column;
    width: 100%;
    background-color: var(--bg-darker);
    border-bottom: 1px solid var(--border-primary);
    overflow-x: hidden;
}

.cal-header-row {
    display: grid;
    grid-template-columns: 1fr 220px 200px 80px;
    gap: 0;
    background-color: var(--bg-dark);
    position: relative;
    border-bottom: 1px solid var(--border-primary);
}

.cal-header {
    font-weight: 600;
    color: var(--text-muted);
    font-size: 0.85em;
    text-transform: uppercase;
    padding: 10px 15px;
}

.cal-row {
    display: grid;
    grid-template-columns: 1fr 220px 200px 80px;
    gap: 0;
    border-bottom: 1px solid var(--border-primary);
    transition: background-color 0.15s;
}

.cal-row:last-child {
    border-bottom: none;
}

.cal-row:hover {
    background-color: var(--bg-card);
}

.cal-cell {
    padding: 12px 15px;
    display: flex;
    align-items: center;
    min-height: 60px;
}

.text-info {
  color: var(--accent-blue) !important;
}
</style>
