# CMS Fetch Service (Sync Engine)

## Project Overview
A synchronization service that receives CMS events via a webhook and exposes processed content to a Rest API. Built with .NET9 and SQLite.

## Key Architectural Decisions
* Incoming requests (events) are dealt with *Asynchronously* - by using Channels as in-memory queue. Reason: This allows for "quick landings" that are automatically accepted by the webhook, whilst preserving a sequence of events
* Records are persisted in a SQLite database file (which is automatically created in the application folder on first execution)

### Prerequisites
.NET 9 SDK

### Installation & Run
* git clone [https://github.com/jaimepn/CmsFetchService.git](https://github.com/jaimepn/CmsFetchService.git)
* dotnet run --project CmsFetchService.API
* (following this, API documentation will be available on: http://localhost:5281/scalar/v1 - with sample credentials and request objects)

### Known limitations
* Credentials are hardcoded, for demonstration only
* In-memory queuing system was used - which is not persisted and would be lost eg, on service crash
* Service not fully test-covered
