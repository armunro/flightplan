<template>
  <div class="vh-100 d-flex flex-row overflow-hidden app-root">
    <Navbar />
    <div class="flex-grow-1 overflow-hidden d-flex flex-column main-wrapper">
      <!-- Rule Progress Modal -->
    <div v-if="applyingRuleName" class="modal-overlay d-flex align-items-center justify-content-center">
      <div class="card bg-dark border-secondary shadow-lg" style="width: 400px;">
        <div class="card-body p-4 text-center">
          <h5 class="text-light mb-3">Applying Rule: {{ applyingRuleName }}</h5>
          <div class="progress mb-3" style="height: 10px;">
            <div class="progress-bar progress-bar-striped progress-bar-animated bg-info" 
                 role="progressbar" 
                 :style="{ width: (ruleProgressTotal > 0 ? (ruleProgress / ruleProgressTotal * 100) : 0) + '%' }">
            </div>
          </div>
          <div class="d-flex justify-content-between x-small text-light opacity-75 mb-3">
            <span>Processing {{ ruleProgress }} of {{ ruleProgressTotal }}</span>
            <span>{{ Math.round(ruleProgressTotal > 0 ? (ruleProgress / ruleProgressTotal * 100) : 0) }}%</span>
          </div>
          <div class="text-start">
            <label class="x-small text-light opacity-50 d-block mb-1">Current Item:</label>
            <div class="small text-light text-truncate" :title="ruleProgressSubject">
              {{ ruleProgressSubject || 'Pending...' }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <div id="app-content" class="email-app-container flex-grow-1" :class="{ 'editing-folders': isEditingFolders }">
        <div class="email-sidebar" :class="{ collapsed: sidebarCollapsed }" :style="sidebarStyle">
          <div class="sidebar-header">
            <h5 v-if="!sidebarCollapsed">Folders</h5>
            <div v-if="!sidebarCollapsed" class="d-flex align-items-center gap-1 ms-auto">
              <button class="btn-icon ms-0" @click="isEditingFolders = !isEditingFolders" :title="isEditingFolders ? 'Save Folders' : 'Edit Folders'">
                <i class="bi" :class="isEditingFolders ? 'bi-check-lg text-success' : 'bi-pencil-square'"></i>
              </button>
            </div>
          </div>
          <div class="folder-list">
            <div v-if="foldersLoading" class="sidebar-loading">
              <div class="spinner"></div>
              <span v-if="!sidebarCollapsed">Loading folders...</span>
            </div>
            <template v-else v-for="(folder, folderIndex) in folderTree" :key="folder.id">
              <div v-if="!folder.hidden || isEditingFolders" 
                   class="folder-item" 
                   :class="{ active: currentFolderId === folder.id, 'opacity-50': folder.hidden }" 
                   @click="isEditingFolders ? null : selectFolder(folder)" 
                   :title="sidebarCollapsed ? folder.displayName : ''"
                   :style="!sidebarCollapsed ? { paddingLeft: (1 + folder.level * 1.5) + 'rem' } : {}">
                <div class="folder-icon-wrapper" :style="folder.color ? { color: folder.color } : {}">
                  <i class="bi" :class="getFolderIcon(folder)"></i>
                </div>
                <div v-if="!sidebarCollapsed" class="d-flex align-items-center flex-grow-1 min-w-0">
                  <span v-if="!isEditingFolders" class="folder-name">{{ folder.displayName }}</span>
                  <input v-else-if="isEditingFolders" 
                         class="folder-name-input" 
                         :value="folder.displayName"
                         @click.stop
                         @input="renameFolder(folder.id, $event.target.value)"
                         @blur="isEditingFolders = isEditingFolders" />
                  <span v-if="folder.unreadItemCount > 0 && !isEditingFolders" class="folder-count ms-2">{{ folder.unreadItemCount }}</span>
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
                  <div v-if="!sidebarCollapsed" class="dropdown color-selector">
                    <button class="btn btn-sm btn-icon p-0 border-0 dropdown-toggle" type="button" data-bs-toggle="dropdown" @click.stop title="Set Color">
                      <i class="bi bi-palette" :style="folder.color ? { color: folder.color } : {}"></i>
                    </button>
                    <div class="dropdown-menu p-2 color-grid" @click.stop>
                      <div class="d-flex flex-wrap gap-2">
                        <div v-for="color in availableColors" 
                             :key="color" 
                             class="color-option" 
                             :style="{ backgroundColor: color || 'transparent' }"
                             :class="{ 'border': !color, 'active': folder.color === color }"
                             @click="setColor(folder.id, color)">
                          <i v-if="!color" class="bi bi-x small text-danger"></i>
                        </div>
                      </div>
                    </div>
                  </div>
                  <template v-if="!sidebarCollapsed">
                    <button class="btn btn-sm btn-icon p-0 border-0" @click.stop="moveFolder(folder, -1)" :disabled="folderIndex === 0" title="Move Up">
                      <i class="bi bi-chevron-up"></i>
                    </button>
                    <button class="btn btn-sm btn-icon p-0 border-0" @click.stop="moveFolder(folder, 1)" :disabled="folderIndex === folderTree.length - 1" title="Move Down">
                      <i class="bi bi-chevron-down"></i>
                    </button>
                  </template>
                  <button class="btn btn-sm btn-icon p-0 border-0" @click.stop="toggleFolderVisibility(folder.id)" :title="folder.hidden ? 'Show Folder' : 'Hide Folder'">
                    <i class="bi" :class="folder.hidden ? 'bi-eye-slash' : 'bi-eye'"></i>
                  </button>
                </div>
              </div>
            </template>
          </div>
          <div class="sidebar-footer">
            <button class="btn-icon sidebar-toggle" @click="sidebarCollapsed = !sidebarCollapsed" :title="sidebarCollapsed ? 'Expand Menu' : 'Collapse Menu'">
              <i class="bi" :class="sidebarCollapsed ? 'bi-chevron-right' : 'bi-chevron-left'"></i>
            </button>
          </div>
        </div>
        <div v-if="!sidebarCollapsed" class="sidebar-resizer" @mousedown="startSidebarResize"></div>

      <div class="main-content">
        <div class="email-container">
          <div class="controls-bar">
            <div class="page-title-area">
              <h2 class="fw-bold mb-0">{{ currentFolderName }}</h2>
            </div>
            <div class="d-flex align-items-center gap-2">
              <div class="input-group input-group-sm">
                <button class="btn btn-outline-secondary" @click="showRulesManager = true" title="Manage Rules">
                  <i class="bi bi-gear"></i>
                </button>
                <span class="input-group-text bg-dark border-secondary text-light">
                  <i class="bi bi-envelope me-1"></i> {{ emails.length }}
                </span>
                <select v-model="pageSize" class="form-select bg-dark border-secondary text-light" style="width: auto;">
                  <option :value="10">10</option>
                  <option :value="20">20</option>
                  <option :value="50">50</option>
                  <option :value="100">100</option>
                </select>
                <button v-if="rules.length > 0" 
                        class="btn btn-outline-secondary dropdown-toggle" 
                        type="button" 
                        data-bs-toggle="dropdown" 
                        aria-expanded="false">
                  Apply Rules
                </button>
                <ul class="dropdown-menu dropdown-menu-dark dropdown-menu-end">
                  <li v-for="rule in rules" :key="rule.name">
                    <a class="dropdown-item" href="#" @click.prevent="applyToAll(rule.name)" :title="'Apply ' + rule.name + ' to all emails'">
                      Apply {{ rule.name }}
                    </a>
                  </li>
                </ul>
              </div>
            </div>
          </div>

          <div class="email-content-scrollable">
            <div v-if="loading" class="text-center py-5">
              <div class="spinner-border text-info" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
            </div>
            <div v-else-if="emails.length === 0" class="card bg-dark border-secondary">
              <div class="card-body text-center py-5">
                <p class="text-light opacity-50 mb-0">No emails found or failed to fetch emails.</p>
              </div>
            </div>
            <div v-else class="email-list">
              <div v-for="email in getSortedEmails(emails)" :key="email.id" class="email-item" :class="{ 'unread': !email.isRead }">
                <div class="email-item-content">
                  <div class="email-item-top">
                    <div class="d-flex align-items-center gap-2">
                      <span class="email-sender" :title="email.fromAddress">{{ email.from }}</span>
                      <span class="email-address text-light small opacity-75">&lt;{{ email.fromAddress }}&gt;</span>
                      <div v-if="email.matchingRules && email.matchingRules.length > 0" class="email-item-tags d-flex gap-1 ms-1">
                        <span v-for="rule in email.matchingRules" 
                              :key="rule.name" 
                              class="rule-tag"
                              :style="rule.color ? { backgroundColor: rule.color } : {}">
                          {{ rule.name }}
                        </span>
                      </div>
                    </div>
                    <span class="email-date">{{ formatFriendlyDate(email.receivedDateTime) }}</span>
                  </div>
                  <div class="email-item-details">
                    <span class="subject-text" :title="email.subject">{{ email.subject }}</span>
                    <span class="preview-separator" v-if="email.subject && email.bodyPreview"> - </span>
                    <span class="email-preview-text">{{ email.bodyPreview }}</span>
                  </div>
                </div>

                <div v-if="email.matchingRules && email.matchingRules.length > 0" 
                     class="rule-border-bar"
                     :style="{ backgroundColor: email.matchingRules[0].color || 'var(--accent-blue)' }">
                </div>
                
                <div class="email-item-actions">
                  <div class="dropdown">
                    <button class="btn btn-link p-1 text-muted action-btn" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                      <i class="bi bi-three-dots"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-dark">
                      <li><a class="dropdown-item small" href="#" @click.prevent="createRuleFromEmail(email.id)">
                        <i class="bi bi-filter me-2"></i> Create Rule
                      </a></li>
                      <li><a class="dropdown-item small" href="#" @click.prevent="createTask(email)">
                        <i class="bi bi-check2-square me-2"></i> Create Task
                      </a></li>
                      <li v-if="rules.length > 0" class="dropdown-submenu submenu-left">
                        <div class="dropdown-item small d-flex align-items-center">
                          <i class="bi bi-chevron-left small me-2"></i>
                          <span><i class="bi bi-lightning me-2"></i> Apply Rule</span>
                        </div>
                        <ul class="dropdown-menu dropdown-menu-dark">
                          <li v-for="rule in rules" :key="rule.name">
                            <a class="dropdown-item small" href="#" @click.prevent="applyRule(email.id, rule.name)">{{ rule.name }}</a>
                          </li>
                        </ul>
                      </li>
                      <li v-if="rules.length > 0" class="dropdown-submenu submenu-left">
                        <div class="dropdown-item small d-flex align-items-center">
                          <i class="bi bi-chevron-left small me-2"></i>
                          <span><i class="bi bi-plus-circle me-2"></i> Add Sender to Rule</span>
                        </div>
                        <ul class="dropdown-menu dropdown-menu-dark">
                          <li v-for="rule in rules" :key="rule.name">
                            <a class="dropdown-item small" href="#" @click.prevent="addSenderToRule(rule.name, email.fromAddress, email.subject)">{{ rule.name }}</a>
                          </li>
                        </ul>
                      </li>
                      <li><hr class="dropdown-divider"></li>
                      <li><a class="dropdown-item small text-info" :href="email.webLink" target="_blank">
                        <i class="bi bi-box-arrow-up-right me-2"></i> Open in Graph
                      </a></li>
                      <li><a class="dropdown-item small text-danger" href="#" @click.prevent="deleteEmail(email.id)">
                        <i class="bi bi-trash me-2"></i> Delete
                      </a></li>
                    </ul>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Rules Manager Modal -->
    <div v-if="showRulesManager" class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.7)">
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content bg-dark text-light border-secondary">
          <div class="modal-header border-secondary">
            <h5 class="modal-title">Manage Email Rules</h5>
            <button type="button" class="btn-close btn-close-white" @click="showRulesManager = false"></button>
          </div>
          <div class="modal-body overflow-auto" style="max-height: 70vh;">
            <div v-if="!editingRule">
              <div class="d-flex justify-content-between align-items-center mb-3">
                <p class="mb-0 text-light opacity-75">Configure automated actions for incoming emails.</p>
                <button class="btn btn-sm btn-info" @click="startCreateRule">
                  <i class="bi bi-plus-lg me-1"></i> Add Rule
                </button>
              </div>
              <div class="list-group list-group-flush border-top border-secondary">
                <div v-for="rule in rules" :key="rule.name" class="list-group-item bg-transparent text-light border-secondary d-flex align-items-center py-3">
                  <div class="rule-color-indicator me-3" :style="{ backgroundColor: rule.color || '#3498db' }"></div>
                  <div class="flex-grow-1">
                    <h6 class="mb-0">{{ rule.name }}</h6>
                    <small class="text-light opacity-75">{{ rule.filters.length }} filters, {{ rule.actions.length }} actions</small>
                  </div>
                  <div class="d-flex gap-2">
                    <button class="btn btn-sm btn-outline-light" @click="editRule(rule)">Edit</button>
                    <button class="btn btn-sm btn-outline-danger" @click="deleteRule(rule.name)">Delete</button>
                  </div>
                </div>
              </div>
            </div>

            <!-- Rule Editor -->
            <div v-else>
              <div class="mb-3">
                <label class="form-label small text-light opacity-75">Rule Name</label>
                <input v-model="editingRule.name" class="form-control bg-darker border-secondary text-light" placeholder="e.g. Work Invoices" />
              </div>
              <div class="mb-3">
                <label class="form-label small text-light opacity-75">Tag Color</label>
                <div class="d-flex flex-wrap gap-2">
                  <div v-for="color in availableColors.filter(c => c)" 
                       :key="color" 
                       class="color-option" 
                       :style="{ backgroundColor: color }"
                       :class="{ 'active': editingRule.color === color }"
                       @click="editingRule.color = color">
                  </div>
                </div>
              </div>
              
              <div class="mb-3">
                <label class="form-label d-flex justify-content-between align-items-center small text-light opacity-75">
                  Filters
                  <button class="btn btn-sm btn-link text-info p-0" @click="addFilter">Add Filter</button>
                </label>
                <div v-for="(filter, fIdx) in editingRule.filters" :key="fIdx" class="card bg-darker border-secondary mb-2">
                  <div class="card-body p-2">
                    <div class="d-flex justify-content-between mb-2">
                      <span class="small text-info">Criteria #{{ fIdx + 1 }}</span>
                      <button class="btn-close btn-close-white" style="font-size: 0.6rem" @click="editingRule.filters.splice(fIdx, 1)"></button>
                    </div>
                    <div class="mb-2">
                      <label class="x-small text-light opacity-75 d-block">From (one per line)</label>
                      <textarea class="form-control form-control-sm bg-dark border-secondary text-light" 
                                :value="filter.from?.join('\n')"
                                @input="e => filter.from = e.target.value.split('\n')"
                                rows="2"></textarea>
                    </div>
                    <div class="mb-2">
                      <label class="x-small text-light opacity-75 d-block">Subject Contains (one per line)</label>
                      <textarea class="form-control form-control-sm bg-dark border-secondary text-light" 
                                :value="filter.subjectContains?.join('\n')"
                                @input="e => filter.subjectContains = e.target.value.split('\n')"
                                rows="2"></textarea>
                    </div>
                  </div>
                </div>
              </div>

              <div class="mb-3">
                <label class="form-label d-flex justify-content-between align-items-center small text-light opacity-75">
                  Actions
                  <button class="btn btn-sm btn-link text-info p-0" @click="addAction">Add Action</button>
                </label>
                <div v-for="(action, aIdx) in editingRule.actions" :key="aIdx" class="d-flex gap-2 mb-2 align-items-center">
                  <select :value="normalizeActionType(action.type)" 
                          @change="action.type = $event.target.value"
                          class="form-select form-select-sm bg-dark border-secondary text-light">
                    <option v-for="opt in actionTypeOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
                  </select>
                  <FolderSelect v-if="normalizeActionType(action.type) == 3" 
                                v-model="action.value" 
                                :folders="folders" 
                                :folder-preferences="folderPreferences" />
                  <input v-else-if="normalizeActionType(action.type) == 1" 
                         v-model="action.value" 
                         class="form-control form-control-sm bg-dark border-secondary text-light" 
                         placeholder="Category Name" />
                  <button class="btn btn-sm btn-outline-danger" @click="editingRule.actions.splice(aIdx, 1)">
                    <i class="bi bi-trash"></i>
                  </button>
                </div>
              </div>
            </div>
          </div>
          <div class="modal-footer border-secondary">
            <button v-if="!editingRule" type="button" class="btn btn-secondary" @click="showRulesManager = false">Close</button>
            <template v-else>
              <button type="button" class="btn btn-secondary" @click="editingRule = null">Cancel</button>
              <button type="button" class="btn btn-info" @click="saveRule">Save Rule</button>
            </template>
          </div>
        </div>
      </div>
    </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, nextTick, watch } from 'vue';
