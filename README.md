# FlightPlan
Productivity and wellbeing for nerds.

```mermaid
classDiagram

    class TimeSpan {
        start: DateTime
        end: DateTime
    }
    
    class Task {
        id: Uuid
        source: String
        key: String
        link: String
        priority: String
        spans: TimeSpan
        cycles: List:Cycle
    }

    class Source {
        id: Uuid
        name: String
    }

    class Cycle {
        id: Uuid
        start: DateTime
        end: DateTime
    }

    class Event {
        id: Uuid
        title: String
        location: string
        start: DateTime
        end: DateTime
    }

    class Transit {
        timeBefore: TimeSpan
        timeAfter: TimeSpan
        Notes: string
    }

    class Reminder {
        id: Uuid
        time: DateTime
        content: String
    }

    Task "1" --> "1..*" Cycle

    Task "1" --> "1..0" TimeSpan
    Event "1" --> "0..*" Transit
    
```