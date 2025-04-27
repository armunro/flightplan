# FlightPlan
Productivity and wellbeing for nerds.

## Concepts

### Waypoints
Waypoints are FlightPlan's way of representing external time/task management resources like tasks and events, as visual checkpoints. 
```json
 {
  "type": "Task",
  "key": "MH-123",
  "source": "ClickUp",
  "color": "White",
  "title": "Laundry - Adults",
  "link": "",
  "time": "2023-09-16T17:00:00.000Z",
  "spans": [
    {
      "spanType": "Work",
      "start": "2023-09-16T17:00:00.000Z",
      "end": "2023-09-16T17:20:00.000Z"
    }
  ]
}

```