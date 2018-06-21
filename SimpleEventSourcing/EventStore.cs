using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace SimpleEventSourcing
{
    public abstract class EventStore
    {
        private readonly Subject<object> _eventSubject = new Subject<object>();

        public virtual void Dispatch(object @event)
        {
            _eventSubject.OnNext(@event);
        }

        public IObservable<object> ObserveEvent()
        {
            return _eventSubject;
        }
        public IObservable<T> ObserveEvent<T>() where T : class
        {
            return _eventSubject.OfType<T>();
        }
    }
}