import Navbar from './components/Navbar.vue';
import FolderSelect from './components/FolderSelect.vue';
import { formatFriendlyDate } from './js/utils.js';

const loadSetting = (key, defaultValue) => {
  const val = localStorage.getItem(key);
  if (!val) return defaultValue;
  try {
    return JSON.parse(val);
  } catch (e) {
    return defaultValue;
  }
};

const emails = ref([]);
const pageSize = ref(loadSetting('emailPageSize', 50));
const folders = ref([]);
const foldersLoading = ref(true);
const isEditingFolders = ref(false);
const folderPreferences = ref({}); // { folderId: { order: number, hidden: boolean, customName: string, customIcon: string, color: string } }
const saveTimeout = ref(null);

const folderTree = computed(() => {
  if (folders.value.length === 0) return [];
  
  const prefs = folderPreferences.value;
  const result = [];
  const map = {};
  folders.value.forEach(f => {
    map[f.id] = { 
      ...f, 
      displayName: prefs[f.id]?.customName || f.displayName,
      children: [], 
      order: prefs[f.id]?.order ?? 999,
      hidden: prefs[f.id]?.hidden ?? false,
      customIcon: prefs[f.id]?.customIcon,
      color: prefs[f.id]?.color
    };
  });
  
  const roots = [];
  folders.value.forEach(f => {
    const node = map[f.id];
    // A folder is a root if its parent is not in our set of folders
    if (f.parentFolderId && map[f.parentFolderId]) {
      map[f.parentFolderId].children.push(node);
    } else {
      roots.push(node);
    }
  });
  
  // Sort function for folders
  const sortFolders = (a, b) => {
    // If both have explicit order, use it
    if (a.order !== 999 || b.order !== 999) {
      return a.order - b.order;
    }

    // Default sorting for root folders if no custom order
    const folderOrder = ['inbox', 'archive', 'sentitems', 'drafts', 'deleteditems', 'junkemail'];
    const idxA = folderOrder.indexOf(a.displayName.toLowerCase().replace(' ', ''));
    const idxB = folderOrder.indexOf(b.displayName.toLowerCase().replace(' ', ''));
    if (idxA !== -1 && idxB !== -1) return idxA - idxB;
    if (idxA !== -1) return -1;
    if (idxB !== -1) return 1;
    return a.displayName.localeCompare(b.displayName);
  };

  const processLevel = (nodes, level = 0) => {
    // Make a copy of nodes to sort so we don't affect the original children array order during recursive sorting if needed
    const sortedNodes = [...nodes].sort(sortFolders);
    sortedNodes.forEach(node => {
      result.push({ ...node, level });
      if (node.children && node.children.length > 0) {
        processLevel(node.children, level + 1);
      }
    });
  };
  
  processLevel(roots);
  return result;
});
const currentFolderId = ref('inbox');
const currentFolderName = ref('Inbox');
const rules = ref([]);
const loading = ref(true);
const headerRow = ref(null);
const sidebarCollapsed = ref(loadSetting('emailSidebarCollapsed', false));
const sidebarWidth = ref(loadSetting('emailSidebarWidth', 260));
const isResizingSidebar = ref(false);
let sidebarStartX = 0;
let sidebarStartWidth = 0;

