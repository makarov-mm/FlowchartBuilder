using System.Collections.Generic;

namespace Makarov.FlowchartBuilder
{

    public sealed class History
    {
        #region Exceptions

        public abstract class HistoryException : FlowchartBuilderException
        {

            protected HistoryException(string message)
                : base(message)
            { }
        }

        public sealed class CurrentHistoryNodeNotExistsException : HistoryException
        {
            public CurrentHistoryNodeNotExistsException()
                : base(@"Current history node not exists.")
            { }
        }

        public sealed class NextHistoryNodeNotExistsException : HistoryException
        {
            public NextHistoryNodeNotExistsException()
                : base(@"Next history node not exists.")
            { }
        }

        public sealed class PrevHistoryNodeNotExistsException : HistoryException
        {
            public PrevHistoryNodeNotExistsException()
                : base(@"Previous history node not exists.")
            { }
        }
        #endregion

        #region Private members

        private HistoryNode _node;

        #endregion

        #region Constructors

        public History(IEnumerable<HistoryEvent> historyEvents)
        {
            _node = new HistoryNode(historyEvents);
        }

        public History(HistoryEvent historyEvent)
        {
            _node = new HistoryNode(historyEvent);
        }

        public History()
        {
            _node = null;
        }
        #endregion

        #region Properties

        public bool IsCurrentExists
        {
            get { return _node != null; }
        }

        public HistoryNode Current
        {
            get
            {
                if (!IsCurrentExists)
                    throw new CurrentHistoryNodeNotExistsException();

                return _node;
            }
        }

        public bool PrevExists
        {
            get { return IsCurrentExists && Current.IsPrevNodeExists; }
        }

        public bool NextExists
        {
            get { return IsCurrentExists && Current.IsNextNodeExists; }
        }

        public void Redo()
        {
            if (!NextExists)
                throw new NextHistoryNodeNotExistsException();

            _node = _node.NextNode;
        }

        public void Undo()
        {
            if (!PrevExists)
                throw new PrevHistoryNodeNotExistsException();

            _node = _node.PrevNode;
        }
        #endregion

        #region Public methods

        public void Add(IEnumerable<HistoryEvent> historyEvents)
        {
            if (_node == null)
                _node = new HistoryNode(historyEvents);
            else
            {
                var tmp = new HistoryNode(historyEvents) {PrevNode = _node};
                _node.NextNode = tmp;
                _node = tmp;
            }
        }

        public void Add(HistoryEvent historyEvent)
        {
            Add(new List<HistoryEvent> {historyEvent});
        }
        #endregion
    }
}
