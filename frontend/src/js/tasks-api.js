export async function getTask(taskId) {
    try {
        const response = await fetch(`/api/tasks/${taskId}`);
        if (response.ok) {
            return await response.json();
        }
    } catch (error) {
        console.error('Error fetching task:', error);
    }
    return null;
}

export async function updateTask(taskId, data) {
    try {
        await fetch(`/api/tasks/${taskId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
    } catch (error) {
        console.error('Error updating task:', error);
    }
}

export async function addTask(listId, title, statusId = null) {
    try {
        await fetch('/api/tasks', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ listId, title, priority: 2, statusId })
        });
    } catch (error) {
        console.error('Error adding task:', error);
    }
}

export async function addSubtask(taskId, title, statusId = null) {
    try {
        const response = await fetch(`/api/tasks/${taskId}/subtasks`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ title, priority: 2, statusId })
        });
        if (response.ok) return await response.json();
    } catch (error) {
        console.error('Error adding subtask:', error);
    }
    return null;
}

export async function addSibling(taskId, title, statusId = null) {
    try {
        const response = await fetch(`/api/tasks/${taskId}/sibling`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ title, priority: 2, statusId })
        });
        if (response.ok) return await response.json();
    } catch (error) {
        console.error('Error adding sibling:', error);
    }
    return null;
}

export async function deleteTask(taskId) {
    try {
        await fetch(`/api/tasks/${taskId}`, {
            method: 'DELETE'
        });
    } catch (error) {
        console.error('Error deleting task:', error);
    }
}

export async function bulkDeleteTasks(taskIds) {
    try {
        await fetch('/api/tasks/bulk-delete', {
            method: 'DELETE',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ taskIds })
        });
    } catch (error) {
        console.error('Error bulk deleting tasks:', error);
    }
}

export async function bulkUpdateTasks(taskIds, data) {
    try {
        await fetch('/api/tasks/bulk-update', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ taskIds, ...data })
        });
    } catch (error) {
        console.error('Error bulk updating tasks:', error);
    }
}

export async function moveTask(taskId, targetListId, targetTaskId, position = 'Inside') {
    try {
        const response = await fetch(`/api/tasks/${taskId}/move`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ targetListId, targetTaskId, position })
        });
        if (!response.ok) {
            console.error('Error moving task:', response.status, response.statusText);
            const errorText = await response.text();
            console.error('Error details:', errorText);
        }
    } catch (error) {
        console.error('Error moving task:', error);
    }
}

export async function bulkMoveTasks(taskIds, targetListId, targetTaskId, position = 'Inside') {
    try {
        await fetch('/api/tasks/bulk-move', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ taskIds, targetListId, targetTaskId, position })
        });
    } catch (error) {
        console.error('Error bulk moving tasks:', error);
    }
}

export async function addList(projectId, name) {
    try {
        await fetch(`/api/projects/${projectId}/lists`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name })
        });
    } catch (error) {
        console.error('Error adding list:', error);
    }
}

export async function moveList(projectId, listId, targetListId, position = 'After') {
    try {
        await fetch(`/api/projects/${projectId}/lists/${listId}/move`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ targetListId, position })
        });
    } catch (error) {
        console.error('Error moving list:', error);
    }
}

export async function updateList(projectId, listId, name) {
    try {
        await fetch(`/api/projects/${projectId}/lists/${listId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name })
        });
    } catch (error) {
        console.error('Error updating list:', error);
    }
}

export async function deleteList(projectId, listId) {
    try {
        await fetch(`/api/projects/${projectId}/lists/${listId}`, {
            method: 'DELETE'
        });
    } catch (error) {
        console.error('Error deleting list:', error);
    }
}

export async function updateProject(projectId, data) {
    try {
        await fetch(`/api/projects/${projectId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
    } catch (error) {
        console.error('Error updating project:', error);
    }
}

export async function addProject(data) {
    try {
        await fetch('/api/projects', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
    } catch (error) {
        console.error('Error adding project:', error);
    }
}

export async function moveProject(projectId, targetProjectId, position = 'After') {
    try {
        await fetch(`/api/projects/${projectId}/move`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ targetProjectId, position })
        });
    } catch (error) {
        console.error('Error moving project:', error);
    }
}

export async function deleteProject(projectId) {
    try {
        await fetch(`/api/projects/${projectId}`, {
            method: 'DELETE'
        });
    } catch (error) {
        console.error('Error deleting project:', error);
    }
}

export async function unassignJiraIssue(issueKey) {
    try {
        const response = await fetch(`/api/jira/unassign?key=${issueKey}`, {
            method: 'POST'
        });
        return response.ok;
    } catch (error) {
        console.error('Error unassigning Jira issue:', error);
        return false;
    }
}
