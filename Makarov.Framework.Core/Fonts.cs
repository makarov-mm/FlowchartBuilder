using System.Drawing;

namespace Makarov.Framework.Core
{

    public static class Fonts
    {

        public sealed class CantCreateFontException : MakarovFrameworkException
        {

            public CantCreateFontException(string familyName)
                : base($"Can't create font '{familyName}'.")
            { }
        }

        public static Font SafeCreateFont(FontFamily familyName, float emSize)
        {

            var styles = new[]
                             {
                                 FontStyle.Regular,
                                 FontStyle.Italic,
                                 FontStyle.Bold,
                                 FontStyle.Underline,
                                 FontStyle.Strikeout
                             };

            for (int i = 0; i < styles.Length; i++)
                try
                {

                    return new Font(familyName, emSize, styles[i]);
                }
                catch
                {

                    if (i == styles.Length - 1)
                        break;

                }

            throw new CantCreateFontException(familyName.Name);
        }

        public static Font SafeCreateFont(string familyName, float emSize)
        {

            var styles = new[]
            {
                FontStyle.Regular,
                FontStyle.Italic,
                FontStyle.Bold,
                FontStyle.Underline,
                FontStyle.Strikeout
            };

            for (int i = 0; i < styles.Length; i++)
                try
                {

                    return new Font(familyName, emSize, styles[i]);
                }
                catch
                {

                    if (i == styles.Length - 1)
                        break;

                }

            throw new CantCreateFontException(familyName);
        }
    }
}
