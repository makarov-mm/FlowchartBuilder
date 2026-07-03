using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Makarov.FlowchartBuilder.Forms;
using Makarov.Framework.Core;
using Makarov.Framework.Graphics;
using BaseSettings = Makarov.Framework.Core.Settings;

namespace Makarov.FlowchartBuilder
{

    public sealed partial class Core : Singleton<Core>, IDisposable
    {
        #region Constructors
        private Core()
        {

            if (CachedSettingsStorage.KeyExists(Settings.RegistryKeys.FirstStart))
                IsFirstRun = false;
            else
            {
                CachedSettingsStorage[Settings.RegistryKeys.FirstStart] = DateTime.Now.ToString();
                IsFirstRun = true;
            }

            CachedSettingsStorage[Settings.RegistryKeys.LastStart] = DateTime.Now.ToString();

            NeedRedraw = true;
        }
        #endregion

        #region Public methods

        public void Run(IEnumerable<string> args)
        {

            foreach (string arg in args)
            {
                if (!string.IsNullOrEmpty(arg))
                {
                    AddDocument(new Document(arg));

                    Settings.Environment.LoadedDocumentName = arg;
                }
            }

            Application.Run(MainWindow);
        }

        public void Dispose()
        {
            if (_disposed)
                throw new CoreAlreadyDisposedException();

            if (_windowsManager != null)
            {
                _windowsManager.Dispose();
                _windowsManager = null;
            }

            if (_graphicsManager != null)
            {
                _graphicsManager.Clear();
                _graphicsManager = null;

            }

            if (_iconsManager != null)
            {
                _iconsManager.Clear();
                _iconsManager = null;

            }

            if (_brushes != null)
            {
                _brushes.Dispose();
                _brushes = null;
            }

            if (_pens != null)
            {
                _pens.Dispose();
                _pens = null;
            }

            _disposed = true;
        }

        public T GetWindowAsChild<T>()
            where T : Form
        {
            if (typeof(T) == typeof(MainForm))
                throw new CantUseMainWindowAsChildException();

            var window = Windows.GetWindow<T>();

            if (window.Owner != MainWindow) window.Owner = MainWindow;

            return window;
        }

        public void SetDefaultWindowsPos()
        {

            using (new DonNotRedraw())
            {

                MainForm mainWindow = MainWindow;

                var glyphsWindow = GetWindowAsChild<GlyphsForm>();
                glyphsWindow.Visible = true;
                glyphsWindow.Left = mainWindow.WorkRectangle.Left;
                glyphsWindow.Top = mainWindow.WorkRectangle.Top;
                glyphsWindow.Height = mainWindow.WorkRectangle.Height;

                var propertiesWindow = GetWindowAsChild<PropertiesForm>();
                propertiesWindow.Visible = true;
                propertiesWindow.Left = mainWindow.WorkRectangle.Left +
                                        (mainWindow.WorkRectangle.Width - propertiesWindow.Width);
                propertiesWindow.Top = mainWindow.WorkRectangle.Top;
                propertiesWindow.Height = mainWindow.WorkRectangle.Height >> 1;
            }

            Redraw();
        }

        public void Redraw()
        {

            if (!NeedRedraw) return;

            MainForm wndMain = MainWindow;

            if (wndMain.ClientSize.Width <= 0 || wndMain.ClientSize.Height <= 0)
                return;

            Point scrollbarsOffset = UpdateScrollbars(wndMain);

            using (var tmp = new Bitmap(wndMain.ClientSize.Width, wndMain.ClientSize.Height))
            {
                using (Graphics gtmp = Graphics.FromImage(tmp))
                {

                    if (BeginRedraw != null)
                        BeginRedraw(gtmp, tmp.Width, tmp.Height);

                    gtmp.Clear(Settings.Colors.WorkArea);

                    Rectangle clientRect = wndMain.InnerRectangle;

                    int leftOffset = 0;
                    int topOffset = 0;

                    if (IsDocumentsExists)
                        using (Bitmap sheet = CurrentDocument.DocumentSheet.Draw())
                        {
                            leftOffset = clientRect.Left + ((clientRect.Width - sheet.Width) >> 1) - scrollbarsOffset.X;
                            topOffset = clientRect.Top + ((clientRect.Height - sheet.Height) >> 1) - scrollbarsOffset.Y;

                            const int shadowSize = 3;
                            gtmp.FillRectangle(Brushes.Black, leftOffset + sheet.Width, topOffset + shadowSize,
                                               shadowSize, sheet.Height);
                            gtmp.FillRectangle(Brushes.Black, leftOffset + shadowSize, topOffset + sheet.Height,
                                               sheet.Width - shadowSize, shadowSize);

                            if (Settings.Environment.CanUseNative)
                            {
                                using (Graphics sourceGfx = Graphics.FromImage(sheet))
                                {
                                    Gdi.BitBlt(
                                        sheet, sourceGfx,
                                        0, 0,
                                        gtmp,
                                        leftOffset, topOffset,
                                        sheet.Width, sheet.Height);
                                }
                            }
                            else
                            {
                                gtmp.DrawImageUnscaled(sheet, leftOffset, topOffset);
                            }

                            gtmp.DrawRectangle(
                                Settings.Pens.SheetBorder,
                                leftOffset - 1,
                                topOffset - 1,
                                sheet.Width + 1,
                                sheet.Height + 1);
                        }

                    DrawRulers(gtmp, clientRect, leftOffset, topOffset);

                    if (EndRedraw != null)
                        EndRedraw(gtmp, tmp.Width, tmp.Height);

                    gtmp.Flush();

                    using (Graphics gfx = wndMain.CreateGraphics())
                        if (Settings.Environment.CanUseNative)
                        {
                            Gdi.BitBlt(tmp, gtmp, 0, 0, gfx, 0, 0, tmp.Width, tmp.Height);
                        }
                        else
                        {
                            gfx.DrawImageUnscaled(tmp, 0, 0);
                        }
                }
            }
        }

        public void AddDocument(Document doc)
        {

            _documents.Insert(0, doc);

            if (DocumentAdded != null)
                DocumentAdded(doc);

            MainWindow.DocumentsTabs.AddTab(doc.Name ?? "Document", _documents.Count - 1);
            _currDocIndex = MainWindow.DocumentsTabs.SelectedTab.Id;
        }

        public void SelectDocument(int idx)
        {
            bool idxChanged = false;

            if (!IsDocumentsExists)
                throw new CurrentDocumentNotExistsException();

            if (idx < 0 || idx >= _documents.Count)
                throw new CurrentDocumentNotExistsException();

            if (_currDocIndex != idx)
                idxChanged = true;

            _currDocIndex = idx;

            if (idxChanged)
                Redraw();
        }

        public void ClearBuffer()
        {
            _buffer.Clear();
        }
        #endregion

        #region Events

        public delegate void BeginRedrawDelegate(Graphics gfx, int width, int height);

        public event BeginRedrawDelegate BeginRedraw;

        public delegate void EndRedrawDelegate(Graphics gfx, int width, int height);

        public event EndRedrawDelegate EndRedraw;

        public delegate void DocumentAddedDelegate(Document doc);

        public event DocumentAddedDelegate DocumentAdded;
        #endregion
    }
}
