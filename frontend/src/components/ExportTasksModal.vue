<template>
  <div class="modal-overlay" @click.self="close">
    <div class="export-modal theme-card border-primary" :class="{ 'theme-cosmic': theme === 'Cosmic' }">
      <div class="modal-header theme-border">
        <h5 class="theme-text mb-0">Export Tasks</h5>
        <button class="close-btn theme-text-muted" @click="close">
          <i class="bi bi-x-lg"></i>
        </button>
      </div>
      
      <div class="modal-body p-4" style="max-height: 85vh; overflow-y: auto;">
        <div class="row g-4">
          <!-- Column Selection -->
          <div class="col-md-6">
            <h6 class="theme-text-muted small text-uppercase fw-bold mb-3">Select & Reorder Columns</h6>
            <div class="column-selection-list border theme-border rounded p-2 theme-bg-dark">
              <div v-for="(col, index) in orderedColumns" 
                   :key="col.id" 
                   class="column-item d-flex align-items-center p-2 mb-1 rounded"
                   :class="{ 'dragging': dragIndex === index }"
                   draggable="true"
                   @dragstart="onDragStart(index)"
                   @dragover.prevent="onDragOver(index)"
                   @dragleave="onDragLeave"
                   @drop="onDrop(index)">
                <div class="drag-handle me-2 theme-text-muted">
                  <i class="bi bi-grip-vertical"></i>
                </div>
                <div class="form-check mb-0 flex-grow-1">
                  <input class="form-check-input" type="checkbox" :id="'col-' + col.id" v-model="selectedColumns" :value="col.id">
                  <label class="form-check-label theme-text cursor-pointer" :for="'col-' + col.id">
                    {{ col.name }}
                  </label>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Format Selection -->
          <div class="col-md-6">
            <h6 class="theme-text-muted small text-uppercase fw-bold mb-3">Export Format</h6>
            <div class="format-selection border theme-border rounded p-3 theme-bg-dark">
              <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="format" id="fmt-csv" value="csv" v-model="format">
                <label class="form-check-label theme-text" for="fmt-csv">CSV (Comma Separated Values)</label>
              </div>
              <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="format" id="fmt-excel" value="excel" v-model="format">
                <label class="form-check-label theme-text" for="fmt-excel">Excel (XML)</label>
              </div>
              <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="format" id="fmt-html" value="html" v-model="format">
                <label class="form-check-label theme-text" for="fmt-html">HTML Table</label>
              </div>
              <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="format" id="fmt-html-styled" value="html-styled" v-model="format">
                <label class="form-check-label theme-text" for="fmt-html-styled">HTML Table (Styled)</label>
              </div>
              <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="format" id="fmt-text" value="text" v-model="format">
                <label class="form-check-label theme-text" for="fmt-text">Plain Text</label>
              </div>
              <div class="form-check mb-2">
                <input class="form-check-input" type="radio" name="format" id="fmt-table" value="table" v-model="format">
                <label class="form-check-label theme-text" for="fmt-table">Pretty Table (ASCII)</label>
              </div>
            </div>

            <div class="export-options mt-4">
               <h6 class="theme-text-muted small text-uppercase fw-bold mb-3">Action</h6>
               <div class="d-flex gap-2">
                  <button class="btn btn-primary flex-grow-1" @click="doExport('file')">
                    <i class="bi bi-download me-2"></i>Download File
                  </button>
                  <button v-if="format !== 'excel'" class="btn btn-outline-primary" @click="doExport('copy')" title="Copy to clipboard">
                    <i class="bi bi-clipboard me-2"></i>Copy
                  </button>
               </div>
            </div>
          </div>
        </div>

        <!-- Preview Area -->
        <div class="mt-4 preview-container">
          <div class="d-flex justify-content-between align-items-center mb-2">
            <h6 class="theme-text-muted small text-uppercase fw-bold mb-0">Preview</h6>
          </div>
          <div class="preview-box theme-border border rounded p-3 overflow-auto theme-bg-dark" style="max-height: 400px;">
            <div v-if="format === 'excel'" class="theme-text text-center p-4">
              <i class="bi bi-file-earmark-excel fs-1 theme-text-muted mb-2 d-block"></i>
              Excel preview not available. Download to view.
            </div>
            <pre v-else-if="format === 'text' || format === 'csv' || format === 'table'" class="theme-text mb-0">{{ previewContent }}</pre>
            <div v-else-if="format === 'html' || format === 'html-styled'" v-html="previewContent"></div>
          </div>
        </div>
      </div>
      
      <div class="modal-footer theme-border p-3">
        <button class="btn btn-subtle" @click="close">Cancel</button>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, watch, onMounted } from 'vue';
