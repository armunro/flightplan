<template>
  <div :class="['vh-100 d-flex flex-row overflow-hidden', themeClass]">
    <Navbar />
    <div class="flex-grow-1 overflow-hidden d-flex flex-column theme-bg-darker" style="position: relative; z-index: 1;">
      <div class="flex-grow-1 overflow-hidden d-flex flex-row">

      <!-- Sidebar for files -->
      <div class="file-sidebar d-flex flex-column" :class="{ collapsed: sidebarCollapsed }" :style="sidebarStyle">
        <div class="sidebar-header d-flex align-items-center theme-border" :class="{ 'collapsed': sidebarCollapsed }">
          <h5 v-if="!sidebarCollapsed" class="mb-0 theme-text">Files</h5>
          <div v-if="!sidebarCollapsed" class="d-flex align-items-center gap-1 ms-auto">
            <button class="btn-icon theme-text" @click="createNewFile" title="New File">
              <i class="bi bi-plus-lg"></i>
            </button>
          </div>
          <i v-else class="bi bi-sticky theme-text"></i>
        </div>
        <div class="file-list flex-grow-1 overflow-auto">
          <div 
            v-for="file in files" 
            :key="file" 
            class="file-item theme-border"
            :class="{ 'active': currentFile === file }"
            @click="selectFile(file)"
            :title="sidebarCollapsed ? file : ''"
          >
            <div class="file-icon-wrapper">
              <i class="bi bi-file-earmark-text"></i>
            </div>
            <div v-if="!sidebarCollapsed" class="d-flex align-items-center flex-grow-1 min-w-0">
              <span class="text-truncate flex-grow-1 theme-text" :title="file">{{ file }}</span>
              <button class="btn btn-sm btn-link text-danger p-0 ms-2 opacity-0 delete-btn" @click.stop="deleteFile(file)">
                <i class="bi bi-trash"></i>
              </button>
            </div>
          </div>
        </div>
        <div class="sidebar-footer theme-border" :class="{ 'collapsed': sidebarCollapsed }">
          <button class="sidebar-toggle theme-text" @click="sidebarCollapsed = !sidebarCollapsed" :title="sidebarCollapsed ? 'Expand Menu' : 'Collapse Menu'">
            <i class="bi" :class="sidebarCollapsed ? 'bi-chevron-right' : 'bi-chevron-left'"></i>
          </button>
        </div>
      </div>

      <!-- Sidebar Resizer -->
      <div v-if="!sidebarCollapsed" class="sidebar-resizer" @mousedown="startSidebarResize"></div>

      <!-- Main Editor Area -->
      <div class="main-area d-flex flex-column flex-grow-1 theme-bg-darker">
        <div class="controls-bar theme-border px-3">
          <h5 class="mb-0 me-3 theme-text d-flex align-items-center"><i class="bi bi-journal-text me-2"></i> {{ currentFile || 'No file selected' }}</h5>
          <div v-if="currentFile" class="save-status small" :class="{ 'text-success': saveStatus === 'Saved', 'text-warning': saveStatus === 'Saving...', 'text-danger': saveStatus === 'Error' }">
            {{ saveStatus }}
          </div>
          <div class="ms-auto d-flex align-items-center gap-2">
            <button class="btn btn-sm btn-subtle d-flex align-items-center gap-2" 
                    @click="handleCreateTask" 
                    title="Create task from selection">
              <i class="bi bi-check2-square"></i> 
              <span>Create Task(s)</span>
            </button>
          </div>
        </div>

        <div v-if="currentFile" class="editor-area flex-grow-1 overflow-hidden" style="position: relative; z-index: 1;">
          <MdEditor 
            v-model="content" 
            :theme="mdTheme" 
            @onChange="onInput" 
            :completions="completions"
            language="en-US"
            style="height: 100%"
            :toolbars="toolbars"
            ref="editorRef"
          >
            <template #defToolbars>
              <NormalToolbar title="Create Task(s)" @onClick="handleCreateTask">
                <template #trigger>
                  <i class="bi bi-check2-square"></i>
                </template>
              </NormalToolbar>
            </template>
          </MdEditor>
        </div>
        <div v-else class="flex-grow-1 d-flex align-items-center justify-content-center theme-text-muted">
          Select or create a file to start writing
        </div>
      </div>
    </div>
  </div>
</div>
</template>

<script setup>
import { ref, computed, onMounted, watch, onUnmounted } from 'vue';
import { showToast } from './components/Toast.vue';
import Navbar from './components/Navbar.vue';
import { MdEditor, NormalToolbar } from 'md-editor-v3';
import 'md-editor-v3/lib/style.css';
import { fetchSettings } from './js/dashboard-api';

const files = ref([]);
const currentFile = ref(null);
const content = ref('');
const editorRef = ref(null);
const saveStatus = ref('');
const saveTimeout = ref(null);

const theme = ref('Cosmic');
const themeClass = computed(() => `theme-${theme.value.toLowerCase()}`);
const mdTheme = computed(() => theme.value.toLowerCase() === 'light' ? 'light' : 'dark');

