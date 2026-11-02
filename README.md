# BasaltMoth — C#/.NET Code Samples

A collection of C#/.NET code samples demonstrating backend development, domain modeling, database operations, algorithm design, and automated testing.

The samples are based on technical challenges encountered while developing **BasaltMoth**, an independent full-stack SaaS application for managing quotations, orders, and production workflows in the manufacturing industry.

BasaltMoth is developed using **ASP.NET Core (.NET 10), Entity Framework Core, PostgreSQL, React, and TypeScript**.

The original application is developed in consultation with experienced industry professionals, whose feedback helps shape features and improve existing workflows.

## Code Samples

### 1. Order Lifecycle — Domain Modeling

Demonstrates object-oriented design and business rule enforcement through a domain entity that manages order state transitions.

**Key concepts:**
- Encapsulation and controlled state changes
- Domain validation and business rules
- State transition management
- Unit testing of valid and invalid operations

### 2. Quote Number Generator — Database Operations

Demonstrates generating sequential quotation numbers using Entity Framework Core and parameterized SQL.

**Key concepts:**
- EF Core database integration
- Asynchronous database operations
- SQL parameterization
- Transaction management and concurrent operations
- Integration testing

The demonstration uses SQLite for local execution. The original BasaltMoth implementation uses PostgreSQL.

### 3. Cutting Optimization — Algorithms

Demonstrates a deterministic best-fit decreasing algorithm for assigning pieces to fixed-length stock bars while accounting for cutting waste.

**Key concepts:**
- Algorithm design and problem solving
- Collections, sorting, and LINQ
- Input validation
- Deterministic behavior
- Unit testing and edge cases

The algorithm uses a heuristic approach and does not guarantee a globally optimal result.

## Technologies

- C# / .NET 10
- Entity Framework Core
- SQLite (demonstration database)
- SQL
- xUnit
- LINQ
- Object-Oriented Programming

## Getting Started

**Requirements:** .NET 10 SDK

Clone the repository:

```bash
git clone  https://github.com/Piotr-Czechowski-It/BasaltMoth-Code-Samples.git
cd BasaltMoth-Code-Samples
```

Build the solution:

```bash
dotnet build BasaltMoth-Code-Samples.slnx
```

Run the tests:

```bash
dotnet test BasaltMoth-Code-Samples.slnx
```

## About BasaltMoth

BasaltMoth is an independently developed full-stack application designed to support manufacturing companies in managing their daily operations.

The application includes:

- Customer quotation and pricing workflows
- Order and production management
- Responsive user interfaces built with React
- Automated production drawing generation
- Material cutting optimization
- REST APIs and PostgreSQL persistence
- Docker-based deployment on Linux

The project is continuously improved based on feedback from experienced professionals working in the manufacturing industry.

## Project Scope

This repository contains **simplified, standalone implementations** of selected technical concepts from BasaltMoth.

These examples are intended to demonstrate programming practices and problem-solving approaches. They are not verbatim extracts of the private application's source code.

The complete BasaltMoth application and its production-specific business logic remain in a private repository.
