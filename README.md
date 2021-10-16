# EventStoreBank

## Intro
I did this projet to learn about [EventStoreDb](https://www.eventstore.com/), [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html), and [Command and Query Responsibility Segregation](https://martinfowler.com/bliki/CQRS.html). 

The domain is a simple bank account with the possibility to Create, Delete, Deposit and Withdraw.

Optimistic concurrency is included so two clients should not be able to cause truble by editing the same account at the same time.

A checkpoint for ReadModelUpdater has been implementet so it can be closed while the clients can keep working and then started again.

I recently read about the new minimal api in .net 6 and decided to try it out for this project.



This diagram shows a high level overview of the project:

![EventStoreBank](/Diagrams/EventStoreBankWithAPI.svg)



## Known issues / Planned improvements

* Commands in the current system are really SQL commands and not system commands.
* Add validation to API, for instance check if account exists.
* Add general errorhandeling to request pipeline.
* Implement a react frontend.
* Implement a blazor frontend look into [mudBlazor](https://mudblazor.com/) and [havit](https://havit.blazor.eu/)
* Implement a wait function to reduce eventual concistensy problems as [suggestet by Greg Young](https://youtu.be/FKFu78ZEIi8?t=1771).
* Implement Transactions to other accounts.
* Figure out how CancelationTokens should be supplied from minimal api.
* Look into supplying CancelationTokens with DI.
* Use DTOs instead of db models for data transfer. :)
* Unit tests.
* Logging of errors and more
* Add swagger Schemas


## How to run

###  Infrastructure
To get EventStoreDB and MSSQL server up and running.

Navigate to the /docker folder and run **docker-compose up**

### Database update
In visual studio set the model project as startup project.

In Package Manager Console run the command **Update-Database**

### Running the code
Start the ReadModelUpdater
Start a few clients or api and start sending events.