const sidebarStyle = computed(() => {
  if (sidebarCollapsed.value) return {};
  return { 
    width: sidebarWidth.value + 'px',
    transition: isResizingSidebar.value ? 'none' : 'width 0.3s ease'
  };
});

watch(sidebarWidth, (newVal) => {
  localStorage.setItem('emailSidebarWidth', JSON.stringify(newVal));
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

const showRulesManager = ref(false);
const editingRule = ref(null);

// Progress state for applying rules
const applyingRuleName = ref(null);
const ruleProgress = ref(0);
const ruleProgressTotal = ref(0);
const ruleProgressSubject = ref('');

const actionTypeOptions = [
  { value: 0, label: 'Star' },
  { value: 1, label: 'Add Category' },
  { value: 2, label: 'Archive' },
  { value: 3, label: 'Move to Folder' },
  { value: 4, label: 'Mark as Read' },
  { value: 5, label: 'Clear Flag' }
];

const normalizeActionType = (type) => {
  if (typeof type === 'number') {
    if (type === 31) return 4;
    if (type === 32) return 5;
    return type;
  }
  const map = {
    'Star': 0,
    'AddCategory': 1,
    'Archive': 2,
    'Move': 3,
    'MarkAsRead': 4,
    'ClearFlag': 5
  };
  return map[type] ?? type;
};


watch(sidebarCollapsed, (newVal) => {
  localStorage.setItem('emailSidebarCollapsed', JSON.stringify(newVal));
});

watch(folderPreferences, (newVal) => {
  if (saveTimeout.value) clearTimeout(saveTimeout.value);
  saveTimeout.value = setTimeout(async () => {
    try {
      await fetch('/api/email/preferences', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newVal)
      });
    } catch (e) {
      console.error('Error saving email preferences:', e);
    }
  }, 500);
}, { deep: true });

