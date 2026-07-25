<template>
  <div class="vh-100 d-flex flex-row overflow-hidden app-root">
    <Navbar />
    <div class="flex-grow-1 overflow-hidden d-flex flex-column main-wrapper">
      <div id="app-content" class="tasks-app-container flex-grow-1">
        <div class="tasks-sidebar" :class="{ collapsed: sidebarCollapsed }" :style="sidebarStyle">
          <div class="sidebar-header d-flex align-items-center">
            <h5 v-if="!sidebarCollapsed">Projects</h5>
            <button v-if="!sidebarCollapsed" class="btn-icon ms-auto" @click="onAddProject" title="Add Project">+</button>
            <div v-else class="mx-auto">
              <i class="bi bi-check2-square"></i>
            </div>
          </div>
          <div class="project-list">
            <div v-if="loading" class="sidebar-loading">
              <div class="spinner"></div>
              <span v-if="!sidebarCollapsed">Loading projects...</span>
            </div>
            <div v-else-if="error" class="p-3 text-danger small">{{ error }}</div>
            <template v-else>
              <div v-for="project in projects" :key="project.id" 
                 class="project-item"
                 :class="{ active: selectedProjectId === project.id, 'project-drag-over-before': dropProjectPosition === 'before' && dropProjectId === project.id, 'project-drag-over-after': dropProjectPosition === 'after' && dropProjectId === project.id }"
                 @click="selectedProjectId = project.id"
                 draggable="true"
                 @dragstart="onProjectDragStart($event, project.id)"
                 @dragover.prevent="onProjectDragOver($event, project.id)"
                 @dragleave="onProjectDragLeave($event)"
                 @drop="onProjectDrop($event, project.id)"
                 :title="sidebarCollapsed ? project.name : ''">
              <div class="project-icon-wrapper" :style="{ backgroundColor: project.color }">
                <i :class="[getProjectIconClass(project)]"></i>
              </div>
              <span v-if="!sidebarCollapsed" class="project-name">{{ project.name }}</span>
              <span v-if="!sidebarCollapsed" class="project-task-count">{{ getTaskCount(project) }}</span>
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

        <div class="main-content" v-if="selectedProject">
          <div class="tasks-container">
            <div class="controls-bar">
              <div class="project-title-area">
                <h2 contenteditable="true" @blur="onUpdateProjectName($event, selectedProject)">{{ selectedProject.name }}</h2>
              </div>
              <div v-if="selectedTaskIds.length > 0" class="bulk-action-bar">
                <div class="bulk-info">
                  <span class="selected-count">{{ selectedTaskIds.length }} tasks selected</span>
                  <div class="btn-group ms-2">
                    <button class="btn btn-outline-light btn-sm" @click="onSelectAll">Select All</button>
                    <button class="btn btn-outline-light btn-sm" @click="onSelectNone">Select None</button>
                  </div>
                </div>
                <div class="bulk-actions">
                  <div class="btn-group">
                    <div class="dropdown d-inline-block">
                      <button class="btn btn-sm btn-outline-light dropdown-toggle" type="button" data-bs-toggle="dropdown">
                        Status
                      </button>
                      <ul class="dropdown-menu dropdown-menu-dark">
                        <li v-for="s in selectedProject.statuses" :key="s.id">
                          <a class="dropdown-item" href="#" @click.prevent="onBulkUpdate({ statusId: s.id })">{{ s.name }}</a>
                        </li>
                      </ul>
                    </div>
                    <div class="dropdown d-inline-block">
                      <button class="btn btn-sm btn-outline-light dropdown-toggle" type="button" data-bs-toggle="dropdown">
                        Priority
                      </button>
                      <ul class="dropdown-menu dropdown-menu-dark">
                        <li v-for="p in selectedProject.priorities" :key="p.id">
                          <a class="dropdown-item" href="#" @click.prevent="onBulkUpdate({ priorityId: p.id })">
                            <i :class="p.icon" :style="{ color: p.color }" class="me-2"></i>{{ p.name }}
                          </a>
                        </li>
                      </ul>
                    </div>
                    <div class="dropdown d-inline-block">
                      <button class="btn btn-sm btn-outline-light dropdown-toggle" type="button" data-bs-toggle="dropdown">
                        Type
                      </button>
                      <ul class="dropdown-menu dropdown-menu-dark">
                        <li v-for="t in selectedProject.taskTypes" :key="t.id">
                          <a class="dropdown-item" href="#" @click.prevent="onBulkUpdate({ taskTypeId: t.id })">{{ t.name }}</a>
                        </li>
                      </ul>
                    </div>
                    <button class="btn btn-sm btn-outline-light" @click="showBulkDateDialog = true">
                      <i class="bi bi-pencil-square"></i> Bulk Edit
                    </button>
                    <button class="btn btn-sm btn-outline-light" @click="onShowMoveDialog()">
                      <i class="bi bi-arrow-right-short"></i> Move
                    </button>
                    <button class="btn btn-sm btn-danger" @click="onBulkDelete">
                      <i class="bi bi-trash"></i> Delete
                    </button>
                  </div>
                </div>
                <button class="btn-close btn-close-white" @click="onSelectNone"></button>
              </div>
              <div class="btn-group" role="group">
                <button class="btn btn-outline-primary btn-sm" @click.prevent="onEditProject(selectedProject)">
                  <i class="bi bi-pencil-square me-1"></i>Edit Project
                </button>
                <button class="btn btn-outline-primary btn-sm" @click.prevent="onAddList(selectedProject.id)">
                  <i class="bi bi-plus-lg me-1"></i>Add List
                </button>
                <button class="btn btn-outline-info btn-sm" @click.prevent="autosizeColumns">
                  <i class="bi bi-arrows-expand-vertical me-1" style="transform: rotate(90deg); display: inline-block;"></i>Autosize
                </button>
                <button class="btn btn-sm" :class="showClosed ? 'btn-info' : 'btn-outline-info'" @click.prevent="showClosed = !showClosed">
                  <i class="bi me-1" :class="showClosed ? 'bi-eye-slash' : 'bi-eye'"></i>{{ showClosed ? 'Hide Closed' : 'Show Closed' }}
                </button>
              </div>
            </div>

            <div v-if="contextMenu.visible" 
                 class="context-menu" 
                 :style="{ top: contextMenu.y + 'px', left: contextMenu.x + 'px' }"
                 @click.stop>
            <div class="context-menu-item" @click="onShowMoveDialog(contextMenu.task)">Move Task...</div>
              <div class="context-menu-item delete" @click="onDeleteTaskFromMenu">Delete Task</div>
            </div>

            <task-detail v-if="selectedTask" 
                         :task="selectedTask" 
                         :project-statuses="selectedProjectStatuses" 
                         :project-task-types="selectedProjectTaskTypes" 
                         :project-priorities="selectedProjectPriorities" 
                         @close="selectedTask = null" 
                         @refresh="fetchProjects"></task-detail>

            <move-task-dialog v-if="showMoveDialog"
                         :task="moveDialogTargetTask"
                         :task-ids="moveDialogTargetTaskIds"
                         :projects="projects"
                         @close="showMoveDialog = false"
                         @move="onMoveTask"></move-task-dialog>

            <bulk-date-dialog v-if="showBulkDateDialog"
                             :task-ids="selectedTaskIds"
                             @close="showBulkDateDialog = false"
                             @apply="onBulkDateUpdate"></bulk-date-dialog>

            <project-dialog v-if="showProjectDialog"
                            :project="projectDialogData"
                            :is-new="projectDialogIsNew"
                            @close="showProjectDialog = false"
                            @save="onSaveProject"></project-dialog>

            <div class="project-content">
              <div v-for="list in selectedProject.lists" :key="list.id" class="list"
                   @dragover.prevent="onListDragOver($event, list.id)"
                   @dragenter.prevent="onListDragOver($event, list.id)"
                   @dragleave="onListDragLeave($event)"
                   @drop="onListDrop($event, list.id, selectedProject.id)"
                   :class="{
                     'list-drag-over-before': dropListPosition === 'before' && dropListId === list.id, 
                     'list-drag-over-after': dropListPosition === 'after' && dropListId === list.id,
                     'collapsed': collapsedLists.has(list.id)
                   }">
                <div class="list-header" :class="{ 'collapsed': collapsedLists.has(list.id) }">
                  <span class="collapse-toggle" :class="{ collapsed: collapsedLists.has(list.id) }" @click="toggleListCollapse(list.id)">
                  </span>
                  <h3 draggable="true" @dragstart="onListDragStart($event, list.id)">{{ list.name }}</h3>
                  <div class="list-actions-dropdown">
                    <button class="list-actions-btn" @click.stop="onAddTask(list.id, selectedProject)" title="Add Task">+</button>
                    <button class="list-actions-btn" @click.stop="onListMenu($event, list.id)" title="List Actions">...</button>
                    <div v-if="listMenu.visible && listMenu.listId === list.id" 
                         class="dropdown-menu show dropdown-menu-end" 
                         style="position: absolute; right: 0; top: 100%;"
                         @click.stop>
                      <div class="dropdown-item delete" @click="onDeleteListFromMenu">Delete List</div>
                    </div>
                  </div>
                </div>
                
                <div class="tasks-grid" v-if="!collapsedLists.has(list.id)">
                  <div class="tasks-header-row" :style="gridStyle">
                    <div class="tasks-header selection-header">
                      <input type="checkbox" :checked="isListAllSelected(list)" @change="toggleListSelectAll(list)">
                    </div>
                    <div class="tasks-header sortable" @click="toggleSort('title')">
                      Task Name
                      <span v-if="sortBy === 'title'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                      <div class="resizer" @mousedown.stop="startResize(0, $event)"></div>
                    </div>
                    <div class="tasks-header sortable" @click="toggleSort('taskTypeId')">
                      Type
                      <span v-if="sortBy === 'taskTypeId'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                      <div class="resizer" @mousedown.stop="startResize(1, $event)"></div>
                    </div>
                    <div class="tasks-header sortable" @click="toggleSort('statusId')">
                      Status
                      <span v-if="sortBy === 'statusId'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                      <div class="resizer" @mousedown.stop="startResize(2, $event)"></div>
                    </div>
                    <div class="tasks-header sortable" @click="toggleSort('priority')">
                      Priority
                      <span v-if="sortBy === 'priority'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                      <div class="resizer" @mousedown.stop="startResize(3, $event)"></div>
                    </div>
                    <div class="tasks-header sortable" @click="toggleSort('start')">
                      Start
                      <span v-if="sortBy === 'start'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                      <div class="resizer" @mousedown.stop="startResize(4, $event)"></div>
                    </div>
                    <div class="tasks-header sortable" @click="toggleSort('end')">
                      End
                      <span v-if="sortBy === 'end'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                      <div class="resizer" @mousedown.stop="startResize(5, $event)"></div>
                    </div>
                    <div class="tasks-header sortable" @click="toggleSort('estimateMinutes')">
                      Est
                      <span v-if="sortBy === 'estimateMinutes'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                      <div class="resizer" @mousedown.stop="startResize(6, $event)"></div>
                    </div>
                  </div>

                  <template v-for="(task, index) in getSortedTasks(list.tasks)" :key="task.id">
                    <task-row :task="task" 
                              :depth="0" 
                              :projectStatuses="selectedProject.statuses" 
                              :projectTaskTypes="selectedProject.taskTypes" 
                              :projectPriorities="selectedProject.priorities" 
                              :showClosed="showClosed" 
                              :grid-style="gridStyle" 
                              :is-last="index === getSortedTasks(list.tasks).length - 1"
                              :selected-task-ids="selectedTaskIds"
                              @refresh="fetchProjects" 
                              @open-task="onOpenTask($event, selectedProject.statuses, selectedProject.taskTypes, selectedProject.priorities)"
                              @toggle-select="onToggleSelect"
                              @context-menu="onTaskContextMenu"></task-row>
                  </template>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="tasks-container empty-state" v-else>
          Select a project from the sidebar to view tasks
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, onUnmounted, computed, watch } from 'vue';
import Navbar from './components/Navbar.vue';
import TaskRow from './components/TaskRow.vue';
import TaskDetail from './components/TaskDetail.vue';
import MoveTaskDialog from './components/MoveTaskDialog.vue';
import BulkDateDialog from './components/BulkDateDialog.vue';
import ProjectDialog from './components/ProjectDialog.vue';
import { addTask, moveTask, bulkMoveTasks, addList, moveList, deleteList, updateProject, addProject, moveProject, deleteTask, bulkDeleteTasks, bulkUpdateTasks } from './js/tasks-api';
import { findTaskInProjects, formatFriendlyDate, formatEstimate } from './js/utils';