import { formatFriendlyDate, formatEstimate, getDateColorClass } from '../js/utils';

export default {
  name: 'ExportTasksModal',
  props: {
    tasks: { type: Array, required: true },
    project: { type: Object, required: true },
    theme: { type: String, default: 'Cosmic' }
  },
  emits: ['close'],
  setup(props, { emit }) {
    const format = ref('csv');
    const selectedColumns = ref(['title', 'status', 'priority', 'list']);
    const previewContent = ref('');
    const dragIndex = ref(null);
    
    const allColumns = [
      { id: 'id', name: 'ID' },
      { id: 'title', name: 'Title' },
      { id: 'status', name: 'Status' },
      { id: 'priority', name: 'Priority' },
      { id: 'type', name: 'Task Type' },
      { id: 'list', name: 'List' },
      { id: 'start', name: 'Start Date' },
      { id: 'end', name: 'End Date' },
      { id: 'estimate', name: 'Estimate' },
      { id: 'created', name: 'Created At' }
    ];

    const orderedColumns = ref([...allColumns]);

    onMounted(() => {
      const savedColumns = localStorage.getItem('export-selected-columns');
      if (savedColumns) {
        try {
          selectedColumns.value = JSON.parse(savedColumns);
        } catch (e) {
          console.error('Failed to parse saved columns', e);
        }
      }

      const savedOrder = localStorage.getItem('export-column-order');
      if (savedOrder) {
        try {
          const orderIds = JSON.parse(savedOrder);
          const newOrdered = [];
          orderIds.forEach(id => {
            const col = allColumns.find(c => c.id === id);
            if (col) newOrdered.push(col);
          });
          // Add any missing columns that might have been added to allColumns later
          allColumns.forEach(col => {
            if (!newOrdered.find(c => c.id === col.id)) {
              newOrdered.push(col);
            }
          });
          orderedColumns.value = newOrdered;
        } catch (e) {
          console.error('Failed to parse saved column order', e);
        }
      }

      window.addEventListener('keydown', handleKeyDown);
      updatePreview();
    });

    const handleKeyDown = (e) => {
      if (e.key === 'Escape') {
        // If there's an active dropdown, don't close the modal
        if (document.querySelector('.dropdown-menu.show')) {
          return;
        }
        close();
      }
    };

    watch([selectedColumns, format, orderedColumns], () => {
      localStorage.setItem('export-selected-columns', JSON.stringify(selectedColumns.value));
      localStorage.setItem('export-column-order', JSON.stringify(orderedColumns.value.map(c => c.id)));
      updatePreview();
    }, { deep: true });

    const close = () => {
      window.removeEventListener('keydown', handleKeyDown);
      emit('close');
    };

    const onDragStart = (index) => {
      dragIndex.value = index;
    };

    const onDragOver = (index) => {
      if (dragIndex.value === null || dragIndex.value === index) return;
      
      const items = [...orderedColumns.value];
      const draggedItem = items[dragIndex.value];
      items.splice(dragIndex.value, 1);
      items.splice(index, 0, draggedItem);
      orderedColumns.value = items;
      dragIndex.value = index;
    };

    const onDragLeave = () => {
      // Could add visual feedback
    };

    const onDrop = (index) => {
      dragIndex.value = null;
    };

    const getTaskValue = (task, colId) => {
      switch (colId) {
        case 'id': return task.id;
        case 'title': return task.title;
        case 'status': {
          const status = props.project.statuses.find(s => s.id === task.statusId);
          return status ? status.name : '';
        }
        case 'priority': {
          const priority = props.project.priorities.find(p => p.id === task.priorityId);
          return priority ? priority.name : '';
        }
        case 'type': {
          const type = props.project.taskTypes.find(t => t.id === task.taskTypeId);
          return type ? type.name : '';
        }
        case 'list': {
           // Find the list that contains this task or contains a task that has this task as subtask
           const list = props.project.lists.find(l => 
             l.tasks.some(t => t.id === task.id || (t.subtasks && t.subtasks.some(st => st.id === task.id)))
           );
           return list ? list.name : '';
        }
        case 'start': return task.start ? formatFriendlyDate(task.start, true, false) : '';
        case 'end': return task.end ? formatFriendlyDate(task.end, true, false) : '';
        case 'estimate': return task.estimateMinutes ? formatEstimate(task.estimateMinutes) : '';
        case 'created': return task.createdAt ? new Date(task.createdAt).toLocaleString() : '';
        default: return '';
      }
    };

    const generateCSV = () => {
      const cols = orderedColumns.value.filter(c => selectedColumns.value.includes(c.id));
      const headers = cols.map(c => c.name);
      const rows = props.tasks.map(task => 
        cols.map(c => {
          let val = getTaskValue(task, c.id);
          if (val === null || val === undefined) val = '';
          // Ensure we're dealing with a string before replacing
          val = String(val).replace(/"/g, '""');
          return `"${val}"`;
        }).join(',')
      );
      return [headers.join(','), ...rows].join('\n');
    };

    const generateText = () => {
      const cols = orderedColumns.value.filter(c => selectedColumns.value.includes(c.id));
      return props.tasks.map(task => {
        return cols.map(c => `${c.name}: ${getTaskValue(task, c.id)}`).join(' | ');
      }).join('\n');
    };

    const generateHTML = () => {
      const cols = orderedColumns.value.filter(c => selectedColumns.value.includes(c.id));
      const isCosmic = props.theme === 'Cosmic';
      const bgColor = isCosmic ? '#1a1a2e' : '#1e1e1e';
      const headerBg = isCosmic ? '#16213e' : '#2d2d2d';
      const borderColor = isCosmic ? '#0f3460' : '#444';
      const textColor = '#ddd';
      const headerTextColor = '#fff';

      let html = `<table border="1" style="border-collapse: collapse; width: 100%; font-size: 12px; color: ${textColor}; background-color: ${bgColor}; border: 1px solid ${borderColor};">`;
      html += '<thead><tr>';
      cols.forEach(c => {
        html += `<th style="padding: 8px; text-align: left; background-color: ${headerBg}; color: ${headerTextColor}; border: 1px solid ${borderColor};">${c.name}</th>`;
      });
      html += '</tr></thead><tbody>';
      props.tasks.forEach(task => {
        html += '<tr>';
        cols.forEach(c => {
          html += `<td style="padding: 8px; border: 1px solid ${borderColor};">${getTaskValue(task, c.id)}</td>`;
        });
        html += '</tr>';
      });
      html += '</tbody></table>';
      return html;
    };

    const generateStyledHTML = () => {
      const cols = orderedColumns.value.filter(c => selectedColumns.value.includes(c.id));
      const isCosmic = props.theme === 'Cosmic';
      const bgColor = isCosmic ? '#1a1a2e' : '#1e1e1e';
      const headerBg = isCosmic ? '#16213e' : '#2d2d2d';
      const borderColor = isCosmic ? '#0f3460' : '#444';
      const textColor = '#ddd';
      const headerTextColor = '#fff';

      const getPriorityIcon = (priorityId) => {
        const priority = props.project.priorities.find(p => p.id === priorityId);
        if (!priority) return '';
        const name = priority.name.toLowerCase();
        if (name.includes('critical')) return 'bi-exclamation-octagon-fill';
        if (name.includes('highest') || name.includes('high')) return 'bi-chevron-double-up';
        if (name.includes('medium')) return 'bi-dash-lg';
        if (name.includes('lowest') || name.includes('low')) return 'bi-chevron-double-down';
        return 'bi-circle';
      };

      const getPriorityColor = (priorityId) => {
        const priority = props.project.priorities.find(p => p.id === priorityId);
        return priority ? priority.color : '#cccccc';
      };

      const getStatusColor = (statusId) => {
        const status = props.project.statuses.find(s => s.id === statusId);
        return status ? status.color : '#cccccc';
      };

      const getDateColor = (dateStr, task) => {
        if (!dateStr) return 'inherit';
        const colorClass = getDateColorClass(dateStr, task.isCompleted);
        switch (colorClass) {
          case 'date-overdue': return '#ff5555';
          case 'date-today': return '#ffcc00';
          case 'date-this-week': return '#55ff55';
          case 'date-muted': return '#777';
          default: return 'inherit';
        }
      };

      const borderColorValue = borderColor;
      const cssStyles = `
        .styled-table { border-collapse: collapse; width: 100%; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif; font-size: 13px; color: ${textColor}; background-color: ${bgColor}; border: 1px solid ${borderColorValue}; }
        .styled-table th { padding: 10px 12px; text-align: left; background-color: ${headerBg}; color: ${headerTextColor}; border: 1px solid ${borderColorValue}; font-weight: 600; text-transform: uppercase; font-size: 11px; letter-spacing: 0.5px; }
        .styled-table td { padding: 8px 12px; border: 1px solid ${borderColorValue}; vertical-align: middle; }
        .badge { display: inline-flex; align-items: center; padding: 2px 8px; border-radius: 4px; font-weight: 600; font-size: 11px; white-space: nowrap; color: #fff; }
        .bi { display: inline-block; width: 1em; height: 1em; vertical-align: -0.125em; margin-right: 4px; }
      `;
      let html = `<style>${cssStyles}</style>`;

      // Add Bootstrap Icons CSS for the exported file
      html += `<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css">`;

      html += `<table class="styled-table">`;
      html += '<thead><tr>';
      cols.forEach(c => {
        html += `<th>${c.name}</th>`;
      });
      html += '</tr></thead><tbody>';

      props.tasks.forEach(task => {
        html += '<tr>';
        cols.forEach(c => {
          let content = getTaskValue(task, c.id);
          let cellStyle = '';

          if (c.id === 'status') {
            const color = getStatusColor(task.statusId);
            content = `<span class="badge status-badge" style="background-color: ${color};">${content}</span>`;
          } else if (c.id === 'priority') {
            const color = getPriorityColor(task.priorityId);
            const icon = getPriorityIcon(task.priorityId);
            content = `<span class="badge priority-badge" style="background-color: ${color};"><i class="bi ${icon}"></i>${content}</span>`;
          } else if (c.id === 'start' || c.id === 'end') {
            const color = getDateColor(task[c.id === 'start' ? 'start' : 'end'], task);
            cellStyle = `color: ${color};`;
          }

          html += `<td style="${cellStyle}">${content}</td>`;
        });
        html += '</tr>';
      });
      html += '</tbody></table>';
      return html;
    };

    const generatePrettyTable = () => {
      const cols = orderedColumns.value.filter(c => selectedColumns.value.includes(c.id));
      if (cols.length === 0) return '';

      // Calculate column widths
      const colWidths = cols.map(c => {
        let max = c.name.length;
        props.tasks.forEach(task => {
          const val = String(getTaskValue(task, c.id) || '');
          if (val.length > max) max = val.length;
        });
        return max;
      });

      const buildRow = (values, char = ' ') => {
        return '| ' + values.map((v, i) => String(v).padEnd(colWidths[i], char)).join(' | ') + ' |';
      };

      const buildSeparator = () => {
        return '+' + colWidths.map(w => '-'.repeat(w + 2)).join('+') + '+';
      };

      let table = buildSeparator() + '\n';
      table += buildRow(cols.map(c => c.name)) + '\n';
      table += buildSeparator() + '\n';

      props.tasks.forEach(task => {
        table += buildRow(cols.map(c => getTaskValue(task, c.id))) + '\n';
      });

      table += buildSeparator();
      return table;
    };

    const downloadFile = (content, fileName, mimeType) => {
      const blob = new Blob([content], { type: mimeType });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    };

    const doExport = async (action) => {
      let content = '';
      let fileName = `export_${new Date().getTime()}`;
      let mimeType = 'text/plain';

      if (format.value === 'csv') {
        content = generateCSV();
        fileName += '.csv';
        mimeType = 'text/csv';
      } else if (format.value === 'text') {
        content = generateText();
        fileName += '.txt';
      } else if (format.value === 'html') {
        content = generateHTML();
        fileName += '.html';
        mimeType = 'text/html';
      } else if (format.value === 'html-styled') {
        content = generateStyledHTML();
        fileName += '_styled.html';
        mimeType = 'text/html';
      } else if (format.value === 'table') {
        content = generatePrettyTable();
        fileName += '.txt';
      } else if (format.value === 'excel') {
         if (action === 'file') {
            await downloadExcel();
            return;
         }
      }

      if (action === 'copy') {
        try {
          await navigator.clipboard.writeText(content);
          alert('Copied to clipboard!');
        } catch (err) {
          console.error('Failed to copy!', err);
        }
      } else if (action === 'file') {
        downloadFile(content, fileName, mimeType);
      }
    };

    const updatePreview = () => {
      if (format.value === 'csv') {
        previewContent.value = generateCSV();
      } else if (format.value === 'text') {
        previewContent.value = generateText();
      } else if (format.value === 'html') {
        previewContent.value = generateHTML();
      } else if (format.value === 'html-styled') {
        previewContent.value = generateStyledHTML();
      } else if (format.value === 'table') {
        previewContent.value = generatePrettyTable();
      } else {
        previewContent.value = '';
      }
    };

    const downloadExcel = async () => {
      try {
        const taskIds = props.tasks.map(t => t.id);
        const cols = orderedColumns.value.filter(c => selectedColumns.value.includes(c.id)).map(c => c.id);
        
        const response = await fetch('/api/tasks/export/excel', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ taskIds, columns: cols })
        });
        
        if (response.ok) {
          const blob = await response.blob();
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `tasks_export_${new Date().toISOString().slice(0, 10)}.xml`;
          document.body.appendChild(a);
          a.click();
          a.remove();
          window.URL.revokeObjectURL(url);
        } else {
          console.error('Excel export failed');
        }
      } catch (error) {
        console.error('Error exporting to Excel:', error);
      }
    };

    return {
      format,
      selectedColumns,
      orderedColumns,
      previewContent,
      dragIndex,
      close,
      doExport,
      onDragStart,
      onDragOver,
      onDragLeave,
      onDrop
    };
  }
};
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.7);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 3000;
  backdrop-filter: blur(2px);
}