// Sorting state
const sortBy = ref('receivedDateTime');
const sortDesc = ref(true);

// Resizing state - No longer used for main list but kept for potential future use or to avoid removing too much logic at once
const columnWidths = ref([200, 0, 150, 80]); // From, Subject (unused as 1fr), Date, Action
const isResizing = ref(false);
const activeResizer = ref(-1);
const startX = ref(0);
const startWidth = ref(0);

const gridStyle = computed(() => {
  const widths = [...columnWidths.value];
  const template = widths.map((w, i) => {
    if (i === 1 && w === 0) return '1fr';
    return `${w}px`;
  }).join(' ');
  return {
    display: 'grid',
    gridTemplateColumns: template
  };
});

const startResize = (index, event) => {
  isResizing.value = true;
  activeResizer.value = index;
  startX.value = event.pageX;
  
  // Find the header cells in the current header row
  const headerCells = event.currentTarget.parentElement.querySelectorAll('.email-header');
  if (headerCells && headerCells[index]) {
    startWidth.value = headerCells[index].offsetWidth;
  } else {
    startWidth.value = columnWidths.value[index];
  }
  
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
};

const fetchEmails = async () => {
  loading.value = true;
  try {
    const response = await fetch(`/api/email?folderId=${currentFolderId.value}&top=${pageSize.value}`);
    if (response.ok) {
      emails.value = await response.json();
    }
  } catch (error) {
    console.error('Error fetching emails:', error);
  } finally {
    loading.value = false;
  }
};

