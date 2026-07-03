using System;
using System.Collections.Generic;
using System.Linq;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Sheets;

namespace Makarov.FlowchartBuilder
{

    public sealed partial class Document
    {
        #region Constructors

        public Document(Sheet sheet)
        {

            DocumentSheet = sheet;

            var historyEvents = new List<HistoryEvent>();

            foreach (AbstractGlyph glyph in sheet.Glyphs)
            {
                var snapshot = new GlyphSnapshot(glyph);
                historyEvents.Add(new HistoryEventCreateGlyph(snapshot));
            }

            DocumentHistory = new History(historyEvents);

            SaveSnapshots(GetSnapshotsFromSheet());
        }

        public Document()
        {
            throw new NotImplementedException();
        }

        public Document(string fileName)
        {
            using (new DonNotRedraw())
                Load(fileName);
        }
        #endregion

        #region Private members

        private readonly List<GlyphSnapshot> _snapshots = new List<GlyphSnapshot>();
        #endregion

        #region Properties

        public bool Saved
        {
            get { return CompareSnapshots(GetSnapshotsFromSheet()); }
        }

        public Sheet DocumentSheet { get; private set; }

        public History DocumentHistory { get; private set; }

        public string Name { get; set; }
        #endregion

        #region Private methods

        private IEnumerable<GlyphSnapshot> GetSnapshotsFromSheet()
        {
            return DocumentSheet.Glyphs.Select(glyph => new GlyphSnapshot(glyph));
        }

        private void SaveSnapshots(IEnumerable<GlyphSnapshot> snapshots)
        {
            _snapshots.Clear();

            foreach (GlyphSnapshot snapshot in snapshots)
                _snapshots.Add(snapshot);
        }

        private bool CompareSnapshots(IEnumerable<GlyphSnapshot> snapshots)
        {

            var sshots = new List<GlyphSnapshot>(snapshots);

            if (sshots.Count != _snapshots.Count)
                return false;

            foreach (GlyphSnapshot currSshot in _snapshots)
            {
                GlyphSnapshotsDiff diff = null;

                foreach (GlyphSnapshot sshot in sshots)
                    if (sshot.GlyphId == currSshot.GlyphId &&
                        sshot.GlyphType == currSshot.GlyphType)
                    {

                        diff = new GlyphSnapshotsDiff(currSshot, sshot);
                    }

                if (diff == null)
                    return false;

                if (diff.Count > 0)
                    return false;
            }

            return true;
        }
        #endregion

        #region Public methods

        public void UpdateHistory()
        {

        }

        public void HistoryBack()
        {

        }

        public void HistoryForward()
        {

        }
        #endregion
    }
}
