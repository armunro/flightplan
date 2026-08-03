<template>
  <div :class="['vh-100 d-flex flex-row overflow-hidden app-root', themeClass]">
    <Navbar />
    <div class="flex-grow-1 overflow-hidden d-flex flex-column main-wrapper">
      <div id="app-content" class="tasks-app-container flex-grow-1">
        <div class="tasks-sidebar" :class="{ collapsed: sidebarCollapsed }" :style="sidebarStyle">
          <div class="sidebar-header d-flex align-items-center">
            <h5 v-if="!sidebarCollapsed" class="theme-text">Projects</h5>
            <button v-if="!sidebarCollapsed" class="btn-icon ms-auto theme-text" @click="onAddProject" title="Add Project">+</button>
            <div v-else class="mx-auto theme-text">
              <i class="bi bi-check2-square"></i>
            </div>
          </div>
          <div class="project-list">
            <div v-if="loading" class="sidebar-loading theme-text">
              <div class="spinner"></div>
              <span v-if="!sidebarCollapsed">Loading projects...</span>
            </div>
            <div v-else-if="error" class="p-3 text-danger small theme-text">{{ error }}</div>
            <template v-else>
              <template v-for="project in projects" :key="project.id">
                <div 
                   class="project-item theme-text"
                   :class="{ active: selectedProjectId === project.id && selectedListId === null, 'project-drag-over-before': dropProjectPosition === 'before' && dropProjectId === project.id, 'project-drag-over-after': dropProjectPosition === 'after' && dropProjectId === project.id }"
                   @click="onSelectProject(project.id)"
                   draggable="true"
                   @dragstart="onProjectDragStart($event, project.id)"
                   @dragover.prevent="onProjectDragOver($event, project.id)"
                   @dragenter.prevent="onProjectDragEnter($event)"
                   @dragleave="onProjectDragLeave($event)"
                   @drop="onProjectDrop($event, project.id)"
                   @contextmenu.prevent="onProjectContextMenu($event, project)"
                   :title="sidebarCollapsed ? project.name : ''">
                  <div class="project-icon-wrapper" :style="{ backgroundColor: project.color }">
                    <i :class="[getProjectIconClass(project)]"></i>
                  </div>
                  <span v-if="!sidebarCollapsed" class="project-name theme-text">{{ project.name }}</span>
                  <div v-if="!sidebarCollapsed" class="project-task-counts theme-text-muted">
                    <span class="project-task-count main-count" title="Tasks (excluding subtasks)">{{ getTaskCount(project, false) }}</span>
                    <span class="count-separator">/</span>
                    <span class="project-task-count sub-count" title="Total tasks (including subtasks)">{{ getTaskCount(project, true) }}</span>
                  </div>
                </div>
                
                <!-- Sub-items: Lists -->
                <div v-if="!sidebarCollapsed && !isProjectCollapsed(project.id)" class="project-lists-subitems">
                  <div v-for="list in project.lists" :key="list.id" 
                       class="list-subitem theme-text"
                       :class="{ active: selectedListId === list.id }"
                       @click.stop="onSelectList(project.id, list.id)">
                    <i :class="[getListIconClass(list), 'me-2']" :style="{ color: list.color }"></i>
                    <span class="list-name text-truncate">{{ list.name }}</span>
                    <span class="list-count ms-auto">{{ getTaskListTaskCount(list) }}</span>
                  </div>
                </div>
              </template>
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
            <div class="controls-bar theme-border">
              <div class="project-title-area d-flex align-items-center gap-3">
                <ColorPicker :modelValue="selectedProject.color" @update:modelValue="onUpdateProjectColor($event, selectedProject)" size="sm" />
                <h2 class="mb-0 text-truncate theme-text" style="max-width: 300px;">{{ selectedProject.name }}</h2>
              </div>
              <div v-if="selectedTaskIds.length > 0" class="bulk-action-bar">
                <div class="bulk-info">
                  <span class="selected-count">{{ selectedTaskIds.length }} tasks selected</span>
                  <div class="btn-group ms-2">
                    <button class="btn theme-btn-outline btn-sm" @click="onSelectAll">Select All</button>
                    <button class="btn theme-btn-outline btn-sm" @click="onSelectNone">Select None</button>
                  </div>
                </div>
                <div class="bulk-actions">
                  <div class="btn-group">
                    <div class="dropdown d-inline-block">
                      <button class="btn btn-sm theme-btn-outline dropdown-toggle" type="button" data-bs-toggle="dropdown">
                        Status
                      </button>
                      <ul class="dropdown-menu border-primary" :class="{ 'dropdown-menu-dark': theme === 'Cosmic' }">
                        <li v-for="s in selectedProject.statuses" :key="s.id">
                          <a class="dropdown-item" href="#" @click.prevent="onBulkUpdate({ statusId: s.id })">{{ s.name }}</a>
                        </li>
                      </ul>
                    </div>
                    <div class="dropdown d-inline-block">
                      <button class="btn btn-sm theme-btn-outline dropdown-toggle" type="button" data-bs-toggle="dropdown">
                        Priority
                      </button>
                      <ul class="dropdown-menu border-primary" :class="{ 'dropdown-menu-dark': theme === 'Cosmic' }">
                        <li v-for="p in selectedProject.priorities" :key="p.id">
                          <a class="dropdown-item" href="#" @click.prevent="onBulkUpdate({ priorityId: p.id })">
                            <i :class="p.icon" :style="{ color: p.color }" class="me-2"></i>{{ p.name }}
                          </a>
                        </li>
                      </ul>
                    </div>
                    <div class="dropdown d-inline-block">
                      <button class="btn btn-sm theme-btn-outline dropdown-toggle" type="button" data-bs-toggle="dropdown">
                        Type
                      </button>
                      <ul class="dropdown-menu border-primary" :class="{ 'dropdown-menu-dark': theme === 'Cosmic' }">
                        <li v-for="t in selectedProject.taskTypes" :key="t.id">
                          <a class="dropdown-item" href="#" @click.prevent="onBulkUpdate({ taskTypeId: t.id })">{{ t.name }}</a>
                        </li>
                      </ul>
                    </div>
                    <button class="btn btn-sm theme-btn-outline" @click="showBulkDateDialog = true" title="Bulk Edit">
                      <i class="bi bi-pencil-square"></i>
                    </button>
                    <button class="btn btn-sm theme-btn-outline" @click="onShowMoveDialog()" title="Move Tasks">
                      <i class="bi bi-arrow-right-short"></i>
                    </button>
                    <button class="btn btn-sm theme-btn-outline" @click="showExportModal = true" title="Export Tasks">
                      <i class="bi bi-download"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" @click="onBulkDelete" title="Delete Tasks">
                      <i class="bi bi-trash"></i>
                    </button>
                  </div>
                </div>
                <button class="btn-close btn-close-white" @click="onSelectNone"></button>
              </div>
              <div class="d-flex align-items-center gap-2">
                <div class="dropdown">
                  <button class="btn btn-sm btn-subtle dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false" title="Project Settings">
                    <i class="bi bi-gear me-1"></i>Project
                  </button>
                  <ul class="dropdown-menu dropdown-menu-dark dropdown-menu-end shadow" style="z-index: 10001;">
                    <li>
                      <a class="dropdown-item" href="#" @click.prevent="onEditProject(selectedProject)">
                        <i class="bi bi-pencil me-2"></i>Edit Project
                      </a>
                    </li>
                    <li>
                      <a class="dropdown-item" href="#" @click.prevent="onAddList(selectedProject.id)">
                        <i class="bi bi-plus-lg me-2"></i>Add List
                      </a>
                    </li>
                  </ul>
                </div>
                <div class="dropdown">
                  <button class="btn btn-sm btn-subtle dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false" title="Customize Columns">
                    <i class="bi bi-layout-three-columns me-1"></i>Columns
                  </button>
                  <ul class="dropdown-menu dropdown-menu-dark dropdown-menu-end shadow show-on-hover" style="z-index: 10001; min-width: 250px;">
                    <li class="dropdown-header">Visible Columns</li>
                    <li v-for="(col, index) in allColumns.filter(c => c.id !== 'filler')" :key="col.id">
                      <div class="dropdown-item d-flex align-items-center justify-content-between py-1">
                        <a href="#" class="d-flex align-items-center text-white text-decoration-none flex-grow-1" @click.prevent="toggleColumnVisibility(col.id)">
                          <i class="bi me-2" :class="isColumnVisible(col.id) ? 'bi-check-square' : 'bi-square'"></i>
                          {{ col.name }}
                        </a>
                        <div class="ms-2 d-flex gap-1" v-if="isColumnVisible(col.id)">
                          <button class="btn btn-xs btn-outline-light py-0 px-1" 
                                  :disabled="visibleColumnIds.indexOf(col.id) === 0"
                                  @click.prevent.stop="moveColumn(col.id, -1)"
                                  title="Move Left">
                            <i class="bi bi-chevron-left" style="font-size: 0.7rem;"></i>
                          </button>
                          <button class="btn btn-xs btn-outline-light py-0 px-1" 
                                  :disabled="visibleColumnIds.indexOf(col.id) === visibleColumnIds.length - 1"
                                  @click.prevent.stop="moveColumn(col.id, 1)"
                                  title="Move Right">
                            <i class="bi bi-chevron-right" style="font-size: 0.7rem;"></i>
                          </button>
                        </div>
                      </div>
                    </li>
                    <li class="dropdown-divider"></li>
                    <li>
                      <a class="dropdown-item" href="#" @click.prevent="autosizeColumns">
                        <i class="bi bi-arrows-expand-vertical me-2" style="transform: rotate(90deg); display: inline-block;"></i>Autosize All
                      </a>
                    </li>
                    <li>
                      <a class="dropdown-item text-warning" href="#" @click.prevent="resetColumns">
                        <i class="bi bi-arrow-counterclockwise me-2"></i>Reset to Default
                      </a>
                    </li>
                  </ul>
                </div>
                <div class="dropdown">
                  <button class="btn btn-sm btn-subtle dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false" title="View Options">
                    <i class="bi bi-eye me-1"></i>View
                  </button>
                  <ul class="dropdown-menu dropdown-menu-dark dropdown-menu-end shadow" style="z-index: 10001;">
                    <li>
                      <a class="dropdown-item" href="#" @click.prevent="showClosed = !showClosed">
                        <i class="bi me-2" :class="showClosed ? 'bi-eye-slash' : 'bi-eye'"></i>{{ showClosed ? 'Hide Closed Tasks' : 'Show Closed Tasks' }}
                      </a>
                    </li>
                  </ul>
                </div>
              </div>
            </div>

            <div v-if="contextMenu.visible" 
                 class="context-menu shadow" 
                 :class="{ 'theme-cosmic': theme === 'Cosmic', 'theme-light': theme === 'Light' }"
                 :style="{ top: contextMenu.y + 'px', left: contextMenu.x + 'px' }"
                 @click.stop>
              <template v-if="contextMenu.task">
                <div class="context-menu-item" @click="onShowMoveDialog(contextMenu.task)">Move Task...</div>
                <div class="context-menu-separator"></div>
                <div class="context-menu-header">Due at...</div>
                <div class="context-menu-item" @click="onSetTaskDueDate('today')">Today</div>
                <div class="context-menu-item" @click="onSetTaskDueDate('this-week')">This Week</div>
                <div class="context-menu-item" @click="onSetTaskDueDate('next-week')">Next Week</div>
                <div class="context-menu-item" @click="onSetTaskDueDate('this-month')">This Month</div>
                <div class="context-menu-item" @click="onSetTaskDueDate('next-month')">Next Month</div>
                <div class="context-menu-separator"></div>
                <div class="context-menu-item delete" @click="onDeleteTaskFromMenu">Delete Task</div>
              </template>
              <template v-else-if="contextMenu.project">
                <div class="context-menu-item" @click="onAddList(contextMenu.project.id)">Add List</div>
                <div class="context-menu-item" @click="onEditProject(contextMenu.project)">Edit Project</div>
              </template>
              <template v-else-if="contextMenu.header">
                <div class="context-menu-header">Show Columns</div>
                <div v-for="col in allColumns.filter(c => c.id !== 'filler')" :key="col.id" 
                     class="context-menu-item d-flex align-items-center justify-content-between" 
                     @click="toggleColumnVisibility(col.id)">
                  <div class="d-flex align-items-center">
                    <i class="bi me-2" :class="isColumnVisible(col.id) ? 'bi-check-square' : 'bi-square'"></i>
                    {{ col.name }}
                  </div>
                  <div class="ms-2 d-flex gap-1" v-if="isColumnVisible(col.id)">
                    <button class="btn btn-xs btn-outline-light py-0 px-1" 
                            :disabled="visibleColumnIds.indexOf(col.id) === 0"
                            @click.prevent.stop="moveColumn(col.id, -1)"
                            title="Move Left">
                      <i class="bi bi-chevron-left" style="font-size: 0.7rem;"></i>
                    </button>
                    <button class="btn btn-xs btn-outline-light py-0 px-1" 
                            :disabled="visibleColumnIds.indexOf(col.id) === visibleColumnIds.length - 1"
                            @click.prevent.stop="moveColumn(col.id, 1)"
                            title="Move Right">
                      <i class="bi bi-chevron-right" style="font-size: 0.7rem;"></i>
                    </button>
                  </div>
                </div>
                <div class="context-menu-separator"></div>
                <div class="context-menu-item" @click="autosizeColumns">Autosize All</div>
              </template>
            </div>

            <task-detail v-if="selectedTask" 
                         :task="selectedTask" 
                         :project-statuses="selectedProjectStatuses" 
                         :project-task-types="selectedProjectTaskTypes" 
                         :project-priorities="selectedProjectPriorities" 
                         :project-custom-fields="selectedProject.customFields"
                         :theme="theme"
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

            <export-tasks-modal v-if="showExportModal"
                             :tasks="selectedTasksForExport"
                             :project="selectedProject"
                             :theme="theme"
                             @close="showExportModal = false"></export-tasks-modal>

            <project-dialog v-if="showProjectDialog"
                            :project="projectDialogData"
                            :is-new="projectDialogIsNew"
                            @close="showProjectDialog = false"
                            @save="onSaveProject"
                            @delete="onDeleteProject"></project-dialog>

            <list-dialog v-if="showListDialog"
                         :list="listDialogData"
                         :is-new="listDialogIsNew"
                         @close="showListDialog = false"
                         @save="onSaveList"></list-dialog>

            <div class="project-content">
              <template v-for="list in selectedProject.lists" :key="list.id">
                <div v-if="selectedListId === null || selectedListId === list.id" class="list"
                     @dragover.prevent="onListDragOver($event, list.id)"
                     @dragenter.prevent="onListDragEnter($event)"
                     @dragleave="onListDragLeave($event)"
                     @drop="onListDrop($event, list.id, selectedProject.id)"
                     :class="{
                       'list-drag-over-before': dropListPosition === 'before' && dropListId === list.id, 
                       'list-drag-over-after': dropListPosition === 'after' && dropListId === list.id,
                       'collapsed': collapsedLists.has(list.id)
                     }">
                <div class="list-header" :class="{ 'collapsed': collapsedLists.has(list.id) }"
                     draggable="true" @dragstart="onListDragStart($event, list.id)">
                  <span class="collapse-toggle" :class="{ collapsed: collapsedLists.has(list.id) }" @click="toggleListCollapse(list.id)">
                  </span>
                  <input v-if="editingListId === list.id" 
                         ref="listNameInput"
                         v-model="editingListName" 
                         class="list-name-input"
                         @blur="saveListName(list)"
                         @keyup.enter="saveListName(list)"
                         @keyup.esc="cancelEditingList"
                         @click.stop>
                  <h3 v-else @dblclick="startEditingList(list)">
                    <i :class="[getListIconClass(list), 'me-2']" :style="{ color: list.color }"></i>
                    {{ list.name }}
                    <button class="btn btn-sm btn-link text-info p-0 ms-2 edit-list-inline-btn" @click.stop="startEditingList(list)" title="Rename List">
                      <i class="bi bi-pencil" style="font-size: 0.8rem;"></i>
                    </button>
                  </h3>
                  <div class="list-actions-dropdown">
                    <button class="list-actions-btn" @click.stop="onAddTask(list.id, selectedProject)" title="Add Task">+</button>
                    <button class="list-actions-btn" @click.stop="onListMenu($event, list.id)" title="List Actions">...</button>
                    <div v-if="listMenu.visible && listMenu.listId === list.id" 
                         class="dropdown-menu show dropdown-menu-end active-list-menu" 
                         :class="{ 'dropdown-menu-dark': theme === 'Cosmic' }"
                         style="position: absolute; right: 0; top: 100%; z-index: 10000; display: block !important; border: 1px solid var(--border-primary); box-shadow: 0 4px 12px rgba(0,0,0,0.5);"
                         @click.stop>
                      <div class="dropdown-item" @click="onEditListFromMenu"><i class="bi bi-gear me-2"></i>List Details</div>
                      <div class="dropdown-item" @click="onRenameListFromMenu"><i class="bi bi-pencil me-2"></i>Rename List</div>
                      <div class="dropdown-item delete" @click="onDeleteListFromMenu"><i class="bi bi-trash me-2"></i>Delete List</div>
                    </div>
                  </div>
                </div>
                
                <div class="tasks-grid" v-if="!collapsedLists.has(list.id)">
                  <div class="tasks-header-row" :class="{ 'is-empty': getSortedTasks(list.tasks).length === 0 }" :style="gridStyle" @contextmenu.prevent="onHeaderContextMenu($event)">
                    <div class="tasks-header selection-header">
                      <input type="checkbox" :checked="isListAllSelected(list)" @change="toggleListSelectAll(list)">
                    </div>

                    <template v-for="colId in visibleColumnIds" :key="colId">
                      <div v-if="colId === 'type'" class="tasks-header sortable type-column" @click="toggleSort('taskTypeId')">
                        Type
                        <span v-if="sortBy === 'taskTypeId'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                        <div class="resizer" @mousedown.stop="startResize(0, $event)" @click.stop></div>
                      </div>
                      <div v-else-if="colId === 'name'" class="tasks-header sortable name-column" @click="toggleSort('title')">
                        Task Name
                        <span v-if="sortBy === 'title'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                        <div class="resizer" @mousedown.stop="startResize(1, $event)" @click.stop></div>
                      </div>
                      <div v-else-if="colId === 'status'" class="tasks-header sortable status-column" @click="toggleSort('statusId')">
                        Status
                        <span v-if="sortBy === 'statusId'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                        <div class="resizer" @mousedown.stop="startResize(2, $event)" @click.stop></div>
                      </div>
                      <div v-else-if="colId === 'priority'" class="tasks-header sortable priority-column" @click="toggleSort('priority')">
                        Priority
                        <span v-if="sortBy === 'priority'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                        <div class="resizer" @mousedown.stop="startResize(3, $event)" @click.stop></div>
                      </div>
                      <div v-else-if="colId === 'start'" class="tasks-header sortable start-column" @click="toggleSort('start')">
                        Start
                        <span v-if="sortBy === 'start'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                        <div class="resizer" @mousedown.stop="startResize(4, $event)" @click.stop></div>
                      </div>
                      <div v-else-if="colId === 'end'" class="tasks-header sortable end-column" @click="toggleSort('end')">
                        End
                        <span v-if="sortBy === 'end'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                        <div class="resizer" @mousedown.stop="startResize(5, $event)" @click.stop></div>
                      </div>
                      <div v-else-if="colId === 'estimate'" class="tasks-header sortable estimate-column" @click="toggleSort('estimateMinutes')">
                        Est
                        <span v-if="sortBy === 'estimateMinutes'" class="sort-icon" :class="{ desc: sortDesc }"></span>
                        <div class="resizer" @mousedown.stop="startResize(6, $event)" @click.stop></div>
                      </div>
                      <div v-else><!-- Fallback for unknown columns --></div>
                    </template>
                    
                    <div class="tasks-header"></div>
                  </div>

                    <template v-for="(task, index) in getSortedTasks(list.tasks)" :key="task.id">
                    <task-row :task="task" 
                              :depth="0" 
                              :parent-task-id="null"
                              :previous-task-id="index > 0 ? getSortedTasks(list.tasks)[index-1].id : null"
                              :projectStatuses="selectedProject.statuses" 
                              :projectTaskTypes="selectedProject.taskTypes" 
                              :projectPriorities="selectedProject.priorities" 
                              :showClosed="showClosed" 
                              :grid-style="gridStyle" 
                              :is-last="index === getSortedTasks(list.tasks).length - 1"
                              :selected-task-ids="selectedTaskIds"
                              :theme="theme"
                              :visible-column-ids="visibleColumnIds"
                              @refresh="fetchProjects" 
                              @open-task="onOpenTask($event, selectedProject.statuses, selectedProject.taskTypes, selectedProject.priorities)"
                              @toggle-select="onToggleSelect"
                              @context-menu="onTaskContextMenu"></task-row>
                  </template>
                </div>
                </div>
              </template>
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
import { ref, onMounted, onUnmounted, computed, watch, nextTick } from 'vue';
import { showToast } from './components/Toast.vue';
import Navbar from './components/Navbar.vue';
import TaskRow from './components/TaskRow.vue';
import TaskDetail from './components/TaskDetail.vue';
import MoveTaskDialog from './components/MoveTaskDialog.vue';
import BulkDateDialog from './components/BulkDateDialog.vue';
import ExportTasksModal from './components/ExportTasksModal.vue';
import ProjectDialog from './components/ProjectDialog.vue';
import ListDialog from './components/ListDialog.vue';
import ColorPicker from './components/ColorPicker.vue';
import { addTask, moveTask, copyTask, bulkMoveTasks, addList, updateList, moveList, deleteList, updateProject, addProject, moveProject, deleteProject, deleteTask, bulkDeleteTasks, updateTask as apiUpdateTask, bulkUpdateTasks } from './js/tasks-api';
import { fetchSettings } from './js/dashboard-api';
import { findTaskInProjects, formatFriendlyDate, formatEstimate } from './js/utils';