const fetchFolders = async () => {
  foldersLoading.value = true;
  try {
    const response = await fetch('/api/email/folders');
    if (response.ok) {
      folders.value = await response.json();
      // Update folder count if needed, or just let it be
    }
  } catch (error) {
    console.error('Error fetching folders:', error);
  } finally {
    foldersLoading.value = false;
  }
};

const fetchPreferences = async () => {
  try {
    const response = await fetch('/api/email/preferences');
    if (response.ok) {
      folderPreferences.value = await response.json();
    }
  } catch (error) {
    console.error('Error fetching preferences:', error);
  }
};

const selectFolder = (folder) => {
  if (isEditingFolders.value) return;
  currentFolderId.value = folder.id;
  currentFolderName.value = folder.displayName;
  fetchEmails();
};

const moveFolder = (folder, direction) => {
  // If editing in collapsed mode, expanding helps seeing what is happening
  if (sidebarCollapsed.value) {
    sidebarCollapsed.value = false;
  }
  const tree = folderTree.value;
  const currentIndex = tree.findIndex(f => f.id === folder.id);
  if (currentIndex === -1) return;

  const targetIndex = currentIndex + direction;
  if (targetIndex < 0 || targetIndex >= tree.length) return;

  const targetFolder = tree[targetIndex];
  
  // Only allow moving within the same parent/level for simplicity in this flat-view representation of tree
  // Though it's flattened, moving "up/down" in the list should probably just adjust local order among siblings
  // if they have the same parent.
  
  const siblings = tree.filter(f => f.parentFolderId === folder.parentFolderId);
  const folderInSiblingsIndex = siblings.findIndex(f => f.id === folder.id);
  const targetSiblingIndex = folderInSiblingsIndex + direction;

  if (targetSiblingIndex >= 0 && targetSiblingIndex < siblings.length) {
    const siblingToSwap = siblings[targetSiblingIndex];
    
    // Update preferences for all siblings to ensure they have an order
    const newPrefs = { ...folderPreferences.value };
    siblings.forEach((s, idx) => {
      if (!newPrefs[s.id]) newPrefs[s.id] = { order: idx, hidden: false };
    });

    // Swap orders
    const oldOrder = newPrefs[folder.id].order;
    newPrefs[folder.id].order = newPrefs[siblingToSwap.id].order;
    newPrefs[siblingToSwap.id].order = oldOrder;

    folderPreferences.value = newPrefs;
  }
};

const toggleFolderVisibility = (folderId) => {
  const newPrefs = { ...folderPreferences.value };
  if (!newPrefs[folderId]) {
    newPrefs[folderId] = { order: 999, hidden: true };
  } else {
    newPrefs[folderId].hidden = !newPrefs[folderId].hidden;
  }
  folderPreferences.value = newPrefs;
};

