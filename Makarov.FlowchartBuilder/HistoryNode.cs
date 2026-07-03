using System.Collections.Generic;
using System.Linq;

namespace Makarov.FlowchartBuilder
{

    public sealed class HistoryNode
    {
        #region Exceptions

        public abstract class HistoryNodeException : History.HistoryException
        {

            protected HistoryNodeException(string message)
                : base(message)
            { }
        }

        public sealed class NextNodeNotExistsException : HistoryNodeException
        {
            public NextNodeNotExistsException()
                : base(@"Next node not exists.")
            { }
        }

        public sealed class PrevNodeNotExistsException : HistoryNodeException
        {
            public PrevNodeNotExistsException()
                : base(@"Previous node not exist.")
            { }
        }
        #endregion

        #region Private members

        private HistoryNode _prevNode;

        private HistoryNode _nextNode;

        private readonly List<HistoryEvent> _events;
        #endregion

        #region Properties

        public bool IsPrevNodeExists
        {
            get { return _prevNode != null; }
        }

        public bool IsNextNodeExists
        {
            get { return _nextNode != null; }
        }

        public HistoryNode NextNode
        {
            get
            {
                if (_nextNode == null)
                    throw new NextNodeNotExistsException();

                return _nextNode;
            }

            set { _nextNode = value; }
        }

        public HistoryNode PrevNode
        {
            get
            {
                if (_prevNode == null)
                    throw new PrevNodeNotExistsException();

                return _prevNode;
            }

            set { _prevNode = value; }
        }

        public IEnumerable<HistoryEvent> Events
        {
            get
            {

                foreach (HistoryEvent @event in _events)
                    if (@event is HistoryEventCreateGlyph)
                        yield return @event;

                foreach (HistoryEvent @event in _events)
                    if (@event is HistoryEventChangeGlyph)
                        yield return @event;

                foreach (HistoryEvent @event in _events)
                    if (@event is HistoryEventDeleteGlyph)
                        yield return @event;

                foreach (HistoryEvent @event in _events)
                    if (!(@event is HistoryEventCreateGlyph ||
                          @event is HistoryEventChangeGlyph ||
                          @event is HistoryEventDeleteGlyph))
                        yield return @event;
            }
        }

        public IEnumerable<HistoryEvent> EventsReverse
        {
            get { return Events.Reverse(); }
        }

        public int EventsCount
        {
            get { return _events.Count; }
        }
        #endregion

        #region Constructors

        public HistoryNode(IEnumerable<HistoryEvent> historyEvents)
        {
            _events = new List<HistoryEvent>(historyEvents);
        }

        public HistoryNode(HistoryEvent historyEvent)
        {
            _events = new List<HistoryEvent> {historyEvent};
        }
        #endregion
    }
}