const toolbars = [
  'bold',
  'underline',
  'italic',
  '-',
  'strikeThrough',
  'title',
  'sub',
  'sup',
  'quote',
  'unorderedList',
  'orderedList',
  'task',
  '-',
  'codeRow',
  'code',
  'link',
  'image',
  'table',
  'mermaid',
  'katex',
  0,
  '-',
  'revoke',
  'next',
  'save',
  '=',
  'pageFullscreen',
  'fullscreen',
  'preview',
  'htmlPreview',
  'catalog',
  'github'
];

const handleCreateTask = () => {
  console.log('[DEBUG_LOG] handleCreateTask called');
  let selection = '';
  
  // Standard window selection
  selection = window.getSelection().toString();
  console.log('[DEBUG_LOG] Window selection:', selection);
  
  if (!selection && editorRef.value) {
    editorRef.value.getSelectedText((text) => {
      selection = text;
      console.log('[DEBUG_LOG] Editor selection:', selection);
    });
  }
  
  if (!selection) {
    console.log('[DEBUG_LOG] No selection found');
    showToast('Please select some text first', 'info');
    return;
  }

  const lines = selection.split('\n').map(l => l.trim()).filter(l => l.length > 0);
  if (lines.length === 0) return;

  if (lines.length === 1) {
    console.log('[DEBUG_LOG] Opening task modal for single task:', lines[0]);
    window.dispatchEvent(new CustomEvent('open-add-task-modal', {
      detail: { title: lines[0] }
    }));
  } else {
    console.log('[DEBUG_LOG] Opening task modal for multiple tasks:', lines.length);
    window.dispatchEvent(new CustomEvent('open-add-task-modal', {
      detail: { titles: lines }
    }));
  }
};

const loadSetting = (key, defaultValue) => {
  const val = localStorage.getItem(key);
  if (!val) return defaultValue;
  try {
    return JSON.parse(val);
  } catch (e) {
    return defaultValue;
  }
};

const sidebarCollapsed = ref(loadSetting('notepadSidebarCollapsed', false));
const sidebarWidth = ref(loadSetting('notepadSidebarWidth', 260));
const isResizingSidebar = ref(false);
let sidebarStartX = 0;
let sidebarStartWidth = 0;

// Force currentFile if there is only one file or to the first one for testing if requested
// But better let the user select. For now, let's make sure it's reactive.
watch(currentFile, (newFile) => {
  console.log('[DEBUG_LOG] currentFile changed to:', newFile);
});

const sidebarStyle = computed(() => {
  if (sidebarCollapsed.value) return {};
  return { 
    width: sidebarWidth.value + 'px',
    transition: isResizingSidebar.value ? 'none' : 'width 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
  };
});

watch(sidebarWidth, (newVal) => {
  localStorage.setItem('notepadSidebarWidth', JSON.stringify(newVal));
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

watch(sidebarCollapsed, (newVal) => {
  localStorage.setItem('notepadSidebarCollapsed', JSON.stringify(newVal));
});

const completions = [
  (context) => {
    const word = context.matchBefore(/\/\w*/);
    if (!word || (word.from === word.to && !context.explicit)) {
      return null;
    }
    return {
      from: word.from,
      options: [
        { label: '/h1', type: 'text', apply: '# ', detail: 'Heading 1' },
        { label: '/h2', type: 'text', apply: '## ', detail: 'Heading 2' },
        { label: '/h3', type: 'text', apply: '### ', detail: 'Heading 3' },
        { label: '/list', type: 'text', apply: '- ', detail: 'Bullet List' },
        { label: '/todo', type: 'text', apply: '- [ ] ', detail: 'Todo List' },
        { label: '/code', type: 'text', apply: '```\n\n```', detail: 'Code Block' },
        { label: '/table', type: 'text', apply: '| Column 1 | Column 2 |\n| --- | --- |\n| Cell 1 | Cell 2 |', detail: 'Table' },
        { label: '/bold', type: 'text', apply: '****', detail: 'Bold' },
        { label: '/italic', type: 'text', apply: '__', detail: 'Italic' },
        { label: '/quote', type: 'text', apply: '> ', detail: 'Blockquote' },
        { label: '/link', type: 'text', apply: '[]()', detail: 'Link' },
        { label: '/image', type: 'text', apply: '![]()', detail: 'Image' },
      ],
    };
  },
];

const loadFileList = async () => {
  try {
    const response = await fetch('/api/notepad/files');
    if (response.ok) {
      files.value = await response.json();
      console.log('[DEBUG_LOG] Files loaded:', files.value);
      if (files.value.length > 0 && !currentFile.value) {
        selectFile(files.value[0]);
      }
    }
  } catch (e) {
    console.error('Failed to load file list:', e);
  }
};

const selectFile = async (filename) => {
  if (saveTimeout.value) {
    clearTimeout(saveTimeout.value);
    await saveNote();
  }
  
  currentFile.value = filename;
  saveStatus.value = 'Loading...';
  try {
    const response = await fetch(`/api/notepad/${filename}`);
    if (response.ok) {
      const data = await response.json();
      content.value = data.content;
      saveStatus.value = 'Loaded';
    }
  } catch (e) {
    console.error('Failed to load note:', e);
    saveStatus.value = 'Error loading';
  }
};

const createNewFile = async () => {
  const name = prompt('Enter filename (e.g. notes.md):');
  if (name) {
    if (!name.endsWith('.md')) {
      showToast('Filename must end with .md', 'warning');
      return;
    }

    try {
      // Create the file on the server immediately to avoid 404 when selecting it
      const response = await fetch(`/api/notepad/${name}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ content: '' })
      });

      if (response.ok) {
        if (!files.value.includes(name)) {
          files.value.push(name);
          // Sort files if needed, or just let it be at the end
          files.value.sort();
        }
        selectFile(name);
      } else {
        console.error('Failed to create file on server');
        showToast('Failed to create file', 'error');
      }
    } catch (e) {
      console.error('Error creating file:', e);
      showToast('Error creating file', 'error');
    }
  }
};

const deleteFile = async (filename) => {
  if (!confirm(`Delete ${filename}?`)) return;
  try {
    const response = await fetch(`/api/notepad/${filename}`, { method: 'DELETE' });
    if (response.ok) {
      files.value = files.value.filter(f => f !== filename);
      if (currentFile.value === filename) {
        currentFile.value = null;
        content.value = '';
      }
    }
  } catch (e) {
    console.error('Failed to delete file:', e);
  }
};

const onInput = () => {
  saveStatus.value = 'Modified';
  if (saveTimeout.value) clearTimeout(saveTimeout.value);
  saveTimeout.value = setTimeout(saveNote, 1000);
};

const saveNote = async () => {
  if (!currentFile.value) return;
  saveStatus.value = 'Saving...';
  try {
    const response = await fetch(`/api/notepad/${currentFile.value}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ content: content.value })
    });
    if (response.ok) {
      saveStatus.value = 'Saved';
    } else {
      saveStatus.value = 'Error';
    }
  } catch (e) {
    console.error('Failed to save note:', e);
    saveStatus.value = 'Error';
  } finally {
    saveTimeout.value = null;
  }
};