const renameFolder = (folderId, newName) => {
  const newPrefs = { ...folderPreferences.value };
  if (!newPrefs[folderId]) {
    newPrefs[folderId] = { order: 999, hidden: false, customName: newName };
  } else {
    newPrefs[folderId].customName = newName;
  }
  folderPreferences.value = newPrefs;
};

const setIcon = (folderId, icon) => {
  const newPrefs = { ...folderPreferences.value };
  if (!newPrefs[folderId]) {
    newPrefs[folderId] = { order: 999, hidden: false, customIcon: icon };
  } else {
    newPrefs[folderId].customIcon = icon;
  }
  folderPreferences.value = newPrefs;
};

const setColor = (folderId, color) => {
  const newPrefs = { ...folderPreferences.value };
  if (!newPrefs[folderId]) {
    newPrefs[folderId] = { order: 999, hidden: false, color: color };
  } else {
    newPrefs[folderId].color = color;
  }
  folderPreferences.value = newPrefs;
};

const availableIcons = [
  'bi-inbox', 'bi-send', 'bi-file-earmark-text', 'bi-trash', 'bi-shield-exclamation', 'bi-archive',
  'bi-folder', 'bi-star', 'bi-tag', 'bi-envelope', 'bi-mailbox', 'bi-flag', 'bi-collection',
  'bi-bookmark', 'bi-clock', 'bi-lightning', 'bi-gear', 'bi-person', 'bi-people', 'bi-chat'
];

const availableColors = [
  '', '#ff4d4d', '#ff944d', '#ffdb4d', '#94ff4d', '#4dff94', '#4dffff', '#4d94ff', '#944dff', '#ff4dff',
  '#ffffff', '#b3b3b3', '#666666',
  '#e74c3c', '#e67e22', '#f1c40f', '#2ecc71', '#1abc9c', '#3498db', '#9b59b6', '#34495e',
  '#c0392b', '#d35400', '#f39c12', '#27ae60', '#16a085', '#2980b9', '#8e44ad', '#2c3e50'
];

const getFolderIcon = (folder) => {
  if (folder.customIcon) return folder.customIcon;
  const name = folder.displayName.toLowerCase();
  if (name.includes('inbox')) return 'bi-inbox';
  if (name.includes('sent')) return 'bi-send';
  if (name.includes('drafts')) return 'bi-file-earmark-text';
  if (name.includes('deleted') || name.includes('trash')) return 'bi-trash';
  if (name.includes('junk') || name.includes('spam')) return 'bi-shield-exclamation';
  if (name.includes('archive')) return 'bi-archive';
  return 'bi-folder';
};

const fetchRules = async () => {
  try {
    const response = await fetch('/api/email/rules');
    if (response.ok) {
      rules.value = await response.json();
    }
  } catch (error) {
    console.error('Error fetching rules:', error);
  }
};

const applyRule = async (emailId, ruleName) => {
  try {
    const response = await fetch(`/api/email/${emailId}/apply-rule`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(ruleName)
    });
    if (response.ok) {
      fetchEmails();
    }
  } catch (error) {
    console.error('Error applying rule:', error);
  }
};

