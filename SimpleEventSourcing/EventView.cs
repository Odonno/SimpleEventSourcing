using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace SimpleEventSourcing
{
    public abstract class EventView<TState> where TState : class, new()
    {
        private readonly Subject<TState> _stateSubject = new Subject<TState>();

        public TState State { get; private set; }

        protected EventView(IObservable<object> events, TState initialState = null)
        {
            State = initialState ?? new TState();

            events
                .Subscribe(@event =>
                {
                    State = Execute(State, @event);
                    _stateSubject.OnNext(State);
                });
        }

        public IObservable<TState> ObserveState()
        {
            return _stateSubject.DistinctUntilChanged();
        }

        protected abstract TState Execute(TState state, object @event);
    }
}
