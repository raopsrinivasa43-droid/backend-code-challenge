# Backend Developer Code Challenge

## Introduction
Welcome to the Backend Developer Technical Assessment! This test is designed to evaluate your proficiency in building REST APIs using .NET 8, focusing on clean architecture, business logic, and testing practices. We have prepared a set of tasks and questions that cover a spectrum of skills, ranging from fundamental concepts to more advanced topics.

**Note:** This assessment focuses on API development, architecture, and testing. During the interview, we'll discuss your experience with databases, event-driven design, Docker/Kubernetes, and cloud platforms.

## Tasks
Complete the provided tasks to demonstrate your ability to work with .NET 8, ASP.NET Core Web API, and unit testing. Adjust the complexity based on your experience level.

## Questions
Answer the questions to showcase your understanding of the underlying concepts and best practices associated with the technologies in use.

## Time Limit
This assessment is designed to take approximately 1-2 hours to complete. Please manage your time effectively.

## Setup the Repository
Make sure you have .NET 8 SDK installed
- Install dependencies with `dotnet restore`
- Build the project with `dotnet build`
- Run the project with `dotnet run --project CodeChallenge.Api`
- Navigate to `https://localhost:5095/swagger` to see the API documentation

## Prerequisite
Start the test by forking this repository, and complete the following tasks:

---

## Task 1
**Assignment:** Implement a REST API with CRUD operations for messages. Use the provided `IMessageRepository` and models to create a `MessagesController` with these endpoints:
- `GET /api/v1/organizations/{organizationId}/messages` - Get all messages for an organization
- `GET /api/v1/organizations/{organizationId}/messages/{id}` - Get a specific message
- `POST /api/v1/organizations/{organizationId}/messages` - Create a new message
- `PUT /api/v1/organizations/{organizationId}/messages/{id}` - Update a message
- `DELETE /api/v1/organizations/{organizationId}/messages/{id}` - Delete a message

**Question 1:** Describe your implementation approach and the key decisions you made.

**Question 2:** What would you improve or change if you had more time?

commit the code as task-1

---

## Task 1 Reflection

**Question 1:** Describe your implementation approach and the key decisions you made.

I implemented the MessagesController by defining five endpoints as specified in the assignment, 
each corresponding to a CRUD operation for messages. For data access, I used methods directly 
from the provided IMessageRepository interface, keeping the controller logic clean and straightforward. 
For each action, I handled null checks and returned appropriate status codes (200 OK, 201 Created, 204 No Content, 404 Not Found) 
following RESTful best practices. When creating a message, 
I ensured required properties (OrganizationId, Title, Content, CreatedAt, IsActive) were set, 
and all IDs used were GUIDs as specified. Testing was done using Swagger to verify endpoint behavior.



**Question 2:** What would you improve or change if you had more time?

With more time, I would add model validation and error handling to improve robustness. 
I would also implement logging for errors and important events for better monitoring, 
and write unit tests to verify each endpoint’s behavior. Additionally, 
I’d consider implementing pagination for the GetAll endpoint and possibly customizing 
response messages for improved API usability.

---

## Task 2
**Assignment:** Separate business logic from the controller and add proper validation.
1. Implement `MessageLogic` class (implement `IMessageLogic`)
2. Implement Business Rules:
   - Title must be unique per organization
   - Content must be between 10 and 1000 characters
   - Title is required and must be between 3 and 200 characters
   - Can only update or delete messages that are active (`IsActive = true`)
   - UpdatedAt should be set automatically on updates
3. Return appropriate result types (see `Logic/Results.cs`)
4. Update Controller to use `IMessageLogic` instead of directly using the repository

**Question 3:** How did you approach the validation requirements and why?

**Question 4:** What changes would you make to this implementation for a production environment?

commit the code as task-2

---

## Task 2 Reflection

**Question 3:** How did you approach the validation requirements and why?

