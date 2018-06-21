using System.Reactive.Subjects;

namespace SimpleEventSourcing
{
    public abstract class EventView<TState> where TState : class, new()
    {
        private readonly Subject<TState> _stateSubject = new Subject<TState>();

        public TState State { get; private set; }

        protected EventView(TState initialState = null)
        {
            State = initialState ?? new TState();
        }

        public void Handle(object @event)
        {
            State = Execute(State, @event);
            _stateSubject.OnNext(State);
        }

        public abstract TState Execute(TState state, object @event);
    }
}
