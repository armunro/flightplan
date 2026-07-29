<template>
  <div class="task-detail-overlay" :class="themeClass">
    <div class="task-detail-modal">
      <div class="task-detail-header" v-if="localTask">
        <h2 contenteditable="true" @blur="onUpdateTitle">{{ localTask.title }}</h2>
        <button class="close-btn" @click="$emit('close')">&times;</button>
      </div>
      
      <div class="task-detail-body" v-if="localTask">
        <div class="detail-row">
            <div class="detail-field flex-1">
              <label>Type</label>
              <div class="dropdown">
                <span class="type-badge dropdown-toggle" 
                      data-bs-toggle="dropdown"
                      :style="{ color: getTaskTypeColor(localTask.taskTypeId) }">
                  <i :class="getTaskTypeIcon(localTask.taskTypeId)"></i>
                  {{ getTaskTypeName(localTask.taskTypeId) }}
                </span>
                <ul class="dropdown-menu border-secondary shadow" :class="{ 'dropdown-menu-dark': theme === 'Cosmic' }">
                  <li><a class="dropdown-item" href="#" @click.prevent="onUpdateTaskType({ target: { value: null } })">-- Type --</a></li>
                  <li v-for="t in projectTaskTypes" :key="t.id">
                    <a class="dropdown-item" href="#" @click.prevent="onUpdateTaskType({ target: { value: t.id } })" :style="{ color: t.color }">
                      <i :class="t.icon" class="me-2"></i>{{ t.name }}
                    </a>
                  </li>
                </ul>
              </div>
            </div>

            <div class="detail-field flex-1">
              <label>Status</label>
            <div class="dropdown">
              <span class="status-badge dropdown-toggle" 
                    data-bs-toggle="dropdown"
                    :style="{ color: getStatusColor(localTask.statusId) }">
                <i class="bi bi-circle-fill" style="font-size: 8px; margin-right: 6px;"></i>
                {{ getStatusName(localTask.statusId) }}
              </span>
              <ul class="dropdown-menu border-secondary shadow" :class="{ 'dropdown-menu-dark': theme === 'Cosmic' }">
                <li v-for="s in projectStatuses" :key="s.id">
                  <a class="dropdown-item" href="#" @click.prevent="onUpdateStatus({ target: { value: s.id } })" :style="{ color: s.color }">
                    <i class="bi bi-circle-fill me-2" style="font-size: 8px;"></i>{{ s.name }}
                  </a>
                </li>
              </ul>
            </div>
          </div>

          <div class="detail-field flex-1">
            <label>Priority</label>
            <div class="dropdown">
              <span class="priority priority-badge dropdown-toggle" 
                    data-bs-toggle="dropdown"
                    :style="{ color: getPriorityColor(localTask.priorityId) }">
                <i :class="getPriorityIcon(localTask.priorityId)" style="margin-right: 6px;"></i>
                {{ getPriorityName(localTask.priorityId) }}
              </span>
              <ul class="dropdown-menu border-secondary shadow" :class="{ 'dropdown-menu-dark': theme === 'Cosmic' }">
                <li v-for="p in projectPriorities" :key="p.id">
                  <a class="dropdown-item" href="#" @click.prevent="onUpdatePriorityId(p.id)" :style="{ color: p.color }">
                    <i :class="p.icon" class="me-2"></i>{{ p.name }}
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </div>

        <div class="detail-row">
          <div class="detail-field flex-1">
            <label>Start</label>
            <date-time-selector 
              v-model="localTask.start" 
              placeholder="e.g. tomorrow 10am"
              :is-closed="localTask.isCompleted"
              @update:model-value="onUpdateField"
            />
          </div>
          <div class="detail-field flex-1">
            <label>End</label>
            <date-time-selector 
              v-model="localTask.end" 
              placeholder="e.g. Next Week"
              :is-closed="localTask.isCompleted"
              @update:model-value="onUpdateField"
            />
          </div>
          <div class="detail-field flex-1">
            <label>Estimate (min)</label>
            <input type="number" v-model.number="localTask.estimateMinutes" @blur="onUpdateField" class="form-control">
          </div>
        </div>

        <div class="detail-field">
          <label>Link</label>
          <div class="input-with-actions has-actions">
            <input type="text" v-model="localTask.link" @blur="onUpdateField" class="form-control" placeholder="https://...">
            <div class="input-actions">
              <button class="action-btn" @click="copyLink" title="Copy Link" v-if="localTask.link">
                <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path></svg>
              </button>
              <button class="action-btn" @click="launchLink" title="Launch Link" v-if="localTask.link">
                <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"></path><polyline points="15 3 21 3 21 9"></polyline><line x1="10" y1="14" x2="21" y2="3"></line></svg>
              </button>
            </div>
          </div>
        </div>
        
        <div class="detail-field">
          <label>Description</label>
          <textarea 
            v-model="localTask.description" 
            @blur="onUpdateDescription" 
            class="form-control description-textarea"
            placeholder="Enter description..."
          ></textarea>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, watch, onMounted, onUnmounted } from 'vue';
