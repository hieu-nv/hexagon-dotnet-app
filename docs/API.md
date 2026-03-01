# API Documentation (v1)

This document provides detailed information about the endpoints available in the Hexagon .NET API.

## Base URL
All requests are prefixed with `/api/v1`.

## Todo Endpoints

### Get All Todos
`GET /todos`
- **Response**: `200 OK`
- **Body**: Array of `TodoResponse` DTOs.

### Create Todo
`POST /todos`
- **Request Body**: `CreateTodoRequest`
- **Response**: `201 Created`
- **Body**: Created `TodoResponse`.

### Get Todo by ID
`GET /todos/{id}`
- **Response**: `200 OK`
- **Body**: `TodoResponse`.
- **Errors**: `404 Not Found` if the ID does not exist.

### Update Todo
`PUT /todos/{id}`
- **Request Body**: `UpdateTodoRequest`
- **Response**: `204 No Content`
- **Errors**: `404 Not Found` or `400 Bad Request` (Validation errors).

### Delete Todo
`DELETE /todos/{id}`
- **Response**: `204 No Content`
- **Errors**: `404 Not Found`.

### Get Filtered Todos
`GET /todos/completed` or `GET /todos/incomplete`

---

## Pokemon Endpoints

### Get Pokemon List
`GET /pokemon`
- **Query Parameters**:
  - `limit` (default: 20)
  - `offset` (default: 0)
- **Response**: `200 OK`
- **Body**: `PokemonListResponse`.

### Get Pokemon by ID
`GET /pokemon/{id}`
- **Response**: `200 OK`
- **Body**: `PokemonResponse`.
- **Errors**: `404 Not Found`.

---

## Infrastructure

### Health Checks
- `GET /health`: Readiness check including database connectivity.
- `GET /alive`: Liveness check for the container/service.

## Error Handling
The API follows **RFC 7807 (Problem Details for HTTP APIs)**. Error responses include:
- `type`: URI reference naming the problem type.
- `title`: Short summary of the problem.
- `status`: HTTP status code.
- `detail`: Detailed explanation.
- `traceId`: Correlation ID for log correlation.
