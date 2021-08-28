# EventStoreBank

## Intro
I did this projet to learn about [EventStoreDb](https://www.eventstore.com/), [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html), and [Command and Query Responsibility Segregation](https://martinfowler.com/bliki/CQRS.html). 

The domain is a simple bank account with the possibility to Create, Delete, Deposit and Withdraw.

Optimistic concurrency is included so two clients should not be able to cause truble by editing the same account at the same time.

A checkpoint for ReadModelUpdater has been implementet so it can be closed while the clients can keep working and then started again.

This diagram shows the current state of the project:

![EventStoreBank](/Diagrams/EventStoreBank.svg)


## Known issues / Planned improvements

* Commands in the current system are really SQL commands and not system commands ( compare the two diagrams).
* Implement an API so the client does not have to know how to send events (see the diagram below).
* Implement a wait function to reduce eventual concistensy problems as [suggestet by Greg Young](https://youtu.be/FKFu78ZEIi8?t=1771).
* Clean up redundant code forinstance in the EventHandlers class.
* Implement Transactions to other accounts.
* Look into supplying CancelationTokens with DI.
* Clean up DI to use it more often, close to every time an object is created.
* Use DTOs instead of db models for data transfer. :)
* Unit tests.

This diagram shows a possible future state of the project:

![EventStoreBank](/Diagrams/EventStoreBankWithAPI.svg)

## How to run

###  Infrastructure
To get EventStoreDB and MSSQL server up and running.

Navigate to the /docker folder and run **docker-compose up**

### Database update
In visual studio set the model project as startup project.

In Package Manager Console run the command **Update-Database**

### Running the code
Start the ReadModelUpdater
Start a few clients and start sending events.