.export-modal {
  width: 95%;
  max-width: 800px;
  background-color: var(--bg-dark);
  border-radius: 12px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.5);
  display: flex;
  flex-direction: column;
}

.modal-header, .modal-footer {
  padding: 1rem 1.5rem;
  background-color: rgba(255, 255, 255, 0.02);
  flex-shrink: 0;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid var(--border-primary);
  border-top-left-radius: 12px;
  border-top-right-radius: 12px;
  overflow: hidden;
}

.modal-footer {
  border-top: 1px solid var(--border-primary);
  border-bottom-left-radius: 12px;
  border-bottom-right-radius: 12px;
  overflow: hidden;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  padding: 0;
  line-height: 1;
  transition: color 0.2s;
}

.close-btn:hover {
  color: var(--text-primary) !important;
}

.preview-box {
  font-family: monospace;
}

.column-item {
  border: 1px solid transparent;
  transition: all 0.2s;
  background-color: rgba(255, 255, 255, 0.03);
}

.column-item:hover {
  background-color: rgba(255, 255, 255, 0.07);
  border-color: var(--border-primary);
}

.column-item.dragging {
  opacity: 0.5;
  background-color: var(--bg-card);
}

.drag-handle {
  cursor: grab;
  font-size: 1.2rem;
  display: flex;
  align-items: center;
}

.drag-handle:active {
  cursor: grabbing;
}

.cursor-pointer {
  cursor: pointer;
}

.column-selection-list {
  max-height: 300px;
  overflow-y: auto;
}

.btn-subtle {
  background: transparent;
  border: 1px solid transparent;
  color: var(--text-muted);
  padding: 6px 16px;
  border-radius: 4px;
  font-size: 0.9rem;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  cursor: pointer;
}

.btn-subtle:hover {
  background-color: var(--bg-card);
  color: var(--text-primary);
  border-color: var(--border-primary);
}

.column-selection-list::-webkit-scrollbar,
.preview-box::-webkit-scrollbar {
  width: 6px;
}

.column-selection-list::-webkit-scrollbar-track,
.preview-box::-webkit-scrollbar-track {
  background: transparent;
}

.column-selection-list::-webkit-scrollbar-thumb,
.preview-box::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 3px;
}

.column-selection-list::-webkit-scrollbar-thumb:hover,
.preview-box::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.2);
}
</style>