export default {
  name: 'App',
  components: {
    Navbar,
    TaskRow,
    TaskDetail,
    MoveTaskDialog,
    BulkDateDialog,
    ExportTasksModal,
    ProjectDialog,
    ListDialog,
    ColorPicker
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
    const selectedListId = ref(null);
    const collapsedProjects = ref(new Set());
    const collapsedLists = ref(new Set());
    const loading = ref(true);
    const error = ref(null);
    const theme = ref('Cosmic');
    const themeClass = computed(() => `theme-${theme.value.toLowerCase()}`);
    const showClosed = ref(false);
    const dropProjectId = ref(null);
    const projectDragCounter = ref(0);
    const dropProjectPosition = ref(null);
    const dropListId = ref(null);
    const listDragCounter = ref(0);
    const dropListPosition = ref(null);
    const selectedTask = ref(null);
    const selectedProjectStatuses = ref([]);
    const selectedProjectTaskTypes = ref([]);
    const selectedProjectPriorities = ref([]);
    const showMoveDialog = ref(false);
    const showBulkDateDialog = ref(false);
    const showExportModal = ref(false);
    const moveDialogTargetTask = ref(null);
    const moveDialogTargetTaskIds = ref(null);
    const showProjectDialog = ref(false);
    const sidebarCollapsed = ref(false);
    const editingListId = ref(null);
    const editingListName = ref('');
    const listNameInput = ref(null);
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
    selectedListId.value = loadSetting('selectedListId', null);
    const savedCollapsedLists = loadSetting('collapsedLists', []);
    collapsedLists.value = new Set(savedCollapsedLists);
    const projectDialogIsNew = ref(false);
    const projectDialogData = ref({});
    const showListDialog = ref(false);
    const listDialogIsNew = ref(false);
    const listDialogData = ref({});
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
      task: null,
      project: null,
      header: false
    });

    // Resizing state
    const selectionColumnWidth = ref(40);
    const columnWidths = ref(loadSetting('columnWidths', [100, 400, 150, 150, 180, 180, 80, 0])); // Type, Task Name, Status, Priority, Start, End, Est, Dead
    const allColumns = [
      { id: 'type', name: 'Type', field: 'taskTypeId', widthIndex: 0 },
      { id: 'name', name: 'Task Name', field: 'title', widthIndex: 1 },
      { id: 'status', name: 'Status', field: 'statusId', widthIndex: 2 },
      { id: 'priority', name: 'Priority', field: 'priorityId', widthIndex: 3 },
      { id: 'start', name: 'Start', field: 'start', widthIndex: 4 },
      { id: 'end', name: 'End', field: 'end', widthIndex: 5 },
      { id: 'estimate', name: 'Est', field: 'estimateMinutes', widthIndex: 6 },
      { id: 'filler', name: '', field: null, widthIndex: 7 }
    ];

    const projectVisibleColumns = ref(loadSetting('projectVisibleColumns', {})); // { projectId: ['type', 'name', ...] }

    const visibleColumnIds = computed({
      get: () => {
        const defaultVisible = allColumns.filter(c => c.id !== 'filler').map(c => c.id);
        if (!selectedProjectId.value) return defaultVisible;
        const saved = projectVisibleColumns.value[selectedProjectId.value];
        // Ensure it's an array and not empty
        if (!saved || !Array.isArray(saved) || saved.length === 0) return defaultVisible;
        
        // Return saved order, but filter out 'filler' and any IDs that might not exist anymore
        return saved.filter(id => id !== 'filler' && allColumns.some(c => c.id === id));
      },
      set: (val) => {
        if (!selectedProjectId.value) return;
        projectVisibleColumns.value[selectedProjectId.value] = val;
        localStorage.setItem('projectVisibleColumns', JSON.stringify(projectVisibleColumns.value));
      }
    });

    const resetColumns = () => {
      if (!selectedProjectId.value) return;
      delete projectVisibleColumns.value[selectedProjectId.value];
      localStorage.setItem('projectVisibleColumns', JSON.stringify(projectVisibleColumns.value));
      showToast('Columns reset to default', 'info');
    };

    const isColumnVisible = (columnId) => {
      if (!visibleColumnIds.value) return true;
      return visibleColumnIds.value.includes(columnId);
    };

    const toggleColumnVisibility = (columnId) => {
      const current = [...visibleColumnIds.value];
      const index = current.indexOf(columnId);
      if (index > -1) {
        current.splice(index, 1);
      } else {
        current.push(columnId);
      }
      visibleColumnIds.value = current;
    };

    const moveColumn = (columnId, direction) => {
      const current = [...visibleColumnIds.value];
      const index = current.indexOf(columnId);
      if (index === -1) return;

      const newIndex = index + direction;
      if (newIndex < 0 || newIndex >= current.length) return;

      const temp = current[index];
      current[index] = current[newIndex];
      current[newIndex] = temp;

      visibleColumnIds.value = current;
    };
    const isResizing = ref(false);
    const activeResizer = ref(-1);
    const startX = ref(0);
    const startWidth = ref(0);

    const selectedProject = computed(() => {
      return projects.value.find(p => p.id === selectedProjectId.value);
    });

    const gridStyle = computed(() => {
      const widths = [];
      widths.push(`${selectionColumnWidth.value}px`);
      
      visibleColumnIds.value.forEach((id) => {
        const col = allColumns.find(c => c.id === id);
        if (col) {
          const w = columnWidths.value[col.widthIndex];
          widths.push(`${w}px`);
        }
      });
      
      // Always add the filler column at the end to take up remaining space
      widths.push('1fr');

      return {
        display: 'grid',
        gridTemplateColumns: widths.join(' ')
      };
    });

    const autosizeColumns = () => {
      if (!selectedProject.value) return;

      const container = document.querySelector('.project-content');
      const availableWidth = container ? container.clientWidth - 40 : 1200; // 40 for selection column and some buffer

      const canvas = document.createElement('canvas');
      const context = canvas.getContext('2d');
      context.font = '0.875rem "Segoe UI", Roboto, Helvetica, Arial, sans-serif'; // Match your app's font

      const getTextWidth = (text) => {
        if (!text) return 0;
        return context.measureText(text).width;
      };

      const padding = 16; // Padding and extra space (8px left + 8px right)
      const datePadding = 48; // Increased padding for date columns to prevent "..." truncation
      const newWidths = [...columnWidths.value];

      // 0: Type
      if (isColumnVisible('type')) {
        let maxTypeWidth = getTextWidth('Type');
        selectedProject.value.taskTypes.forEach(t => {
          const w = getTextWidth(t.name);
          if (w > maxTypeWidth) maxTypeWidth = w;
        });
        // 8px left padding + 16px badge padding + 12px for icon/spacing (since right padding is 0)
        newWidths[0] = Math.ceil(maxTypeWidth + 8 + 16 + 12); 
      }

      // 1: Task Name
      if (isColumnVisible('name')) {
        let maxNameWidth = getTextWidth('Task Name');
        const checkTasks = (tasks, depth) => {
          tasks.forEach(t => {
            const w = getTextWidth(t.title) + (depth * 20) + 24; // Indent + icon space
            if (w > maxNameWidth) maxNameWidth = w;
            if (t.subtasks) checkTasks(t.subtasks, depth + 1);
          });
        };
        selectedProject.value.lists.forEach(l => {
          if (l.tasks) checkTasks(l.tasks, 0);
        });
        // 4px left padding + 8px right padding + maxNameWidth
        newWidths[1] = Math.max(200, Math.ceil(maxNameWidth + 4 + 8));
      }

      // 2: Status
      if (isColumnVisible('status')) {
        let maxStatusWidth = getTextWidth('Status');
        selectedProject.value.statuses.forEach(s => {
          const w = getTextWidth(s.name);
          if (w > maxStatusWidth) maxStatusWidth = w;
        });
        newWidths[2] = Math.ceil(maxStatusWidth + padding + 32); // Extra for badge padding and non-wrapping
      }

      // 3: Priority
      if (isColumnVisible('priority')) {
        let maxPriorityWidth = getTextWidth('Priority');
        selectedProject.value.priorities.forEach(p => {
          const w = getTextWidth(p.name);
          if (w > maxPriorityWidth) maxPriorityWidth = w;
        });
        newWidths[3] = Math.ceil(maxPriorityWidth + padding + 32);
      }

      // 4: Start
      // 5: End
      if (isColumnVisible('start') || isColumnVisible('end')) {
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
        if (isColumnVisible('start')) newWidths[4] = Math.max(120, Math.ceil(maxDateWidth + datePadding));
        if (isColumnVisible('end')) newWidths[5] = Math.max(120, Math.ceil(maxDateWidth + datePadding));
      }

      // 6: Est
      if (isColumnVisible('estimate')) {
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
      }

      // 7: Deadline (represented as 0 in columnWidths, meaning it takes 1fr)
      // No fixed width for the last column in this logic.

      // Adjust widths if total exceeds available width
      const totalFixedWidth = newWidths.slice(0, 7).reduce((a, b) => a + b, 0);
      const minLastColumnWidth = 100;
      
      if (totalFixedWidth + minLastColumnWidth > availableWidth) {
        const excess = totalFixedWidth + minLastColumnWidth - availableWidth;
        // Reduce Task Name column first (index 1)
        if (newWidths[1] - excess >= 200) {
          newWidths[1] -= excess;
        } else {
          // If we can't take it all from Task Name, set it to min and spread the rest?
          // For now, just cap it to available space even if it's below min
          newWidths[1] = Math.max(100, newWidths[1] - excess);
        }
      }

      columnWidths.value = newWidths;
      localStorage.setItem('columnWidths', JSON.stringify(newWidths));
    };

    const startResize = (index, event) => {
      // Find the column by widthIndex
      const col = allColumns.find(c => c.widthIndex === index);
      if (col && !isColumnVisible(col.id)) return;
      
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

      const sorted = [...visibleTasks].sort((a, b) => {
        let valA = a[sortBy.value];
        let valB = b[sortBy.value];

        if (sortBy.value === 'title') {
          valA = (valA || '').toString().trim().toLowerCase();
          valB = (valB || '').toString().trim().toLowerCase();
        } else if (sortBy.value === 'taskTypeId') {
          valA = a.taskTypeId === null || a.taskTypeId === undefined ? -1 : a.taskTypeId;
          valB = b.taskTypeId === null || b.taskTypeId === undefined ? -1 : b.taskTypeId;
        } else if (sortBy.value === 'statusId') {
          valA = a.statusId === null || a.statusId === undefined ? -1 : a.statusId;
          valB = b.statusId === null || b.statusId === undefined ? -1 : b.statusId;
        } else if (sortBy.value === 'priority' || sortBy.value === 'estimateMinutes') {
          if (sortBy.value === 'priority') {
            const priorities = selectedProject.value?.priorities || [];
            const pA = priorities.find(p => p.id === a.priorityId);
            const pB = priorities.find(p => p.id === b.priorityId);
            valA = pA ? pA.order : -1;
            valB = pB ? pB.order : -1;
          } else {
            valA = a.estimateMinutes === null || a.estimateMinutes === undefined ? -1 : a.estimateMinutes;
            valB = b.estimateMinutes === null || b.estimateMinutes === undefined ? -1 : b.estimateMinutes;
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

      // Maintain subtasks if any (sorting top level doesn't automatically sort subtasks)
      return sorted;
    };

    const getProjectIconClass = (project) => {
      const icon = project.icon || 'bi-folder';
      return icon.startsWith('bi-') ? `bi ${icon}` : `bi bi-${icon}`;
    };

    const getListIconClass = (list) => {
      const icon = list.icon || 'bi-list-task';
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
        await fetchProjects();
      }
    };

    const selectedTasksForExport = computed(() => {
      if (selectedTaskIds.value.length === 0) return [];
      const allTasks = [];
      projects.value.forEach(p => {
        p.lists.forEach(l => {
          const flatten = (tasks) => {
            tasks.forEach(t => {
              if (selectedTaskIds.value.includes(t.id)) allTasks.push(t);
              if (t.subtasks) flatten(t.subtasks);
            });
          };
          flatten(l.tasks);
        });
      });
      return allTasks;
    });

    const onBulkUpdate = async (data) => {
      await bulkUpdateTasks(selectedTaskIds.value, data);
      fetchProjects();
    };

    const onBulkDateUpdate = async (dates) => {
      await bulkUpdateTasks(selectedTaskIds.value, dates);
      showBulkDateDialog.value = false;
      await fetchProjects();
    };

    const fetchProjects = async () => {
      try {
        const [projectsResponse, settings] = await Promise.all([
          fetch('/api/projects'),
          fetchSettings()
        ]);
        
        projects.value = await projectsResponse.json();
        
        if (settings) {
          theme.value = settings.theme || 'Cosmic';
        }
        
        if (projects.value.length > 0) {
          if (!selectedProjectId.value || !projects.value.find(p => p.id === selectedProjectId.value)) {
            selectedProjectId.value = projects.value[0].id;
          }
        }

        if (selectedTask.value) {
          const result = findTaskInProjects(projects.value, selectedTask.value.id);
          if (result) {
            selectedTask.value = result.task;
            selectedProjectStatuses.value = result.project.statuses;
            selectedProjectTaskTypes.value = result.project.taskTypes;
            selectedProjectPriorities.value = result.project.priorities;
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

    const onAddList = (projectId) => {
      selectedProjectId.value = projectId;
      listDialogIsNew.value = true;
      listDialogData.value = { name: '', icon: '', color: '' };
      showListDialog.value = true;
      closeContextMenu();
    };

    const onEditListFromMenu = () => {
      const listId = listMenu.value.listId;
      const list = selectedProject.value.lists.find(l => l.id === listId);
      if (list) {
        listDialogIsNew.value = false;
        listDialogData.value = { ...list };
        showListDialog.value = true;
      }
      closeListMenu();
    };

    const onSaveList = async (formData) => {
      try {
        if (listDialogIsNew.value) {
          await addList(selectedProjectId.value, formData.name, formData.color, formData.icon);
          showToast('List created successfully', 'success');
        } else {
          await updateList(selectedProjectId.value, listDialogData.value.id, formData.name, formData.color, formData.icon);
          showToast('List updated successfully', 'success');
        }
        showListDialog.value = false;
        fetchProjects();
      } catch (error) {
        console.error('Error saving list:', error);
        showToast('Failed to save list', 'error');
      }
    };

    const onAddProject = () => {
      projectDialogIsNew.value = true;
      projectDialogData.value = {};
      showProjectDialog.value = true;
      closeContextMenu();
    };

    const onEditProject = (project) => {
      projectDialogIsNew.value = false;
      projectDialogData.value = { ...project };
      showProjectDialog.value = true;
      closeContextMenu();
    };

    const onSaveProject = async (formData) => {
      try {
        if (projectDialogIsNew.value) {
          await addProject(formData);
          showToast('Project created successfully', 'success');
        } else {
          await updateProject(projectDialogData.value.id, {
            name: formData.name,
            icon: formData.icon,
            color: formData.color,
            description: formData.description,
            statuses: formData.statuses,
            taskTypes: formData.taskTypes,
            priorities: formData.priorities,
            customFields: formData.customFields
          });
          showToast('Project updated successfully', 'success');
        }
        showProjectDialog.value = false;
        fetchProjects();
      } catch (error) {
        console.error('Error saving project:', error);
        showToast('Failed to save project', 'error');
      }
    };

    const onUpdateProjectName = async (e, project) => {
      await updateProject(project.id, { 
        name: e.target.innerText,
        icon: project.icon,
        color: project.color,
        description: project.description,
        statuses: project.statuses,
        taskTypes: project.taskTypes,
        priorities: project.priorities,
        customFields: project.customFields
      });
    };

    const onUpdateProjectColor = async (color, project) => {
      await updateProject(project.id, { 
        name: project.name,
        icon: project.icon,
        color: color,
        description: project.description,
        statuses: project.statuses,
        taskTypes: project.taskTypes,
        priorities: project.priorities,
        customFields: project.customFields
      });
      fetchProjects();
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
        const newPosition = y < rect.height / 2 ? 'before' : 'after';
        if (dropProjectPosition.value !== newPosition) {
          dropProjectPosition.value = newPosition;
        }
      }
    };

    const onProjectDragEnter = (e) => {
      const types = Array.from(e.dataTransfer.types).map(t => t.toLowerCase());
      if (types.includes('application/x-flightplan-project')) {
        e.preventDefault();
      }
    };

    const onProjectDragLeave = (e) => {
      if (!e.currentTarget.contains(e.relatedTarget)) {
        dropProjectId.value = null;
        dropProjectPosition.value = null;
      }
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
        e.dataTransfer.dropEffect = e.ctrlKey ? 'copy' : 'move';
        e.currentTarget.classList.add('drag-over');
      } else if (isDraggingList) {
        e.preventDefault();
        
        dropListId.value = listId;
        const rect = e.currentTarget.getBoundingClientRect();
        const y = e.clientY - rect.top;
        const newPosition = y < rect.height / 2 ? 'before' : 'after';
        if (dropListPosition.value !== newPosition) {
          dropListPosition.value = newPosition;
        }
      }
    };

    const onListDragEnter = (e) => {
      const types = Array.from(e.dataTransfer.types).map(t => t.toLowerCase());
      const isDraggingTask = types.includes('application/x-flightplan-task');
      const isDraggingList = types.includes('application/x-flightplan-list');
      if (isDraggingTask || isDraggingList) {
        e.preventDefault();
      }
    };

    const onListDragLeave = (e) => {
      if (!e.currentTarget.contains(e.relatedTarget)) {
        e.currentTarget.classList.remove('drag-over');
        dropListId.value = null;
        dropListPosition.value = null;
      }
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
          if (e.ctrlKey) {
            await copyTask(taskId, listId, null);
          } else {
            await moveTask(taskId, listId, null);
          }
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
        task: task,
        project: null,
        header: false
      };
      setTimeout(() => {
        document.addEventListener('click', closeContextMenu);
      }, 0);
    };

    const onProjectContextMenu = (e, project) => {
      contextMenu.value = {
        visible: true,
        x: e.pageX,
        y: e.pageY,
        task: null,
        project: project,
        header: false
      };
      setTimeout(() => {
        document.addEventListener('click', closeContextMenu);
      }, 0);
    };

    const onHeaderContextMenu = (e) => {
      console.log('Header context menu triggered at', e.pageX, e.pageY);
      contextMenu.value = {
        visible: true,
        x: e.pageX,
        y: e.pageY,
        task: null,
        project: null,
        header: true
      };
      setTimeout(() => {
        document.addEventListener('click', closeContextMenu);
      }, 0);
    };

    const closeContextMenu = (e) => {
      // Don't close if clicking inside the context menu itself
      if (e && e.target && e.target.closest && e.target.closest('.context-menu')) {
        return;
      }
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

    const onSetTaskDueDate = async (period) => {
      const task = contextMenu.value.task;
      if (!task) return;

      const now = new Date();
      let dueDate = new Date();

      switch (period) {
        case 'today':
          // End of today
          dueDate.setHours(23, 59, 59, 999);
          break;
        case 'this-week':
          // End of this week (Sunday)
          const dayOfWeek = now.getDay(); // 0 is Sunday
          const diffToSunday = dayOfWeek === 0 ? 0 : 7 - dayOfWeek;
          dueDate.setDate(now.getDate() + diffToSunday);
          dueDate.setHours(23, 59, 59, 999);
          break;
        case 'next-week':
          // End of next week (Next Sunday)
          const dayOfWeekNext = now.getDay();
          const diffToNextSunday = (dayOfWeekNext === 0 ? 7 : 7 - dayOfWeekNext) + 7;
          dueDate.setDate(now.getDate() + (dayOfWeekNext === 0 ? 7 : 7 - dayOfWeekNext + 7));
          dueDate.setHours(23, 59, 59, 999);
          break;
        case 'this-month':
          // End of this month
          dueDate = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59, 999);
          break;
        case 'next-month':
          // End of next month
          dueDate = new Date(now.getFullYear(), now.getMonth() + 2, 0, 23, 59, 59, 999);
          break;
      }

      // Convert local date to ISO string for backend
      const tzoffset = dueDate.getTimezoneOffset() * 60000;
      const formattedDate = (new Date(dueDate - tzoffset)).toISOString();
      
      // If task is in selectedTaskIds, update all selected tasks
      if (selectedTaskIds.value.includes(task.id)) {
        await onBulkUpdate({ end: formattedDate });
      } else {
        await onUpdateTask(task.id, { ...task, end: formattedDate });
      }
      
      fetchProjects();
      closeContextMenu();
    };

    const onUpdateTask = async (taskId, data) => {
      await apiUpdateTask(taskId, data);
      fetchProjects();
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

    const startEditingList = (list) => {
      editingListId.value = list.id;
      editingListName.value = list.name;
      nextTick(() => {
        if (listNameInput.value) {
          if (Array.isArray(listNameInput.value)) {
             listNameInput.value[0]?.focus();
             listNameInput.value[0]?.select();
          } else {
             listNameInput.value.focus();
             listNameInput.value.select();
          }
        }
      });
    };

    const cancelEditingList = () => {
      editingListId.value = null;
      editingListName.value = '';
    };

    const saveListName = async (list) => {
      if (!editingListId.value) return;
      
      const newName = editingListName.value.trim();
      if (newName && newName !== list.name) {
        await updateList(selectedProject.value.id, list.id, newName);
        fetchProjects();
      }
      cancelEditingList();
    };

    const onRenameListFromMenu = () => {
      const listId = listMenu.value.listId;
      const list = selectedProject.value.lists.find(l => l.id === listId);
      if (list) {
        startEditingList(list);
      }
      closeListMenu();
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
      console.log('TasksApp mounted. Column customization initialized.');
      console.log('Available columns:', allColumns.map(c => c.id));
      await fetchProjects();
      console.log('Projects fetched:', projects.value.length);

      // Handle deep linking from Dashboard
      const urlParams = new URLSearchParams(window.location.search);
      const projectId = urlParams.get('projectId');
      const taskId = urlParams.get('taskId');

      if (projectId) {
        selectedProjectId.value = projectId;
        if (taskId) {
          // Wait for next tick to ensure project is selected and tasks are loaded
          nextTick(() => {
            const result = findTaskInProjects(projects.value, taskId);
            if (result && result.task) {
              selectedTask.value = result.task;
            }
          });
        }
      }

      window.addEventListener('keydown', onKeyDown);
      window.addEventListener('task-added', fetchProjects);
    });
    onUnmounted(() => {
      document.removeEventListener('click', closeContextMenu);
      document.removeEventListener('click', closeListMenu);
      window.removeEventListener('keydown', onKeyDown);
      window.removeEventListener('task-added', fetchProjects);
    });

    const toggleListCollapse = (listId) => {
      if (collapsedLists.value.has(listId)) {
        collapsedLists.value.delete(listId);
      } else {
        collapsedLists.value.add(listId);
      }
      localStorage.setItem('collapsedLists', JSON.stringify(Array.from(collapsedLists.value)));
    };

    const getTaskListTaskCount = (list, includeSubtasks = false) => {
      if (!list || !list.tasks) return 0;
      
      const countTasks = (tasks) => {
        let internalCount = 0;
        tasks.forEach(task => {
          if (!task.isCompleted) {
            internalCount++;
          }
          if (includeSubtasks && task.subtasks && task.subtasks.length > 0) {
            internalCount += countTasks(task.subtasks);
          }
        });
        return internalCount;
      };

      return countTasks(list.tasks);
    };

    const getTaskCount = (project, includeSubtasks = false) => {
      if (!project || !project.lists) return 0;
      let count = 0;
      
      const countTasks = (tasks) => {
        let internalCount = 0;
        tasks.forEach(task => {
          if (!task.isCompleted) {
            internalCount++;
          }
          if (includeSubtasks && task.subtasks && task.subtasks.length > 0) {
            internalCount += countTasks(task.subtasks);
          }
        });
        return internalCount;
      };

      project.lists.forEach(list => {
        if (list.tasks) {
          count += countTasks(list.tasks);
        }
      });
      return count;
    };

    watch(selectedProjectId, (newId) => {
      if (newId) {
        localStorage.setItem('selectedProjectId', JSON.stringify(newId));
      }
      // Clear bulk selection when switching projects
      selectedTaskIds.value = [];
    });

    watch(selectedListId, (newId) => {
      localStorage.setItem('selectedListId', JSON.stringify(newId));
      selectedTaskIds.value = [];
    });

    watch(sidebarCollapsed, (newVal) => {
      localStorage.setItem('sidebarCollapsed', JSON.stringify(newVal));
    });

    const onDeleteProject = async (projectId) => {
      try {
        await deleteProject(projectId);
        showProjectDialog.value = false;
        showToast('Project deleted successfully', 'success');
        
        // If the deleted project was selected, clear selection
        if (selectedProjectId.value === projectId) {
          selectedProjectId.value = null;
          localStorage.removeItem('selectedProjectId');
        }
        
        await fetchProjects();
      } catch (error) {
        console.error('Error deleting project:', error);
        showToast('Failed to delete project', 'error');
      }
    };

    const onSelectProject = (projectId) => {
      if (selectedProjectId.value === projectId) {
        // Toggle collapse if already selected
        if (collapsedProjects.value.has(projectId)) {
          collapsedProjects.value.delete(projectId);
        } else {
          collapsedProjects.value.add(projectId);
        }
      } else {
        selectedProjectId.value = projectId;
        selectedListId.value = null; // Show all lists by default when selecting a project
        collapsedProjects.value.delete(projectId); // Expand when selecting
      }
    };

    const onSelectList = (projectId, listId) => {
      selectedProjectId.value = projectId;
      selectedListId.value = listId;
    };

    const isProjectCollapsed = (projectId) => {
      return collapsedProjects.value.has(projectId);
    };

    return {
      projects,
      collapsedLists,
      toggleListCollapse,
      getTaskCount,
      getTaskListTaskCount,
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
      sortBy,
      sortDesc,
      toggleSort,
      getSortedTasks,
      fetchProjects,
      selectedProjectId,
      selectedListId,
      selectedProject,
      onSelectProject,
      onSelectList,
      isProjectCollapsed,
      getProjectIconClass,
      getListIconClass,
      onAddTask,
      onAddList,
      onEditListFromMenu,
      onSaveList,
      onAddProject,
      onUpdateProjectName,
      onUpdateProjectColor,
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
      onProjectContextMenu,
      onHeaderContextMenu,
      resetColumns,
      onSetTaskDueDate,
      onUpdateTask,
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
      showListDialog,
      listDialogIsNew,
      listDialogData,
      onSaveProject,
      onDeleteProject,
      selectedTaskIds,
      onToggleSelect,
      onSelectAll,
      onSelectNone,
      onBulkDelete,
      onBulkUpdate,
      showExportModal,
      selectedTasksForExport,
      isListAllSelected,
      toggleListSelectAll,
      isAllSelected,
      toggleSelectAll,
      listMenu,
      onListMenu,
      isColumnVisible,
      toggleColumnVisibility,
      moveColumn,
      allColumns,
      visibleColumnIds,
      onDeleteListFromMenu,
      startEditingList,
      cancelEditingList,
      saveListName,
      onRenameListFromMenu,
      editingListId,
      editingListName,
      listNameInput,
      onEditProject,
      sidebarCollapsed,
      sidebarWidth,
      isResizingSidebar,
      sidebarStyle,
      startSidebarResize,
      startResize,
      theme,
      themeClass
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
.project-lists-subitems {
  padding-left: 20px;
  margin-bottom: 5px;
}

.list-subitem {
  display: flex;
  align-items: center;
  padding: 6px 12px;
  margin: 2px 8px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.85rem;
  color: var(--text-muted);
  transition: all 0.2s ease;
}

.list-subitem:hover {
  background-color: var(--bg-hover);
  color: var(--text-primary);
}

.list-subitem.active {
  background-color: var(--bg-selected);
  color: var(--accent-blue);
  font-weight: 500;
}

.list-subitem .list-count {
  font-size: 0.75rem;
  opacity: 0.7;
}

.project-item.active {
  background-color: var(--bg-hover);
  border-left: 3px solid var(--accent-blue);
}
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
  background-color: var(--bg-selected);
  border-left-color: var(--accent-blue);
}

.project-item .project-icon-wrapper {
}

.project-item:hover .project-icon-wrapper {
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
  font-size: var(--fs-base);
}

.project-task-counts {
  display: flex;
  align-items: center;
  background-color: var(--bg-darker);
  padding: 2px 6px;
  border-radius: 10px;
}

.project-task-count {
  font-size: var(--fs-xs);
  color: var(--text-muted);
}

.count-separator {
  font-size: var(--fs-xs);
  color: var(--border-primary);
  margin: 0 2px;
}

.sub-count {
  opacity: 0.7;
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
  margin-bottom: 0;
}

.project-content {
  flex-grow: 1;
  overflow-x: auto;
  overflow-y: visible;
  display: flex;
  flex-direction: column;
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
  position: relative;
  z-index: 1;
}

.list:has(.show) {
  z-index: 1000;
}

.list.collapsed {
  flex: 0 0 auto;
  width: auto;
  min-width: 0;
}

.list:has(.active-list-menu) .list-header {
  z-index: 2000;
}

/* Ensure that when a row has an open dropdown, the header stays below it */
.tasks-grid:has(.show) .list-header {
  z-index: 1;
}

.list-header {
  padding: 0.75rem 1rem;
  display: flex;
  align-items: center;
  position: sticky;
  left: 0;
  width: fit-content;
  min-width: 100%;
  z-index: 2;
  background-color: var(--bg-dark);
  border-top-left-radius: 8px;
  border-top-right-radius: 8px;
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
  font-size: var(--fs-base);
  font-weight: 600;
  flex-grow: 1;
}

.list-name-input {
  background: var(--bg-card);
  border: 1px solid var(--accent-blue);
  color: var(--text-primary);
  font-size: var(--fs-base);
  font-weight: 600;
  padding: 2px 4px;
  border-radius: 4px;
  flex-grow: 1;
  margin-right: 8px;
  outline: none;
}

.edit-list-inline-btn {
  opacity: 0;
  transition: opacity 0.2s;
}

.list-header:hover .edit-list-inline-btn {
  opacity: 0.7;
}

.edit-list-inline-btn:hover {
  opacity: 1 !important;
}

.tasks-grid {
  overflow: visible;
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

.list:has(.active-list-menu) .tasks-header-row {
  z-index: 1;
}

.tasks-header-row {
  background-color: var(--bg-card);
  border-bottom: 1px solid var(--border-primary);
  position: sticky;
  top: 0;
  z-index: 1;
  display: grid;
}

.tasks-header-row.is-empty {
  border-bottom: none;
  border-bottom-left-radius: 8px;
  border-bottom-right-radius: 8px;
}

/* Ensure that when a row has an open dropdown, it goes above the sticky header */
.tasks-grid:has(.show) .tasks-header-row {
  z-index: 1;
}

.tasks-header {
  padding: 0.5rem;
  font-size: var(--fs-xs);
  font-weight: 600;
  color: var(--text-muted);
  position: relative; /* For resizer positioning */
}

.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-muted);
  font-style: italic;
}

.btn-subtle {
  background: transparent;
  border: 1px solid transparent;
  color: var(--text-muted);
  padding: 4px 12px;
  border-radius: 4px;
  font-size: 0.9rem;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  cursor: pointer;
}

.btn-subtle:not(.dropdown-toggle)::after {
  display: none;
}

.btn-subtle:hover, .btn-subtle[aria-expanded="true"] {
  background-color: var(--bg-card);
  color: var(--text-primary);
  border-color: var(--border-primary);
}

.btn-subtle::after {
  margin-left: 0.5em;
  vertical-align: 0.255em;
  content: "";
  border-top: 0.3em solid;
  border-right: 0.3em solid transparent;
  border-bottom: 0;
  border-left: 0.3em solid transparent;
}
</style>