export default {
  name: 'App',
  components: {
    Navbar,
    TaskRow,
    TaskDetail,
    MoveTaskDialog,
    BulkDateDialog,
    ProjectDialog
  },
  setup() {
    const loadSetting = (key, defaultValue) => {
      const val = localStorage.getItem(key);
      if (!val) return defaultValue;
      try {
        return JSON.parse(val);
      } catch (e) {
        return defaultValue;
      }
    };

    const projects = ref([]);
    const selectedProjectId = ref(null);
    const collapsedProjects = ref(new Set());
    const collapsedLists = ref(new Set());
    const loading = ref(true);
    const error = ref(null);
    const showClosed = ref(false);
    const dropListId = ref(null);
    const dropListPosition = ref(null);
    const dropProjectId = ref(null);
    const dropProjectPosition = ref(null);
    const selectedTask = ref(null);
    const selectedProjectStatuses = ref([]);
    const selectedProjectTaskTypes = ref([]);
    const selectedProjectPriorities = ref([]);
    const showMoveDialog = ref(false);
    const showBulkDateDialog = ref(false);
    const moveDialogTargetTask = ref(null);
    const moveDialogTargetTaskIds = ref(null);
    const showProjectDialog = ref(false);
    const sidebarCollapsed = ref(false);
    const sidebarWidth = ref(loadSetting('tasksSidebarWidth', 260));
    const isResizingSidebar = ref(false);
    let sidebarStartX = 0;
    let sidebarStartWidth = 0;

    const sidebarStyle = computed(() => {
      if (sidebarCollapsed.value) return {};
      return { 
        width: sidebarWidth.value + 'px',
        transition: isResizingSidebar.value ? 'none' : 'width 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
      };
    });

    watch(sidebarWidth, (newVal) => {
      localStorage.setItem('tasksSidebarWidth', JSON.stringify(newVal));
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

    sidebarCollapsed.value = loadSetting('sidebarCollapsed', false);
    selectedProjectId.value = loadSetting('selectedProjectId', null);
    const savedCollapsedLists = loadSetting('collapsedLists', []);
    collapsedLists.value = new Set(savedCollapsedLists);
    const projectDialogIsNew = ref(false);
    const projectDialogData = ref({});
    const selectedTaskIds = ref([]);

    const listMenu = ref({
      visible: false,
      x: 0,
      y: 0,
      listId: null
    });

    const contextMenu = ref({
      visible: false,
      x: 0,
      y: 0,
      task: null
    });

    // Resizing state
    const selectionColumnWidth = ref(40);
    const columnWidths = ref(loadSetting('columnWidths', [400, 100, 150, 150, 180, 180, 120, 0])); // Task Name, Type, Status, Priority, Start, End, Est, Dead
    const isResizing = ref(false);
    const activeResizer = ref(-1);
    const startX = ref(0);
    const startWidth = ref(0);

    const selectedProject = computed(() => {
      return projects.value.find(p => p.id === selectedProjectId.value);
    });

    const gridStyle = computed(() => {
      const widths = [...columnWidths.value];
      const template = [`${selectionColumnWidth.value}px`, ...widths.map((w, i) => i === widths.length - 1 ? '1fr' : `${w}px`)].join(' ');
      return {
        display: 'grid',
        gridTemplateColumns: template
      };
    });

    const autosizeColumns = () => {
      if (!selectedProject.value) return;

      const canvas = document.createElement('canvas');
      const context = canvas.getContext('2d');
      context.font = '14px "Noto Sans"'; // Match your app's font

      const getTextWidth = (text) => {
        if (!text) return 0;
        return context.measureText(text).width;
      };

      const padding = 24; // Padding and extra space
      const newWidths = [...columnWidths.value];

      // 0: Task Name
      let maxNameWidth = getTextWidth('Task Name');
      const checkTasks = (tasks, depth) => {
        tasks.forEach(t => {
          const w = getTextWidth(t.title) + (depth * 20) + 40; // Indent + some extra for icons/spacing
          if (w > maxNameWidth) maxNameWidth = w;
          if (t.subtasks) checkTasks(t.subtasks, depth + 1);
        });
      };
      selectedProject.value.lists.forEach(l => {
        if (l.tasks) checkTasks(l.tasks, 0);
      });
      newWidths[0] = Math.min(800, Math.max(200, Math.ceil(maxNameWidth + padding)));

      // 1: Type
      let maxTypeWidth = getTextWidth('Type');
      selectedProject.value.taskTypes.forEach(t => {
        const w = getTextWidth(t.name);
        if (w > maxTypeWidth) maxTypeWidth = w;
      });
      newWidths[1] = Math.ceil(maxTypeWidth + padding + 40);

      // 2: Status
      let maxStatusWidth = getTextWidth('Status');
      selectedProject.value.statuses.forEach(s => {
        const w = getTextWidth(s.name);
        if (w > maxStatusWidth) maxStatusWidth = w;
      });
      newWidths[2] = Math.ceil(maxStatusWidth + padding + 40); // Extra for badge padding and non-wrapping

      // 3: Priority
      let maxPriorityWidth = getTextWidth('Priority');
      selectedProject.value.priorities.forEach(p => {
        const w = getTextWidth(p.name);
        if (w > maxPriorityWidth) maxPriorityWidth = w;
      });
      newWidths[3] = Math.ceil(maxPriorityWidth + padding + 40);

      // 4: Start
      // 5: End
      const dateHeaderWidth = getTextWidth('Start'); // Both are similar
      let maxDateWidth = dateHeaderWidth;
      const checkDates = (tasks) => {
        tasks.forEach(t => {
          if (t.start) {
            const w = getTextWidth(formatFriendlyDate(t.start, false, true));
            if (w > maxDateWidth) maxDateWidth = w;
          }
          if (t.end) {
            const w = getTextWidth(formatFriendlyDate(t.end, false, true));
            if (w > maxDateWidth) maxDateWidth = w;
          }
          if (t.subtasks) checkDates(t.subtasks);
        });
      };
      selectedProject.value.lists.forEach(l => {
        if (l.tasks) checkDates(l.tasks);
      });
      newWidths[4] = Math.ceil(maxDateWidth + padding);
      newWidths[5] = Math.ceil(maxDateWidth + padding);

      // 6: Est
      let maxEstWidth = getTextWidth('Est');
      const checkEst = (tasks) => {
        tasks.forEach(t => {
          if (t.estimateMinutes) {
            const w = getTextWidth(formatEstimate(t.estimateMinutes));
            if (w > maxEstWidth) maxEstWidth = w;
          }
          if (t.subtasks) checkEst(t.subtasks);
        });
      };
      selectedProject.value.lists.forEach(l => {
        if (l.tasks) checkEst(l.tasks);
      });
      newWidths[6] = Math.max(80, Math.ceil(maxEstWidth + padding));

      columnWidths.value = newWidths;
      localStorage.setItem('columnWidths', JSON.stringify(newWidths));
    };

    const startResize = (index, event) => {
      if (index >= columnWidths.value.length - 1) return; // Don't resize the last column as it's 1fr
      isResizing.value = true;
      activeResizer.value = index;
      startX.value = event.pageX;
      startWidth.value = columnWidths.value[index]; // We resize the column to the left of the resizer
      
      document.addEventListener('mousemove', onMouseMove);
      document.addEventListener('mouseup', stopResize);
    };

    const onMouseMove = (event) => {
      if (!isResizing.value) return;
      const diff = event.pageX - startX.value;
      const newWidth = Math.max(50, startWidth.value + diff);
      columnWidths.value[activeResizer.value] = newWidth;
    };

    const stopResize = () => {
      isResizing.value = false;
      activeResizer.value = -1;
      document.removeEventListener('mousemove', onMouseMove);
      document.removeEventListener('mouseup', stopResize);
      localStorage.setItem('columnWidths', JSON.stringify(columnWidths.value));
    };

    // Sorting state
    const sortBy = ref(null);
    const sortDesc = ref(false);

    const toggleSort = (field) => {
      if (sortBy.value === field) {
        sortDesc.value = !sortDesc.value;
      } else {
        sortBy.value = field;
        sortDesc.value = false;
      }
    };

    const getSortedTasks = (tasks) => {
      if (!tasks) return [];
      const visibleTasks = showClosed.value ? tasks : tasks.filter(t => !t.isCompleted);
      if (!sortBy.value) return visibleTasks;

      return [...visibleTasks].sort((a, b) => {
        let valA = a[sortBy.value];
        let valB = b[sortBy.value];

        if (sortBy.value === 'title') {
          valA = (valA || '').toString().trim().toLowerCase();
          valB = (valB || '').toString().trim().toLowerCase();
        } else if (sortBy.value === 'priority' || sortBy.value === 'estimateMinutes') {
          if (sortBy.value === 'priority') {
            const pA = selectedProject.value.priorities.find(p => p.id === a.priorityId);
            const pB = selectedProject.value.priorities.find(p => p.id === b.priorityId);
            valA = pA ? pA.order : -1;
            valB = pB ? pB.order : -1;
          } else {
            valA = valA === null || valA === undefined ? -1 : valA;
            valB = valB === null || valB === undefined ? -1 : valB;
          }
        } else if (sortBy.value === 'start' || sortBy.value === 'end') {
          valA = valA ? new Date(valA).getTime() : 0;
          valB = valB ? new Date(valB).getTime() : 0;
        }

        if (valA === valB) return 0;
        if (valA === null || valA === undefined || valA === '') return 1;
        if (valB === null || valB === undefined || valB === '') return -1;

        let result = valA < valB ? -1 : 1;
        return sortDesc.value ? -result : result;
      });
    };

    const getProjectIconClass = (project) => {
      const icon = project.icon || 'bi-folder';
      return icon.startsWith('bi-') ? `bi ${icon}` : `bi bi-${icon}`;
    };

    const isListAllSelected = (list) => {
      const listTaskIds = [];
      const traverse = (tasks) => {
        tasks.forEach(t => {
          if (showClosed.value || !t.isCompleted) {
            listTaskIds.push(t.id);
            if (t.subtasks) traverse(t.subtasks);
          }
        });
      };
      traverse(list.tasks);
      return listTaskIds.length > 0 && listTaskIds.every(id => selectedTaskIds.value.includes(id));
    };

    const isAllSelected = computed(() => {
      if (!selectedProject.value) return false;
      return selectedProject.value.lists.every(l => isListAllSelected(l));
    });

    const lastSelectedTaskId = ref(null);

    const onToggleSelect = (payload) => {
      let taskId, shiftKey;
      if (typeof payload === 'object' && payload !== null && 'taskId' in payload) {
        taskId = payload.taskId;
        shiftKey = payload.shiftKey;
      } else {
        taskId = payload;
        shiftKey = false;
      }

      if (shiftKey && lastSelectedTaskId.value && selectedTaskIds.value.includes(lastSelectedTaskId.value)) {
        // Range selection
        const allVisibleTasks = [];
        const traverse = (tasks) => {
          getSortedTasks(tasks).forEach(t => {
            if (showClosed.value || !t.isCompleted) {
              allVisibleTasks.push(t);
              if (t.subtasks && t.subtasks.length > 0) {
                traverse(t.subtasks);
              }
            }
          });
        };

        selectedProject.value.lists.forEach(l => {
          if (!collapsedLists.value.has(l.id)) {
            traverse(l.tasks);
          }
        });

        const lastIdx = allVisibleTasks.findIndex(t => t.id === lastSelectedTaskId.value);
        const currentIdx = allVisibleTasks.findIndex(t => t.id === taskId);

        if (lastIdx !== -1 && currentIdx !== -1) {
          const start = Math.min(lastIdx, currentIdx);
          const end = Math.max(lastIdx, currentIdx);
          
          for (let i = start; i <= end; i++) {
            const id = allVisibleTasks[i].id;
            if (!selectedTaskIds.value.includes(id)) {
              selectedTaskIds.value.push(id);
            }
          }
        }
      } else {
        const index = selectedTaskIds.value.indexOf(taskId);
        if (index === -1) {
          selectedTaskIds.value.push(taskId);
        } else {
          selectedTaskIds.value.splice(index, 1);
        }
      }
      
      if (selectedTaskIds.value.includes(taskId)) {
        lastSelectedTaskId.value = taskId;
      } else {
        lastSelectedTaskId.value = null;
      }
    };

    const onSelectAll = () => {
      if (!selectedProject.value) return;
      const allTaskIds = [];
      const traverse = (tasks) => {
        tasks.forEach(t => {
          if (showClosed.value || !t.isCompleted) {
            allTaskIds.push(t.id);
            if (t.subtasks) traverse(t.subtasks);
          }
        });
      };
      selectedProject.value.lists.forEach(l => traverse(l.tasks));
      selectedTaskIds.value = allTaskIds;
    };

    const onSelectNone = () => {
      selectedTaskIds.value = [];
      lastSelectedTaskId.value = null;
    };

    const toggleListSelectAll = (list) => {
      if (isListAllSelected(list)) {
        const listTaskIds = [];
        const traverse = (tasks) => {
          tasks.forEach(t => {
            listTaskIds.push(t.id);
            if (t.subtasks) traverse(t.subtasks);
          });
        };
        traverse(list.tasks);
        selectedTaskIds.value = selectedTaskIds.value.filter(id => !listTaskIds.includes(id));
      } else {
        const listTaskIds = [];
        const traverse = (tasks) => {
          tasks.forEach(t => {
            if (showClosed.value || !t.isCompleted) {
              listTaskIds.push(t.id);
              if (t.subtasks) traverse(t.subtasks);
            }
          });
        };
        traverse(list.tasks);
        listTaskIds.forEach(id => {
          if (!selectedTaskIds.value.includes(id)) {
            selectedTaskIds.value.push(id);
          }
        });
      }
    };

    const toggleSelectAll = () => {
      if (isAllSelected.value) {
        onSelectNone();
      } else {
        onSelectAll();
      }
    };

    const onBulkDelete = async () => {
      if (confirm(`Delete ${selectedTaskIds.value.length} tasks?`)) {
        await bulkDeleteTasks(selectedTaskIds.value);
        selectedTaskIds.value = [];
        lastSelectedTaskId.value = null;
        await fetchProjects();
      }
    };

    const onBulkUpdate = async (data) => {
      await bulkUpdateTasks(selectedTaskIds.value, data);
      await fetchProjects();
    };

    const onBulkDateUpdate = async (dates) => {
      await bulkUpdateTasks(selectedTaskIds.value, dates);
      showBulkDateDialog.value = false;
      await fetchProjects();
    };

    const fetchProjects = async () => {
      try {
        const response = await fetch('/api/projects');
        projects.value = await response.json();
        
        if (projects.value.length > 0) {
          if (!selectedProjectId.value || !projects.value.find(p => p.id === selectedProjectId.value)) {
            selectedProjectId.value = projects.value[0].id;
          }
        }

        if (selectedTask.value) {
          const result = findTaskInProjects(projects.value, selectedTask.value.id);
          if (result) {
            selectedTask.value = result.task;
            selectedProjectStatuses.value = result.statuses;
            selectedProjectTaskTypes.value = result.taskTypes;
          } else {
            selectedTask.value = null;
          }
        }
        
        loading.value = false;
      } catch (err) {
        error.value = 'Error loading projects.';
        loading.value = false;
      }
    };

    const onAddTask = async (listId, project) => {
      const defaultStatusId = project.statuses?.length > 0 ? project.statuses[0].id : null;
      await addTask(listId, "", defaultStatusId);
      fetchProjects();
    };

    const onAddList = async (projectId) => {
      const name = prompt('Enter list name:');
      if (name) {
        await addList(projectId, name);
        fetchProjects();
      }
    };

    const onAddProject = () => {
      projectDialogIsNew.value = true;
      projectDialogData.value = {};
      showProjectDialog.value = true;
    };

    const onEditProject = (project) => {
      projectDialogIsNew.value = false;
      projectDialogData.value = { ...project };
      showProjectDialog.value = true;
    };

    const onSaveProject = async (formData) => {
      if (projectDialogIsNew.value) {
        await addProject(formData);
      } else {
        await updateProject(projectDialogData.value.id, {
          name: formData.name,
          icon: formData.icon,
          color: formData.color,
          description: formData.description,
          statuses: formData.statuses,
          taskTypes: formData.taskTypes,
          priorities: formData.priorities
        });
      }
      showProjectDialog.value = false;
      fetchProjects();
    };

    const onUpdateProjectName = async (e, project) => {
      await updateProject(project.id, { name: e.target.innerText });
    };

    const onProjectDragStart = (e, projectId) => {
      e.dataTransfer.setData('application/x-flightplan-project', projectId);
      e.dataTransfer.effectAllowed = 'move';
    };

    const onProjectDragOver = (e, projectId) => {
      const types = Array.from(e.dataTransfer.types).map(t => t.toLowerCase());
      if (types.includes('application/x-flightplan-project')) {
        e.preventDefault();
        dropProjectId.value = projectId;
        const rect = e.currentTarget.getBoundingClientRect();
        const y = e.clientY - rect.top;
        dropProjectPosition.value = y < rect.height / 2 ? 'before' : 'after';
      }
    };

    const onProjectDragLeave = () => {
      dropProjectId.value = null;
      dropProjectPosition.value = null;
    };

    const onProjectDrop = async (e, projectId) => {
      const types = Array.from(e.dataTransfer.types).map(t => t.toLowerCase());
      if (!types.includes('application/x-flightplan-project')) return;

      e.preventDefault();
      const draggedProjectId = e.dataTransfer.getData('application/x-flightplan-project');
      if (draggedProjectId && draggedProjectId !== projectId) {
        const position = dropProjectPosition.value === 'before' ? 0 : 1;
        await moveProject(draggedProjectId, projectId, position);
        fetchProjects();
      }
      dropProjectId.value = null;
      dropProjectPosition.value = null;
    };

    const onListDragStart = (e, listId) => {
      e.dataTransfer.setData('application/x-flightplan-list', listId);
      e.dataTransfer.effectAllowed = 'move';
    };

    const onListDragOver = (e, listId) => {
      const types = Array.from(e.dataTransfer.types).map(t => t.toLowerCase());
      const isDraggingTask = types.includes('application/x-flightplan-task');
      const isDraggingList = types.includes('application/x-flightplan-list');

      if (isDraggingTask) {
        e.preventDefault();
        if (!e.target.closest('.task-row')) {
          e.currentTarget.classList.add('drag-over');
        } else {
          e.currentTarget.classList.remove('drag-over');
        }
      } else if (isDraggingList) {
        e.preventDefault();
        dropListId.value = listId;
        const rect = e.currentTarget.getBoundingClientRect();
        const y = e.clientY - rect.top;
        dropListPosition.value = y < rect.height / 2 ? 'before' : 'after';
      }
    };

    const onListDragLeave = (e) => {
      e.currentTarget.classList.remove('drag-over');
      dropListId.value = null;
      dropListPosition.value = null;
    };

    const onListDrop = async (e, listId, projectId) => {
      const types = Array.from(e.dataTransfer.types).map(t => t.toLowerCase());
      const isDraggingTask = types.includes('application/x-flightplan-task');
      const isDraggingList = types.includes('application/x-flightplan-list');
      
      if (!isDraggingTask && !isDraggingList) return;

      e.preventDefault();
      e.currentTarget.classList.remove('drag-over');
      const taskId = e.dataTransfer.getData('application/x-flightplan-task');
      const draggedListId = e.dataTransfer.getData('application/x-flightplan-list');

      if (taskId) {
        if (!e.target.closest('.task-row')) {
          await moveTask(taskId, listId, null);
          fetchProjects();
        }
      } else if (draggedListId && draggedListId !== listId) {
        const position = dropListPosition.value === 'before' ? 0 : 1;
        await moveList(projectId, draggedListId, listId, position);
        fetchProjects();
      }

      dropListId.value = null;
      dropListPosition.value = null;
    };

    const onOpenTask = (task, statuses, taskTypes, priorities) => {
      selectedTask.value = task;
      selectedProjectStatuses.value = statuses;
      selectedProjectTaskTypes.value = taskTypes;
      selectedProjectPriorities.value = priorities;
    };

    const onTaskContextMenu = (e, task) => {
      contextMenu.value = {
        visible: true,
        x: e.pageX,
        y: e.pageY,
        task: task
      };
      setTimeout(() => {
        document.addEventListener('click', closeContextMenu);
      }, 0);
    };

    const closeContextMenu = () => {
      contextMenu.value.visible = false;
      document.removeEventListener('click', closeContextMenu);
    };

    const onDeleteTaskFromMenu = async () => {
      const task = contextMenu.value.task;
      if (task && confirm(`Are you sure you want to delete "${task.title}"?`)) {
        await deleteTask(task.id);
        fetchProjects();
      }
      closeContextMenu();
    };

    const onShowMoveDialog = (task = null) => {
      if (task) {
        moveDialogTargetTask.value = task;
        moveDialogTargetTaskIds.value = null;
      } else {
        moveDialogTargetTask.value = null;
        moveDialogTargetTaskIds.value = [...selectedTaskIds.value];
      }
      showMoveDialog.value = true;
      closeContextMenu();
    };

    const onMoveTask = async ({ taskId, taskIds, targetListId }) => {
      if (taskId) {
        await moveTask(taskId, targetListId, null);
      } else if (taskIds) {
        await bulkMoveTasks(taskIds, targetListId, null);
        selectedTaskIds.value = [];
        lastSelectedTaskId.value = null;
      }
      showMoveDialog.value = false;
      fetchProjects();
    };

    const onListMenu = (e, listId) => {
      if (listMenu.value.visible && listMenu.value.listId === listId) {
        closeListMenu();
        return;
      }
      listMenu.value = {
        visible: true,
        x: e.pageX,
        y: e.pageY,
        listId: listId
      };
      setTimeout(() => {
        document.addEventListener('click', closeListMenu);
      }, 0);
    };

    const closeListMenu = () => {
      listMenu.value.visible = false;
      document.removeEventListener('click', closeListMenu);
    };

    const onDeleteListFromMenu = async () => {
      const listId = listMenu.value.listId;
      if (listId && confirm(`Are you sure you want to delete this list and all its tasks?`)) {
        await deleteList(selectedProject.value.id, listId);
        fetchProjects();
      }
      closeListMenu();
    };

    const onKeyDown = (e) => {
      if (e.key === 'Escape') {
        if (selectedTaskIds.value.length > 0) {
          onSelectNone();
        }
      }
    };

    onMounted(async () => {
      await fetchProjects();
      window.addEventListener('keydown', onKeyDown);
    });
    onUnmounted(() => {
      document.removeEventListener('click', closeContextMenu);
      document.removeEventListener('click', closeListMenu);
      window.removeEventListener('keydown', onKeyDown);
    });

    const toggleListCollapse = (listId) => {
      if (collapsedLists.value.has(listId)) {
        collapsedLists.value.delete(listId);
      } else {
        collapsedLists.value.add(listId);
      }
      localStorage.setItem('collapsedLists', JSON.stringify(Array.from(collapsedLists.value)));
    };

    const getTaskCount = (project) => {
      if (!project || !project.lists) return 0;
      let count = 0;
      project.lists.forEach(list => {
        if (list.tasks) {
          count += list.tasks.length;
        }
      });
      return count;
    };

    watch(selectedProjectId, (newId) => {
      if (newId) {
        localStorage.setItem('selectedProjectId', JSON.stringify(newId));
      }
    });

    watch(sidebarCollapsed, (newVal) => {
      localStorage.setItem('sidebarCollapsed', JSON.stringify(newVal));
    });

    return {
      projects,
      collapsedLists,
      toggleListCollapse,
      getTaskCount,
      loading,
      error,
      showClosed,
      dropListId,
      dropListPosition,
      selectedTask,
      selectedProjectStatuses,
      selectedProjectTaskTypes,
      selectedProjectPriorities,
      columnWidths,
      gridStyle,
      autosizeColumns,
      startResize,
      sortBy,
      sortDesc,
      toggleSort,
      getSortedTasks,
      getProjectIconClass,
      fetchProjects,
      selectedProjectId,
      selectedProject,
      onAddTask,
      onAddList,
      onAddProject,
      onUpdateProjectName,
      onProjectDragStart,
      onProjectDragOver,
      onProjectDragLeave,
      onProjectDrop,
      onListDragStart,
      onListDragOver,
      onListDragLeave,
      onListDrop,
      onOpenTask,
      contextMenu,
      onTaskContextMenu,
      onDeleteTaskFromMenu,
      moveDialogTargetTask,
      moveDialogTargetTaskIds,
      onShowMoveDialog,
      onMoveTask,
      showMoveDialog,
      showBulkDateDialog,
      onBulkDateUpdate,
      showProjectDialog,
      projectDialogIsNew,
      projectDialogData,
      onSaveProject,
      selectedTaskIds,
      onToggleSelect,
      onSelectAll,
      onSelectNone,
      onBulkDelete,
      onBulkUpdate,
      isListAllSelected,
      toggleListSelectAll,
      isAllSelected,
      toggleSelectAll,
      listMenu,
      onListMenu,
      onDeleteListFromMenu,
      onEditProject,
      sidebarCollapsed,
      sidebarWidth,
      isResizingSidebar,
      sidebarStyle,
      startSidebarResize
    };
  }
};
</script>

<style>
.form-control::placeholder {
  color: #aab2bb !important;
  opacity: 0.6 !important;
}

label, .form-label {
  color: var(--text-primary) !important;
  opacity: 0.9 !important;
}
</style>

<style scoped>
.tasks-app-container {
  display: flex;
  height: 100%;
  background-color: var(--bg-darker);
  color: var(--text-primary);
}

.app-root {
  background-color: var(--bg-darker);
}

.main-wrapper {
  background-color: var(--bg-darker);
}

.tasks-sidebar {
  width: 260px;
  border-right: 1px solid var(--border-primary);
  display: flex;
  flex-direction: column;
  background-color: var(--bg-dark);
  overflow: hidden;
  flex-shrink: 0;
  z-index: 1;
}

.tasks-sidebar.collapsed .sidebar-header {
  justify-content: center;
}

.tasks-sidebar {
  width: 230px;
  background-color: var(--bg-dark);
  border-right: 1px solid var(--border-primary);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  flex-shrink: 0;
  z-index: 1;
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
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

.tasks-sidebar.collapsed {
  width: 50px !important;
}

.sidebar-header h5 {
  margin: 0;
  font-weight: 600;
  white-space: nowrap;
}

.project-list {
  flex-grow: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 0.5rem 0;
}

.project-item {
  padding: 0.75rem 1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: background-color 0.2s, padding 0.3s ease;
  border-left: 3px solid transparent;
  min-width: 0;
  overflow: hidden;
}

.tasks-sidebar.collapsed .project-item {
  padding: 0.75rem 0;
  justify-content: center;
}

.tasks-sidebar.collapsed .project-icon-wrapper {
  margin-right: 0;
}

.project-item:hover {
  background-color: var(--bg-card);
}

.project-item.active {
  background-color: rgba(88, 166, 255, 0.1);
  border-left-color: var(--accent-blue);
}

.project-item .project-icon-wrapper {
  transition: transform 0.2s;
}

.project-item:hover .project-icon-wrapper {
  transform: scale(1.1);
}

.project-icon-wrapper {
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

.project-name {
  flex-grow: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  font-size: 0.95rem;
}

.project-task-count {
  font-size: 0.8rem;
  color: var(--text-muted);
  background-color: var(--bg-darker);
  padding: 2px 6px;
  border-radius: 10px;
}

.main-content {
  flex-grow: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.tasks-container {
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.project-title-area h2 {
  outline: none;
}

.project-content {
  flex-grow: 1;
  overflow-x: auto;
  overflow-y: visible;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  align-items: stretch;
}

.list {
  flex: 0 0 auto;
  min-width: 100%;
  width: max-content;
  background-color: var(--bg-dark);
  border: 1px solid var(--border-primary);
  border-radius: 8px;
  display: flex;
  flex-direction: column;
}

.list.collapsed {
  flex: 0 0 auto;
  width: auto;
  min-width: 0;
}

.list-header {
  padding: 0.75rem 1rem;
  display: flex;
  align-items: center;
  position: sticky;
  left: 0;
  width: fit-content;
  min-width: 100%;
  z-index: 20;
}

.list-actions-dropdown {
  position: relative;
  display: flex;
  gap: 0.5rem;
}

.list-actions-btn {
  background: transparent;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  padding: 2px 8px;
  border-radius: 4px;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
}

.list-actions-btn:hover {
  background-color: var(--bg-card);
  color: var(--text-primary);
}

.list-header h3 {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  flex-grow: 1;
}

.tasks-grid {
  overflow-y: visible;
  border-top: 1px solid var(--border-primary);
}

.resizer {
  position: absolute;
  top: 0;
  bottom: 0;
  right: -2px;
  width: 4px;
  cursor: col-resize;
  background: transparent;
  z-index: 20;
}

.resizer:hover {
  background: var(--accent-blue);
}

.tasks-header-row {
  background-color: var(--bg-card);
  border-bottom: 1px solid var(--border-primary);
  position: sticky;
  top: 0;
  z-index: 10;
  display: grid;
}

.tasks-header {
  padding: 0.5rem;
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  position: relative; /* For resizer positioning */
}

.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-muted);
  font-style: italic;
}
</style>
