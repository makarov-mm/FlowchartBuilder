using System.Drawing;

namespace Makarov.Framework.Components
{

    public interface IActiveControl
    {

        string ActiveCaption { get; set; }

        string ActiveBalloon { get; set; }

        Bitmap ActiveIcon { get; set; }

        bool ActiveEnabled { get; set; }

        bool ActiveChecked { get; set; }
    }
}