import { showToast } from './Toast.vue';
import DateTimeSelector from './DateTimeSelector.vue';
import { updateTask } from '../js/tasks-api';
import { formatForInput, formatToISO } from '../js/utils';

export default {
  name: 'TaskDetail',
  components: {
    DateTimeSelector
  },
  props: ['task', 'projectStatuses', 'projectTaskTypes', 'projectPriorities', 'theme'],
  emits: ['close', 'refresh'],
  setup(props, { emit }) {
    const localTask = ref({ 
      ...props.task
    });

    const onKeyDown = (e) => {
      if (e.key === 'Escape') {
        emit('close');
      }
    };

    onMounted(() => {
      window.addEventListener('keydown', onKeyDown);
    });

    onUnmounted(() => {
      window.removeEventListener('keydown', onKeyDown);
    });

    watch(() => props.task, (newVal) => {
      if (!newVal) return;
      localTask.value = { 
        ...newVal
      };
    }, { deep: true });

    const getPriorityName = (priorityId) => {
      const priority = props.projectPriorities?.find(p => p.id === priorityId);
      return priority ? priority.name : '';
    };

    const getStatusName = (statusId) => {
      const status = props.projectStatuses?.find(s => s.id === statusId);
      return status ? status.name : 'Unknown';
    };

    const getStatusColor = (statusId) => {
      const status = props.projectStatuses?.find(s => s.id === statusId);
      return status ? status.color : '#cccccc';
    };

    const getTaskTypeName = (typeId) => {
      const type = props.projectTaskTypes?.find(t => t.id === typeId);
      return type ? type.name : 'Work';
    };

    const getTaskTypeColor = (typeId) => {
      const type = props.projectTaskTypes?.find(t => t.id === typeId);
      return type ? type.color : '#3498db';
    };

    const getTaskTypeIcon = (typeId) => {
      const type = props.projectTaskTypes?.find(t => t.id === typeId);
      return type ? type.icon : 'bi-briefcase';
    };

    const getPriorityIcon = (priorityId) => {
      const priority = props.projectPriorities?.find(p => p.id === priorityId);
      return priority ? priority.icon : 'bi-dash-lg';
    };

    const getPriorityColor = (priorityId) => {
      const priority = props.projectPriorities?.find(p => p.id === priorityId);
      return priority ? priority.color : '#ccc';
    };

    const onUpdateTitle = async (e) => {
      const newTitle = e.target.innerText;
      if (newTitle !== localTask.value.title) {
        localTask.value.title = newTitle;
        await saveTask();
      }
    };

    const onUpdateDescription = async () => {
      await saveTask();
    };

    const onUpdateField = async () => {
      await saveTask();
    };

    const copyLink = () => {
      if (!localTask.value.link) return;
      
      const url = localTask.value.link.startsWith('http') ? localTask.value.link : 'https://' + localTask.value.link;
      const text = localTask.value.link;
      const html = `<a href="${url}">${text}</a>`;
      
      if (window.ClipboardItem) {
        const fullHtml = `<!DOCTYPE html><html><head><meta charset="utf-8"></head><body>${html}</body></html>`;
        
        const plainText = new Blob([text], { type: 'text/plain' });
        const htmlText = new Blob([fullHtml], { type: 'text/html' });
        const clipboardItem = new ClipboardItem({
          'text/plain': plainText,
          'text/html': htmlText
        });
        navigator.clipboard.write([clipboardItem]).then(() => {
          console.log('Successfully copied rich text to clipboard');
        }).catch(err => {
          console.error('Failed to copy rich text: ', err);
          showToast('Failed to copy rich text. Falling back to plain text. Error: ' + err.message, 'warning');
          navigator.clipboard.writeText(text);
        });
      } else {
        navigator.clipboard.writeText(text);
      }
    };

    const launchLink = () => {
      if (!localTask.value.link) return;
      let url = localTask.value.link;
      if (!url.startsWith('http://') && !url.startsWith('https://')) {
        url = 'https://' + url;
      }
      window.open(url, '_blank');
    };

    const onUpdatePriorityId = async (priorityId) => {
      localTask.value.priorityId = priorityId;
      await saveTask();
    };

    const onUpdateStatus = async (e) => {
      const newStatusId = e.target.value;
      if (!newStatusId) return;
      const newStatus = props.projectStatuses?.find(s => s.id === newStatusId);
      localTask.value.statusId = newStatusId;
      localTask.value.isCompleted = newStatus?.isCompletedState || false;
      await saveTask();
    };

    const onUpdateTaskType = async (e) => {
      localTask.value.taskTypeId = e.target.value || null;
      await saveTask();
    };

    const saveTask = async () => {
      if (!localTask.value) return;
      
      // Create a clean copy for the API
      const taskToSave = { 
        ...localTask.value
      };
      
      // Ensure we don't send subtasks recursively as it's not needed for update and might cause issues
      if (taskToSave.subtasks) {
        delete taskToSave.subtasks;
      }
      
      await updateTask(localTask.value.id, taskToSave);
      emit('refresh');
    };

    return {
      localTask,
      getPriorityName,
      getStatusName,
      getStatusColor,
      getPriorityIcon,
      getPriorityColor,
      onUpdateTitle,
      onUpdateDescription,
      onUpdatePriorityId,
      onUpdateStatus,
      onUpdateTaskType,
      getTaskTypeName,
      getTaskTypeColor,
      getTaskTypeIcon,
      onUpdateField,
      copyLink,
      launchLink
    };
  }
};
</script>

