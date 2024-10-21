# todo.storage

## Table of Contents
1. [Overview](#overview)
2. [Installation](#installation)
3. [Usage](#usage)
4. [API Endpoints](#api-endpoints)
    - [Todo Controller](#todo-controller)
    - [User Controller](#user-controller)
5. [SQS Listener](#sqs-listener)
6. [Contributing](#contributing)

## Overview
The `todo.storage` repository provides the data management service for the Todo application. This service handles all data-related operations, including storing, retrieving, and processing todo items asynchronously. It integrates with the `todo.users` service to manage user interactions and utilizes a queue for background processing of data tasks.

### Technologies Used
- .NET 8
- Entity Framework Core
- MySql 8
- AWS SQS for reading message queue
- AWS SNS for posting notifications

## Installation
To set up the `todo.users` service locally, follow these steps:

1. Clone the repository:
   ```bash
   git clone https://github.com/coder755/todo.storage.git
   cd todo.users
   ```
2. Ensure you have .NET SDK installed. You can download it from [here](https://dotnet.microsoft.com/en-us/download).
3. Install the dependencies
4. Run the application
      ```bash
   dotnet run
   ```
   
## Usage
Once the service is running, it will listen for messages from the queue and handle them accordingly. You can interact with the API using tools like Postman or curl.

## API Endpoints

### Todo Controller
- **GET** `/api/todo/v1/{userId}`
   - Description: Retrieves all todo items for a specific user by user ID.

- **POST** `/api/todo/v1/{userId}`
   - Description: Adds a new todo item for the specified user.

- **POST** `/api/todo/v1/{userId}/completed`
   - Description: Marks the specified todo item as completed for the user.

### User Controller
- **POST** `/api/user/v1`
   - Description: Creates a new user.

- **GET** `/api/user/v1/{userId}`
   - Description: Retrieves information for the specified user by user ID.

## SQS Listener
The `SqsQueueListener` service is responsible for asynchronously processing messages from an Amazon SQS queue. It listens for incoming messages that indicate actions to be performed, such as creating a new user.

### Key Features:
- **Message Processing**: Listens for messages from the configured SQS queue and processes them based on their type.
- **User Creation**: When a message indicating a new user is received, it extracts the user information and stores it in the database.
- **Error Handling**: Logs any errors encountered during message processing and ensures that successfully processed messages are deleted from the queue.

## Contributing
Contributions are welcome! If you find a bug or want to suggest a feature, please open an issue or submit a pull request.
