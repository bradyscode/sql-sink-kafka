# User Actions Kafka Stream Processing

A real-time data processing system that simulates user actions, streams them through Apache Kafka, and stores them in PostgreSQL. This project demonstrates event-driven architecture using .NET Core, Kafka, and Docker.

## Overview

This system consists of two main components:

- **Producer**: Generates simulated user action events and publishes them to a Kafka topic
- **Consumer**: Consumes events from Kafka and persists them to a PostgreSQL database

## Architecture

```
Producer → Kafka Topic → Consumer → PostgreSQL
```

The system processes various user actions including login/logout, page views, purchases, social interactions, and subscription events.

## Features

- **Event Generation**: Simulates realistic user actions with metadata (IP address, device info)
- **Stream Processing**: Uses Kafka for reliable message queuing and processing
- **Data Persistence**: Stores processed events in PostgreSQL with proper schema
- **Fault Tolerance**: Implements retry logic and graceful error handling
- **Containerized**: Fully containerized with Docker Compose for easy deployment

## User Actions Tracked

The system tracks the following user actions:
- Authentication: Login, Logout
- Navigation: PageView, Search
- E-commerce: Purchase, AddToCart, RemoveFromCart
- Profile: ProfileUpdate
- Social: FollowUser, UnfollowUser, Like, Unlike, Comment, DeleteComment
- Subscription: SubscriptionStart, SubscriptionCancel

## Prerequisites

- Docker and Docker Compose
- .NET 8.0 SDK (for local development)

## Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/bradyscode/sql-sink-kafka
   cd sql-sink-kafka
   ```

2. **Start all services**
   ```bash
   docker-compose up -d
   ```

3. **Monitor the logs**
   ```bash
   # View producer logs
   docker-compose logs -f producer
   
   # View consumer logs
   docker-compose logs -f consumer
   ```

4. **Verify data in PostgreSQL**
   ```bash
   docker exec -it postgres psql -U myuser -d MyDb
   SELECT * FROM useractions LIMIT 10;
   ```

## Services

### Producer
- **Port**: Internal only
- **Function**: Generates user action events every 500ms
- **Output**: JSON messages to `user-actions` Kafka topic

### Consumer
- **Port**: Internal only
- **Function**: Processes messages from Kafka and stores in PostgreSQL
- **Features**: Automatic table creation, retry logic, graceful shutdown

### Kafka
- **Port**: 9092
- **Zookeeper Port**: 2181
- **Topic**: `user-actions`

### PostgreSQL
- **Port**: 5432
- **Database**: MyDb
- **User**: myuser
- **Password**: mypassword

## Database Schema

The consumer automatically creates the following table:

```sql
CREATE TABLE useractions (
    id SERIAL PRIMARY KEY,
    userid UUID NOT NULL,
    action TEXT NOT NULL,
    ipaddress TEXT,
    device TEXT,
    timestamp TIMESTAMPTZ NOT NULL
);
```

## Configuration

### Environment Variables

You can customize the system by modifying the `docker-compose.yml` file:

- **Kafka Configuration**: Modify broker settings in the `kafka` service
- **Database Configuration**: Update PostgreSQL credentials in the `postgres` service
- **Consumer Group**: Change `GroupId` in the consumer code for different consumer groups

### Message Format

Messages are sent in JSON format:

```json
{
  "UserId": "550e8400-e29b-41d4-a716-446655440000",
  "Action": "Purchase",
  "Metadata": {
    "IpAddress": "192.168.1.145",
    "Device": "Chrome on Windows"
  },
  "Timestamp": "2024-01-15T10:30:00Z"
}
```

## Development

### Local Development Setup

1. **Install dependencies**
   ```bash
   # In producer directory
   cd producer
   dotnet restore
   
   # In consumer directory
   cd ../consumer
   dotnet restore
   ```

2. **Run services locally**
   ```bash
   # Start infrastructure only
   docker-compose up -d zookeeper kafka postgres
   
   # Run producer locally
   cd producer
   dotnet run
   
   # Run consumer locally (in another terminal)
   cd consumer
   dotnet run
   ```

### Building Docker Images

```bash
# Build producer image
docker build -f Dockerfile.producer -t user-actions-producer .

# Build consumer image
docker build -f Dockerfile.consumer -t user-actions-consumer .
```

## Monitoring and Troubleshooting

### Checking Service Health

```bash
# Check all services status
docker-compose ps

# Check Kafka topics
docker exec -it kafka kafka-topics.sh --bootstrap-server localhost:9092 --list

# Check consumer group status
docker exec -it kafka kafka-consumer-groups.sh --bootstrap-server localhost:9092 --describe --group user-action-loader
```

### Common Issues

1. **Services not starting**: Ensure Docker has enough memory allocated (at least 4GB recommended)

2. **Connection refused errors**: Wait for services to fully initialize (Kafka can take 30-60 seconds)

3. **Consumer lag**: Monitor consumer group lag using Kafka tools

## Scaling

### Horizontal Scaling

To scale consumers for higher throughput:

```bash
docker-compose up -d --scale consumer=3
```

### Partitioning

For production use, consider:
- Partitioning the Kafka topic by user ID
- Using multiple consumer instances
- Implementing proper error handling and dead letter queues

## Production Considerations

- **Security**: Implement proper authentication for Kafka and PostgreSQL
- **Monitoring**: Add metrics collection and alerting
- **Persistence**: Configure proper data retention policies
- **Performance**: Tune Kafka and PostgreSQL configurations
- **High Availability**: Set up Kafka and database clustering

## License

This project is provided as-is for educational and demonstration purposes.

## Contributing

Feel free to submit issues and enhancement requests!