<style scoped>
.task-detail-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.75);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.task-detail-modal {
  background: var(--bg-dark);
  width: 900px;
  max-width: 95%;
  max-height: 90vh;
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.5);
  color: var(--text-primary);
  border: 1px solid var(--border-primary);
}

.task-detail-header {
  padding: 20px;
  border-bottom: 1px solid var(--border-primary);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.task-detail-header h2 {
  margin: 0;
  padding: 5px;
  border-radius: 4px;
  color: var(--text-primary);
}

.task-detail-header h2:hover {
  background: var(--bg-card);
}

.close-btn {
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
  color: var(--text-muted);
}

.close-btn:hover {
  color: var(--accent-red);
}

.task-detail-body {
  padding: 20px;
  overflow-y: auto;
}

.detail-field {
  margin-bottom: 20px;
}

.detail-field label {
  display: block;
  font-weight: bold;
  margin-bottom: 8px;
  color: var(--text-muted);
  font-size: 0.85em;
  text-transform: uppercase;
  cursor: pointer;
}

.detail-row {
  display: flex;
  gap: 15px;
}

.flex-1 {
  flex: 1;
}

.form-control {
  width: 100%;
  padding: 8px;
  border: 1px solid var(--border-primary);
  background: var(--bg-darker);
  color: var(--text-primary);
  border-radius: 4px;
}

.form-control:focus {
  outline: none;
  border-color: var(--accent-blue);
}

.input-with-actions {
  position: relative;
  display: flex;
  align-items: center;
  cursor: pointer;
}

.input-with-actions .form-control {
  padding-right: 10px;
}

.input-with-actions.has-actions .form-control {
  padding-right: 70px;
}

.input-actions {
  position: absolute;
  right: 5px;
  display: flex;
  gap: 2px;
}

.action-btn {
  background: transparent;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  padding: 6px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.action-btn:hover {
  background: var(--bg-card);
  color: var(--text-primary);
}

.status-badge, .priority-badge, .type-badge {
  padding: 4px 0;
  border-radius: 4px;
  font-size: 0.85em;
  font-weight: bold;
  color: white;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  text-transform: uppercase;
  background-color: transparent;
  gap: 6px;
}

.type-badge {
}

.dropdown-container {
  position: relative;
  display: inline-block;
}

.hidden-select {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  opacity: 0;
  cursor: pointer;
}

.priority-Lowest { }
.priority-Low { }
.priority-Medium { }
.priority-High { }
.priority-Highest { }
.priority-Critical { background-color: #ff0000; color: #fff; box-shadow: 0 0 10px rgba(255,0,0,0.3); }
.description-textarea {
  min-height: 200px;
  resize: vertical;
}
</style>