const applyToAll = async (ruleName) => {
  applyingRuleName.value = ruleName;
  ruleProgress.value = 0;
  ruleProgressTotal.value = 0;
  ruleProgressSubject.value = 'Initializing...';
  
  try {
    const response = await fetch(`/api/email/apply-rule-all?folderId=${currentFolderId.value}&top=${pageSize.value}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(ruleName)
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const reader = response.body.getReader();
    const decoder = new TextDecoder();
    let buffer = '';

    while (true) {
      const { value, done } = await reader.read();
      if (done) break;

      buffer += decoder.decode(value, { stream: true });
      const lines = buffer.split('\n');
      buffer = lines.pop(); // Keep partial line in buffer

      for (const line of lines) {
        if (line.startsWith('data: ')) {
          const data = line.substring(6).trim();
          if (data === '[DONE]') {
            // Processing complete
            break;
          }
          try {
            const update = JSON.parse(data);
            if (update.error) {
              alert(`Error applying rule: ${update.error}`);
              applyingRuleName.value = null;
              return;
            }
            ruleProgress.value = update.current;
            ruleProgressTotal.value = update.total;
            ruleProgressSubject.value = update.subject;
          } catch (e) {
            console.error('Error parsing SSE data:', e, data);
          }
        }
      }
    }

    // Wrap up
    applyingRuleName.value = null;
    fetchEmails();
    
  } catch (error) {
    console.error('Error applying rule to all:', error);
    alert('Failed to apply rule: ' + error.message);
    applyingRuleName.value = null;
  }
};


const createTask = async (email) => {
  try {
    const params = new URLSearchParams({
      subject: email.subject,
      sender: email.from,
      link: email.webLink
    });
    const response = await fetch(`/api/tasks/from-email?${params.toString()}`, {
      method: 'POST'
    });
    if (response.ok) {
      alert('Task created successfully!');
    }
  } catch (error) {
    console.error('Error creating task:', error);
  }
};

const createRuleFromEmail = async (emailId) => {
  try {
    const response = await fetch(`/api/email/rules/create-from-email?messageId=${emailId}`, {
      method: 'POST'
    });
    if (response.ok) {
      const rule = await response.json();
      // console.log('[DEBUG_LOG] rules/create-from-email response:', rule);
      fetchRules();
      fetchEmails();
      editRule(rule);
      showRulesManager.value = true;
    }
  } catch (error) {
    console.error('Error creating rule:', error);
  }
};

const addSenderToRule = async (ruleName, senderEmail, subject) => {
  try {
    const response = await fetch(`/api/email/rules/add-sender?ruleName=${encodeURIComponent(ruleName)}&senderEmail=${encodeURIComponent(senderEmail)}&subject=${encodeURIComponent(subject || '')}`, {
      method: 'POST'
    });
    if (response.ok) {
      const rule = await response.json();
      // console.log('[DEBUG_LOG] rules/add-sender response:', rule);
      fetchRules();
      fetchEmails();
      editRule(rule);
      showRulesManager.value = true;
    }
  } catch (error) {
    console.error('Error adding sender/subject to rule:', error);
  }
};

const startCreateRule = () => {
  editingRule.value = {
    name: '',
    color: '#3498db',
    rootFolder: 'Inbox',
    filters: [{ from: [], subjectContains: [] }],
    actions: [{ type: 31, value: null }] // Mark as read
  };
};

const editRule = (rule) => {
  // console.log('[DEBUG_LOG] editRule:', JSON.parse(JSON.stringify(rule)));
  editingRule.value = JSON.parse(JSON.stringify(rule));
  editingRule.value.originalName = rule.name;
};

const addFilter = () => {
  editingRule.value.filters.push({ from: [], subjectContains: [] });
};

const addAction = () => {
  editingRule.value.actions.push({ type: 4, value: null });
};

const saveRule = async () => {
  if (!editingRule.value.name) {
    alert('Rule name is required');
    return;
  }
  
  // Clean up empty filter values before saving
  if (editingRule.value.filters) {
    editingRule.value.filters.forEach(filter => {
      if (filter.from) {
        filter.from = filter.from.map(s => s.trim()).filter(s => s !== '');
      }
      if (filter.subjectContains) {
        filter.subjectContains = filter.subjectContains.map(s => s.trim()).filter(s => s !== '');
      }
    });
  }

  // console.log('[DEBUG_LOG] saving rule:', JSON.parse(JSON.stringify(editingRule.value)));

  try {
    const response = await fetch('/api/email/rules', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(editingRule.value)
    });
    if (response.ok) {
      editingRule.value = null;
      fetchRules();
      fetchEmails();
    }
  } catch (error) {
    console.error('Error saving rule:', error);
  }
};

const deleteRule = async (ruleName) => {
  if (!confirm(`Are you sure you want to delete the rule "${ruleName}"?`)) return;
  try {
    const response = await fetch(`/api/email/rules/${encodeURIComponent(ruleName)}`, {
      method: 'DELETE'
    });
    if (response.ok) {
      fetchRules();
      fetchEmails();
    }
  } catch (error) {
    console.error('Error deleting rule:', error);
  }
};

const deleteEmail = async (id) => {
  if (!confirm('Move this email to trash?')) return;
  try {
    const response = await fetch(`/api/email/${id}`, { method: 'DELETE' });
    if (response.ok) {
      // Remove email from local list for seamless update
      emails.value = emails.value.filter(e => e.id !== id);
      // Refresh folders to update unread counts
      fetchFolders();
    }
  } catch (error) {
    console.error('Error deleting email:', error);
  }
};

const formatDate = (dateString) => {
  if (!dateString) return 'N/A';
  const date = new Date(dateString);
  return date.toLocaleString();
};

const toggleSort = (field) => {
  if (sortBy.value === field) {
    sortDesc.value = !sortDesc.value;
  } else {
    sortBy.value = field;
    sortDesc.value = false;
  }
};

const getSortedEmails = (emails) => {
  if (!sortBy.value) return emails;
  return [...emails].sort((a, b) => {
    let valA = a[sortBy.value];
    let valB = b[sortBy.value];
    
    if (sortBy.value === 'receivedDateTime') {
      valA = new Date(valA).getTime();
      valB = new Date(valB).getTime();
    } else {
      valA = (valA || '').toString().toLowerCase();
      valB = (valB || '').toString().toLowerCase();
    }
    
    if (valA < valB) return sortDesc.value ? 1 : -1;
    if (valA > valB) return sortDesc.value ? -1 : 1;
    return 0;
  });
};

watch(pageSize, (newVal) => {
  localStorage.setItem('emailPageSize', JSON.stringify(newVal));
  fetchEmails();
});

onMounted(() => {
  fetchPreferences();
  fetchFolders();
  fetchEmails();
  fetchRules();
});
</script>

<style>
.form-control::placeholder {
  color: var(--text-muted) !important;
  opacity: 0.6 !important;
}

label, .form-label {
  color: var(--text-primary) !important;
  opacity: 0.9 !important;
}

.email-app-container {
  display: flex;
  height: 100%;
  background-color: var(--bg-darker);
  color: var(--text-primary);
  overflow: hidden;
}

.app-root {
  background-color: var(--bg-darker);
}

.main-wrapper {
  background-color: var(--bg-darker);
}

.email-sidebar {
  width: 260px;
  border-right: 1px solid var(--border-primary);
  display: flex;
  flex-direction: column;
  background-color: var(--bg-dark);
  overflow: hidden;
  flex-shrink: 0;
  z-index: 1;
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

.editing-folders .email-sidebar {
  border-right-color: var(--accent-blue);
  box-shadow: 2px 0 10px rgba(0, 123, 255, 0.2);
}

.email-sidebar.collapsed {
  width: 50px !important;
}

.sidebar-header {
  padding: 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid var(--border-primary);
  height: 60px;
}

.email-sidebar.collapsed .sidebar-header {
  padding: 1rem 0;
  justify-content: center;
}

.sidebar-header h5 {
  margin: 0;
  font-weight: 600;
  white-space: nowrap;
}

.sidebar-footer {
  padding: 0.5rem;
  display: flex;
  justify-content: flex-end;
  background-color: var(--bg-dark);
}

.sidebar-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent !important;
  border: none !important;
  color: var(--text-muted);
  font-size: 1.2rem;
  padding: 8px;
  box-shadow: none !important;
}

.sidebar-toggle:hover {
  color: var(--text-primary);
}

.sidebar-toggle:focus {
  outline: none;
  box-shadow: none;
}

.email-sidebar.collapsed .sidebar-toggle {
  margin-left: 0;
}

.folder-list {
  flex-grow: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 0.5rem 0;
}

.folder-item {
  position: relative;
  padding: 0.75rem 1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: background-color 0.2s;
  border-left: 3px solid transparent;
  min-width: 0;
}

.email-sidebar.collapsed .folder-item {
  padding: 0.75rem 0;
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

.email-sidebar.collapsed .folder-edit-controls {
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
  margin-right: .25rem;
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

.icon-selector .dropdown-toggle::after,
.color-selector .dropdown-toggle::after {
  display: none;
}

/* Dropdown Submenu */
.dropdown-submenu {
  position: relative;
}

.dropdown-submenu > .dropdown-menu {
  top: 0;
  left: 100%;
  margin-top: -6px;
  margin-left: -1px;
}

.dropdown-submenu.submenu-left > .dropdown-menu {
  left: auto;
  right: 100%;
  margin-left: 0;
  margin-right: -1px;
}

.dropdown-submenu:hover > .dropdown-menu {
  display: block;
}

.color-grid {
  min-width: 120px;
}

.color-option {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 2px solid transparent;
  transition: transform 0.2s, border-color 0.2s;
}

.color-option:hover {
  transform: scale(1.2);
  border-color: var(--accent-blue);
}

.color-option.active {
  border-color: white;
  box-shadow: 0 0 5px rgba(255, 255, 255, 0.5);
}

.color-option.border {
  border: 1px dashed #666;
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

.email-sidebar.collapsed .folder-icon-wrapper {
  margin-right: 0;
}

.folder-name {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  font-size: 0.95rem;
  min-width: 0;
}

.folder-count {
  font-size: 0.8rem;
  color: var(--text-muted);
  background-color: var(--bg-darker);
  padding: 2px 6px;
  border-radius: 10px;
  opacity: 0.85;
}

.main-content {
  flex-grow: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.email-container {
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.controls-bar {
  padding: 10px 15px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid var(--border-primary);
  min-height: 60px;
  flex-shrink: 0;
}

.page-title-area h2 {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
}

.email-content-scrollable {
  flex-grow: 1;
  overflow-y: auto;
}

.btn-icon {
  background: var(--bg-card);
  color: var(--text-primary);
  border: 1px solid var(--border-primary);
  padding: 0.4rem 0.8rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.email-list {
  display: flex;
  flex-direction: column;
  width: 100%;
}

.email-item {
  display: flex;
  padding: 6px 16px;
  border-bottom: 1px solid var(--border-primary);
  cursor: pointer;
  transition: background-color 0.1s;
  position: relative;
  min-height: 54px;
}

.email-item:hover {
  background-color: var(--bg-card);
}

.email-item.unread::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
  background-color: var(--accent-blue);
}

.rule-border-bar {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
  z-index: 1;
}

.email-item-content {
  flex-grow: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.email-item-top {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
}

.email-sender {
  font-weight: 600;
  font-size: 0.95rem;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.email-item.unread .email-sender {
  color: var(--accent-blue);
}

.email-date {
  font-size: 0.75rem;
  color: var(--text-muted);
  flex-shrink: 0;
  margin-left: 10px;
  opacity: 0.85;
}

.email-item-details {
  font-size: 0.85rem;
  color: var(--text-muted);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-top: -2px;
  opacity: 0.85;
}

.subject-text {
  font-weight: 500;
  color: var(--text-primary);
}

.email-item.unread .subject-text {
  font-weight: 700;
}

.preview-separator {
  opacity: 0.7;
  margin: 0 4px;
}

.email-preview-text {
  opacity: 0.8;
}

.email-item-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.email-item-actions {
  display: flex;
  align-items: flex-start;
  margin-left: 8px;
  transition: opacity 0.2s;
}


.action-btn {
  font-size: 1.1rem;
}

.action-btn:hover {
  color: var(--text-primary) !important;
}

.rule-tag {
  font-size: 0.7rem;
  padding: 1px 6px;
  background-color: var(--accent-blue);
  color: white;
  border-radius: 10px;
}

.rule-color-indicator {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  flex-shrink: 0;
}

.bg-darker {
  background-color: var(--bg-darker) !important;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.7);
  z-index: 1060;
  backdrop-filter: blur(4px);
}

.x-small {
  font-size: 0.75rem;
}
</style>
