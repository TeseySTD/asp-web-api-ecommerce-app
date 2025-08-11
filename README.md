# 🚀 Microservices Web API Project 
---
## 📋Overview 
This is a **PET** project — a set of ASP.NET Core web APIs built as microservices.  
The main goal of the project is to gain hands-on experience with modern architectural practices  (Clean Architecture, DDD, CQRS, event-driven communication, SAGA) and libraries or frameworks (EF Core, MediatR, MassTransit, FluentValidation, etc.) . Each service follows DDD principles and is designed to be independently developed, tested and deployed.

---

## 📁 Project Structure
```
Server/
│   ├── src/
│   │   ├── Services/
│   │   │   ├── Basket/
│   │   │   │   └── Basket.API/
│   │   │   ├── Catalog/
│   │   │   │   ├── Catalog.API/
│   │   │   │   ├── Catalog.Application/
│   │   │   │   ├── Catalog.Core/
│   │   │   │   └── Catalog.Persistence/
│   │   │   ├── Ordering/
│   │   │   │   ├── Ordering.API/
│   │   │   │   ├── Ordering.Application/
│   │   │   │   ├── Ordering.Core/
│   │   │   │   └── Ordering.Persistence/
│   │   │   ├── Users/
│   │   │   │   ├── Users.API/
│   │   │   │   ├── Users.Application/
│   │   │   │   ├── Users.Core/
│   │   │   │   ├── Users.Infrastructure/
│   │   │   │   └── Users.Persistence/
│   │   ├── Shared/
│   │   │   ├── Shared.Core/
│   │   │   └── Shared.Messaging/
│   │   ├── APIGateways/
│   │   │   └── YarpGateway/
│   │   ├── KeyManager/
│   ├── tests/
│   │   ├── Basket
│   │   │   ├── Basket.Tests.Unit/
│   │   │   └── Basket.Tests.Integration/
│   │   ├── Catalog
│   │   │   ├── Catalog.Tests.Unit/
│   │   │   └── Catalog.Tests.Integration/
│   │   ├── Ordering
│   │   │   ├── Ordering.Tests.Unit/
│   │   │   └── Ordering.Tests.Integration/
│   │   ├── Users
│   │   │   ├── Users.Tests.Unit/
│   │   │   └── Users.Tests.Integration/
│   │   ├── Shared
│   └── └── └── Shared.Core.Tests
├── docker-compose.yml
└── docker-compose.override.yml
```
### Services
#### 🛒 Basket Service
Basket is a lightweight microservice, that responsible for product cart actions. Stores minimal info about Product and Category in one ProductCart object as JSON, using MartenDb ORM.
Reacts to product/category events in Catalog service.
Because its responsibility is small, I decided to make it monolith one-project, but in it separates all contexts using Clean Architecture principle. By this reason I also used MartenDb ORM because of simplicity of business logic and lightweight service conception.
#### 📦 Catalog Service
Catalog is largest microservice in project, that is responsible for Product and Category actions. Stores all info about Product and Category aggregates in DB, using EF Core ORM. Also acts as an CDN server by storing Product/Category images in separate table `Images` as aggragate with its own actions and endpoints.
Reacts to Ordering ReserveProduct event.
#### 📋 Ordering Service
Ordering is microservice, that handles the order lifecycle. Stores all info about Orders and minimal info about Product aggregates in DB, using EF Core ORM. It also controles all stages of ordering, using Event-Driven approach and SAGA pattern.
Reacts to product events in Catalog service.
#### 👥 Users Service
Users is microservice, that responsible for all user and auth actions. Plays role of the authentication server, because it contains all necessary information about User aggregate. Stores all info about User in DB, using EF Core ORM.
Performs authentication and authorization using JWT token with RSA signing and Role-based hierarchy policies.
Here is hierarchy:
```
Admin->Seller->Default
```
Works with SMTP server and sends emails with verification link to users.
Reacts to CheckCustomer event in Ordering service.
#### 🔑 KeyManager
KeyManager is a simple console project, that has only one purpose - create public and private keys for Users service.
Create keys in Server/secrets folder.
#### 🌐 YARP Gateway
Gateway of all microservices. Has rate limmiter and SwaggerUI for development. Each microservice has its swagger Open Api scheme, Yarp Gateway service unite all this schemes in one, format their routes to gateway and enable developer to use one SwaggerUI for all microservices.
#### 🔧 Shared Libraries
Has two projects - Shared.Core and Shared.Messaging. 
Shared.Messaging contains only events class, shared DTOs and all data objects that are used for communication between services. It also has extention for adding MassTransit into api.
Shared.Core contains shared logic and base classes. It has Fluent Validation abstractions, Result pattern implementation and custom validators.
Shared.Core describes all DDD abstractions like Entity<> and AggregateRoot<>, adds MediatR abstractions for CQRS implementation, configurations for logging and validation pipelines, api pagination and envelope records and extension methods for shared authentication, authorization, swagger, etc.