I approached the validation requirements by centralizing all business rule and 
input validation logic within the MessageLogic class, separate from the controller and 
repository layers. This ensures consistency for rules such as title presence and length, 
uniqueness per organization, required content, and validation of message state before updates or deletions. 
By consolidating validation in a single component, the controllers remain clean and focused on request handling, 
while business rules are easy to maintain, test, and extend. This design aligns with best practices 
for separation of concerns, promotes code reuse, and enables comprehensive error handling and 
reporting for client applications.


**Question 4:** What changes would you make to this implementation for a production environment?

For a production environment, I would replace the in-memory data storage with a persistent 
database solution such as SQL Server, accessed through an ORM like Entity Framework Core to 
ensure data reliability and scalability. The repository registration would be changed from 
singleton to scoped, following industry standards for thread safety in web applications. 
Additional improvements would include implementing comprehensive error handling, 
logging, authentication, and authorization to protect sensitive data and monitor application health. 
I would also introduce automated and integration testing, use environment-specific application settings, 
enforce API versioning, and document all endpoints using OpenAPI to facilitate client integration and 
maintainability.

---

## Task 3
**Assignment:** Write comprehensive unit tests for your business logic.
1. Create `CodeChallenge.Tests` project (xUnit)
2. Add required packages: xUnit, Moq, FluentAssertions
3. Write Tests for MessageLogic covering these scenarios:
   - Test successful creation of a message
   - Test duplicate title returns Conflict
   - Test invalid content length returns ValidationError
   - Test update of non-existent message returns NotFound
   - Test update of inactive message returns ValidationError
   - Test delete of non-existent message returns NotFound

**Question 5:** Explain your testing strategy and the tools you chose.

**Question 6:** What other scenarios would you test in a real-world application?

commit the code as task-3

---

## Task 3 Reflection



Testing:
This solution contains a separate test project called CodeChallenge.
Tests that focuses on the business logic in MessageLogic.

Covered scenarios--
The MessageLogicTests class includes unit tests for:
	1)Successful creation of a message with valid title and content.
	2)Creation with a duplicate title for an active message returns Conflict.
	3)Creation with invalid content length returns ValidationError.
	4)Updating a non‑existent message returns NotFound.
	5)Updating an inactive message returns Conflict (only active messages can be updated).
	6)Deleting a non‑existent message returns NotFound.


**Question 5:** Explain your testing strategy and the tools you chose.

The testing strategy was to unit test the business logic in isolation, 
without touching the real database or ASP.NET pipeline. xUnit was used as the test framework 
to define and run tests, Moq was used to mock IMessageRepository so the tests control the data returned, 
and FluentAssertions was used to write readable, 
expressive assertions like `result.Should().BeOfType<Created<Message>>()`. 
Each test follows the Arrange–Act–Assert pattern: first setting up repository behavior and input data, 
then calling the MessageLogic method, and finally asserting that the correct 
Result type (Created, Conflict, ValidationError, NotFound) and repository interactions happen.​
The focus was on the key business rules: successful creation with valid data, 
duplicate title detection, content length validation, behavior when updating or deleting missing messages, 
and rules around inactive messages. By mocking the repository, the tests stay fast and deterministic 
and verify only the logic in MessageLogic, not infrastructure concerns.



**Question 6:** What other scenarios would you test in a real-world application?

In a real‑world application, more scenarios around messages would be tested, 
not just the ones required in the assignment. Examples include boundary values for validation, 
such as titles and content exactly at the minimum and maximum allowed lengths, 
and ensuring that inactive messages cannot be created with duplicate titles that 
conflict with other inactive records if the rules change. 
Tests would also cover successful update and delete flows, concurrency or race conditions 
around duplicate titles, audit fields like CreatedAt and UpdatedAt, and error handling 
for unexpected repository failures or exceptions.​
Integration tests would likely be added on top of these unit tests to verify that the API controllers, 
model binding, validation attributes, and persistence layer all work together correctly when the 
application is running end‑to‑end.