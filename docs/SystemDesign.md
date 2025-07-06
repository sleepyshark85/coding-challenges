## System overview

```mermaid
graph TB
    %% Client Layer
    WEB[Client Application]
    
    %% Microservices
    subgraph "Microservices Layer"
        subgraph "Quiz API Service (Port 5001)"
            QA[Quiz API<br/>• Answer Processing<br/>• Score Calculation<br/>• Business Logic<br/>• Cache Management]
        end
        
        subgraph "WebSocket Service (Port 5002)"
            WS[WebSocket Hub<br/>• Real-time Updates<br/>• Connection Management<br/>• Event Broadcasting<br/>• SignalR]
        end
    end
    
    %% Database Layer
    subgraph "Database Layer"
        PG_MASTER[PostgreSQL Master<br/>• Quiz Data<br/>• User Scores<br/>• Answer Records<br/>• ACID Transactions]
    end
    
    %% Caching Layer
    subgraph "Caching Layer"
        subgraph "Redis Cluster"
            REDIS_CACHE[Redis Cache<br/>• Quiz Data Cache<br/>• Session Storage<br/>• Leaderboards]
            REDIS_PUBSUB[Redis Pub/Sub<br/>• Event Broadcasting<br/>• Service Communication<br/>• Real-time Events]
        end
        
        subgraph "Application Cache"
            MEM_CACHE[In-Memory Cache<br/>• Active Quiz Sessions<br/>• Frequent Data<br/>• 30min TTL]
        end
    end
    
    %% Connection Flows
    
    %% Client to WebSocket (Real-time)
    WEB -.->|"WebSocket Connection"<br/>Route /quizHub| WS
    
    %% Load Balancer Routing
    WEB -->|"Route /api/*"| QA
    
    %% Inter-Service Communication
    QA -->|"Publish Events<br/>Score Updates"| REDIS_PUBSUB
    WS -->|"Subscribe Events<br/>Broadcast Updates"| REDIS_PUBSUB
    
    %% Caching Flows
    QA <-->|"Cache Quiz Data<br/>L1: Memory, L2: Redis"| MEM_CACHE
    QA <-->|"Cache Questions<br/>Answer Keys, Points"| REDIS_CACHE
    WS <-->|"Session Storage<br/>Connection State"| REDIS_CACHE
    
    %% Database Connections
    QA -->|"Read/Write Operations<br/>Scores, Answers"| PG_MASTER
    
    
    %% Styling
    classDef clientClass fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef serviceClass fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef cacheClass fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef dbClass fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px
    classDef infraClass fill:#fce4ec,stroke:#880e4f,stroke-width:2px
    
    class WEB clientClass
    class QA,WS serviceClass
    class REDIS_CACHE,REDIS_PUBSUB,MEM_CACHE cacheClass
    class PG_MASTER dbClass
```

### Data flow
This diagram describes the flow from when a user joins a quiz to when the leaderboard is updated
```mermaid
sequenceDiagram
    participant Client
    participant QuizAPI
    participant WebSocket
    participant MemoryCache
    participant RedisCache
    participant Database

    Note over Client,Database: 1. User Joins Quiz
    Client->>QuizAPI: POST /quiz/join
    QuizAPI->>MemoryCache: Get quiz data
    MemoryCache-->>QuizAPI: Cache miss
    QuizAPI->>RedisCache: Get quiz data
    RedisCache-->>QuizAPI: Cache miss
    QuizAPI->>Database: Query quiz + questions
    Database-->>QuizAPI: Quiz data
    QuizAPI->>RedisCache: Store quiz (24h TTL)
    QuizAPI->>MemoryCache: Store quiz (30min TTL)
    QuizAPI->>Database: Create user session
    Database-->>QuizAPI: Session created
    QuizAPI-->>Client: Quiz details

    Note over Client,Database: 2. Connect for Real-time Updates
    Client->>WebSocket: Connect to quiz room
    WebSocket-->>Client: Connected

    Note over Client,Database: 3. User Submits Answer
    Client->>QuizAPI: POST /quiz/answer
    QuizAPI->>MemoryCache: Get quiz data (cache hit)
    MemoryCache-->>QuizAPI: Cached quiz data
    QuizAPI->>Database: Save answer & update score
    Database-->>QuizAPI: Score updated
    QuizAPI->>RedisCache: Publish score update event
    QuizAPI-->>Client: Answer result

    Note over Client,Database: 4. Real-time Score Broadcast
    RedisCache->>WebSocket: Score update event
    WebSocket->>Client: Broadcast new scores
    Note over Client: Leaderboard updated in real-time
```

### AI Collaboration in Design
#### Discuss current Elsa's system design, tech stacks, infrastructure ...
This prompt was, mostly, for me to have any overview of how Elsa is built and the technical stacks that are being used. Elsa is using AWS, which doesn't really friendly for a quick coding challenge, so I decided to use another stack, specifically for the demonstration purpose.

#### Elsa number of users, concurrent users
I want to know the number of users/concurrent users the Quiz application need to serve. Elsa have over 15 millions total users, with around 4 millions recording daily. This is the conservative estimate of number of concurrent users:
- **Normal peak**: 25k-50k concurrent users
- **Global peak hours**: 75k-100k+ concurrent users
- **Event-driven spikes**: 150k+ concurrent users

Which is high. It's the main reason I chose the current architecture design.

### Key decisions
- **Service Separation**: WebSocket service isolated to handle connection overhead without affecting API performance. Allow them to be scaled independently.
- **Multi-Level Caching**: L1 (in-memory) + L2 (Redis) reduces database load by ~90%. Quiz data should rarely be changed but will be used very frequently. It's the perfect candidate for caching.
- **Redis Pub/Sub**: Enables real-time score updates across all connected clients. Simple to use. Consider using Kafka for higher throughput, better durability and guarantee delivery.
- **Partially adopt DDD**: For better code maintainability.
    - Enhanced encapsulation and behavior-rich domain models.
    - Clear separation of concerns between application logic, domain logic and infrastructure.
    - (Almost) Always-valid models