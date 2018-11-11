![./images/logo-large.png](./images/logo-large.png)

# Simple Event Sourcing

[![NuGet](https://img.shields.io/nuget/v/Simple.EventSourcing.svg)](https://www.nuget.org/packages/Simple.EventSourcing/)

> Event Sourcing made simple using Reactive Extensions

Simple Event Sourcing is a .NET library based on the Event Sourcing architecture. This library is written with Rx.NET and built with the *minimum of code* you need to create event sourced application.

## Example app

There is a sample application in this repository. It is a console application that showcases how you can use this library in your backend application. The console application focuses on:

* A list of events on a concrete domain (e-commerce cart)
* An in-memory aggregator (building a State in-memory)
* A relational database aggregator (using sqlite database system)

## Getting started

Simple Event Sourcing library is defined by 5 different components:

* Commands - a command is an action coming from the top-layer of your architecture (from an HTTP method for example)
* Command Dispatcher - convert a command (user action or system action) from into a list of events 
* Events - a list of actions that happened in the system (ex: user ordered a book)
* Event Store - the recording of events & the single source of truth (events stored in-memory or in a database)
* Event View - a point of view extracted from the events (ex: total sales per user)

### Commands

TODO

### Dispatcher of commands

TODO

### Events

Events is your own definition of the business actions. Using this library, you have to express each event as `class` and we take care of the rest. Here are some examples of events:

```csharp
public class ItemAddedInCart
{
    public string ItemName { get; set; }
    public int NumberOfUnits { get; set; } = 1;
}

public class ItemRemovedFromCart
{
    public string ItemName { get; set; }
}

public class CartReseted { }
```

### Write Model - Event Store

Using Event Sourcing, the Write model consists of a set of events which are stored in the commonly named Event Store.

The `EventStore` class should be redefine for your purpose. By default, the behavior of the `EventStore` is to store events in-memory.

```csharp
public abstract class EventStore<TEvent> 
    where TEvent : SimpleEvent
{
    protected EventStore(IObservable<IEnumerable<TEvent>> eventAggregates) { }

    public void Push(IEnumerable<TEvent> events);

    public IObservable<TEvent> ObserveEvent();
    public IObservable<TEvent> ObserveEvent<TEventType>();
}
```

If you need to make a persistent `EventStore`, we offer a method called `Persist` you can use to store events in a database system.

### Read Model - Event View

Using Event Sourcing, the Read model consists of reading a set of events from the Event Store to get a consistent view of the business.

Using this library, you will have the freedom to choose between two kind of views:

1. In-memory view - listening to events will generate a view used instantly
2. Persistent view - listening to events will create data stored in a database system

#### In-memory view

```csharp
public abstract class InMemoryEventView<TEvent, TState>
    where TEvent : SimpleEvent
    where TState : class, new()
{
    public TState State { get; }

    protected InMemoryEventView(IObservable<TEvent> events, TState initialState = null) { }

    public IObservable<TState> ObserveState();
    public IObservable<TPartial> ObserveState<TPartial>(Func<TState, TPartial> selector);
}
```

In this particular form of `EventView`, from an event you get a new state immediately available for consumption/subscription. This form of `EventView` is close to the Redux pattern.

#### Persistent view

This other form of `EventView` gives you the ability to override the `Handle` method in which you can update a specific part of your data like a Table in a relational database.

```csharp
public abstract class EventView<TEvent>
    where TEvent : SimpleEvent
{
    protected EventView(IObservable<TEvent> events) { }

    public virtual void Replay(TEvent @event);
    public virtual void Replay(IEnumerable<TEvent> events);
}
```

Notice that we offer a `Replay` mechanism you can use to replay the history in case you changed the logic of the handler or the structure of the database.
