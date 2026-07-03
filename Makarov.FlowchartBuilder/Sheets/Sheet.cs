using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Makarov.FlowchartBuilder.Commands;
using Makarov.FlowchartBuilder.Forms;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Sheets
{

    public abstract partial class Sheet: ICloneable
    {
        #region Exceptions

        public class SheetException : FlowchartBuilderException
        {

            public SheetException(string message)
                : base(message)
            { }
        }

        public sealed class InvalidSheetSizeException : InvalidValueException
        {

            public InvalidSheetSizeException(float width, float height)
                : base("Sheet", string.Format("[{0}, {1}]", width, height))
            { }
        }

        public sealed class UnknownNameException : SheetException
        {

            public UnknownNameException(Type type)
                : base(string.Format(@"Unknown sheet '{0}' name.", type.Name))
            { }
        }

        public sealed class UnknownFamilyException : SheetException
        {

            public UnknownFamilyException(Type type)
                : base(string.Format(@"Unknown sheet '{0}' family.", type.Name))
            { }
        }
        #endregion

        #region Constructors

        protected Sheet()
        {

            Groups = new List<List<BlockGlyph>>();

            Command.GetInstance("ExportImageCommand").Enabled = true;
        }
        #endregion

        #region Private members

        private float _scaleFactor = 0.5f;

        private readonly List<AbstractGlyph> _glyphs = new List<AbstractGlyph>();

        protected Point _oldPt;

        protected Point? _selectionStart;

        protected Point _selectionEnd;

        protected BlockGlyph _connectorSource;

        protected int _connectorSourcePort;

        protected Point? _connectorCurrent;
        #endregion

        #region Protected methods

        public void UpdateGlyphsCommands()
        {

            bool groupsFinded = false;
            foreach (var glyph in SelectedGlyphs)
                if (glyph is BlockGlyph)
                    if (((BlockGlyph) glyph).InGroup)
                    {
                        Command.GetInstance("UngroupCommand").Enabled = true;
                        groupsFinded = true;
                        break;
                    }

            if (!groupsFinded)
                Command.GetInstance("UngroupCommand").Enabled = false;

            if (SelectedGlyphsExists)
            {

                List<BlockGlyph> currGroup = null;

                bool freeGlyphFounded = false;

                foreach (var glyph in SelectedGlyphs)
                    if (glyph is BlockGlyph)
                    {
                        var bg = (BlockGlyph) glyph;

                        if (freeGlyphFounded)
                        {
                            Command.GetInstance("GroupCommand").Enabled = true;
                            break;
                        }

                        if (bg.InGroup)
                        {
                            if (currGroup == null)
                            {
                                currGroup = bg.Group;
                            }
                            else if (currGroup != bg.Group)
                            {
                                Command.GetInstance("GroupCommand").Enabled = true;
                                break;
                            }
                        }
                        else
                        {
                            freeGlyphFounded = true;

                            if (currGroup != null)
                            {
                                Command.GetInstance("GroupCommand").Enabled = true;
                                break;
                            }
                        }
                    }
            }
            else
                Command.GetInstance("GroupCommand").Enabled = false;

            if (SelectedGlyphsExists)
            {

                Command.GetInstance("DeleteCommand").Enabled = true;

                Command.GetInstance("CutCommand").Enabled = true;

                Command.GetInstance("CopyCommand").Enabled = true;

                Command.GetInstance("FixSelectedCommand").Enabled = true;

                Command.GetInstance("DeselectCommand").Enabled = true;
            }
            else
            {

                Command.GetInstance("DeleteCommand").Enabled = false;

                Command.GetInstance("CutCommand").Enabled = false;

                Command.GetInstance("CopyCommand").Enabled = false;

                Command.GetInstance("FixSelectedCommand").Enabled = false;

                Command.GetInstance("DeselectCommand").Enabled = false;
            }

            Command.GetInstance("FixAllCommand").Enabled = GlyphsExists;

            Command.GetInstance("UnfixAllCommand").Enabled = FixedGlyphsExists;
        }
        #endregion
    }
}
