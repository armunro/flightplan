export async function getScheduledTasks() {
    try {
        const response = await fetch('/api/scheduledtasks');
        if (response.ok) {
            return await response.json();
        }
    } catch (error) {
        console.error('Error fetching scheduled tasks:', error);
    }
    return [];
}

export async function createScheduledTask(task) {
    try {
        const response = await fetch('/api/scheduledtasks', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(task)
        });
        if (response.ok) {
            return await response.json();
        }
    } catch (error) {
        console.error('Error creating scheduled task:', error);
    }
    return null;
}

export async function updateScheduledTask(id, task) {
    try {
        const response = await fetch(`/api/scheduledtasks/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(task)
        });
        if (response.ok) {
            return await response.json();
        }
    } catch (error) {
        console.error('Error updating scheduled task:', error);
    }
    return null;
}

export async function deleteScheduledTask(id) {
    try {
        const response = await fetch(`/api/scheduledtasks/${id}`, {
            method: 'DELETE'
        });
        return response.ok;
    } catch (error) {
        console.error('Error deleting scheduled task:', error);
    }
    return false;
}

export async function getProjects() {
    try {
        const response = await fetch('/api/projects');
        if (response.ok) {
            return await response.json();
        }
    } catch (error) {
        console.error('Error fetching projects:', error);
    }
    return [];
}
