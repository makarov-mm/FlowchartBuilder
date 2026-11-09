namespace Makarov.FlowchartBuilder.Extensions;

/// <summary>
/// Unit conversions and zoom scaling as extension members. Scaling uses the zoom of the current document.
/// </summary>
public static class UnitExtensions
{
    extension(float value)
    {
        public int MMToPx() => UnitsConverter.MMToPx(value);

        public float PxToMM() => UnitsConverter.PxToMM(value);

        public float MMToCurr() => UnitsConverter.MMToCurr(value);

        public float Scale() => value * CurrentScaleFactor;

        public float Unscale() => value / CurrentScaleFactor;
    }

    extension(int value)
    {
        public int MMToPx() => UnitsConverter.MMToPx(value);

        public float PxToMM() => UnitsConverter.PxToMM(value);

        public int CurrToPx() => UnitsConverter.CurrToPx(value);

        public int Scale() => (int)(value * CurrentScaleFactor);

        public int Unscale() => (int)(value / CurrentScaleFactor);
    }

    private static float CurrentScaleFactor => Core.Instance.CurrentDocument.DocumentSheet.ScaleFactor;
}