onMounted(async () => {
  console.log('[DEBUG_LOG] NotepadApp mounted');
  await loadFileList();
  try {
    const settings = await fetchSettings();
    if (settings) {
      theme.value = settings.theme || 'Cosmic';
    }
  } catch (e) {
    console.error('Failed to load settings:', e);
  }
});

onUnmounted(() => {
  if (saveTimeout.value) clearTimeout(saveTimeout.value);
});
</script>

<style>
.form-control::placeholder {
  color: #aab2bb !important;
  opacity: 0.6 !important;
}
</style>

<style scoped>
.file-sidebar {
  width: 230px;
  background-color: var(--bg-dark);
  border-right: 1px solid var(--border-primary);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  flex-shrink: 0;
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.file-sidebar.collapsed {
  width: 50px !important;
}

.sidebar-header h5 {
  white-space: nowrap;
}

.file-item {
  padding: 8px 12px;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: background-color 0.2s, padding 0.3s ease;
  border-left: 3px solid transparent;
  min-width: 0;
  overflow: hidden;
  font-size: var(--fs-base);
}

.file-sidebar.collapsed .file-item {
  padding: 10px 0;
  justify-content: center;
}

.file-item:hover {
  background-color: var(--bg-card);
}

.file-item.active {
  background-color: rgba(88, 166, 255, 0.1);
  border-left-color: var(--accent-blue);
}

.file-icon-wrapper {
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 10px;
  font-size: 1.1rem;
  flex-shrink: 0;
  color: var(--text-muted);
}

.file-sidebar.collapsed .file-icon-wrapper {
  margin-right: 0;
}

.file-item:hover .delete-btn {
  opacity: 1 !important;
}

.main-area {
  background-color: var(--bg-darker);
  position: relative;
  overflow: hidden;
  display: flex !important;
  flex-direction: column !important;
}

.controls-bar {
  height: 50px;
  padding: 0;
  display: flex;
  align-items: center;
  border-bottom: 1px solid var(--border-primary);
  flex-shrink: 0;
  background-color: var(--bg-dark);
  position: relative;
  z-index: 10;
  width: 100%;
}

.controls-bar .btn-primary:hover {
  background-color: var(--bg-hover) !important;
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

.editor-area {
  background-color: var(--bg-darker);
}

:deep(.md-editor) {
  border: none !important;
  --md-bk-color: var(--bg-darker);
}

:deep(.md-editor-toolbar-wrapper) {
  background-color: var(--bg-dark) !important;
  border-bottom: 1px solid var(--border-primary) !important;
}

:deep(.md-editor-content) {
  background-color: var(--bg-darker) !important;
}

:deep(.md-editor-preview-wrapper) {
  background-color: var(--bg-darker) !important;
}

:deep(.md-editor-footer) {
  background-color: var(--bg-dark) !important;
  border-top: 1px solid var(--border-primary) !important;
}
</style>
