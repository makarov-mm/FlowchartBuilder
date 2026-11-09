namespace Makarov.FlowchartBuilder;

/// <summary>
/// Conversions between millimeters (the document unit), inches, screen pixels and the units chosen by the user.
/// </summary>
public static class UnitsConverter
{
    private static float? _inchesInMM;

    /// <summary>Inches per millimeter, as defined by the "Inches" entry of Data\Units.xml.</summary>
    private static float InchesInMM => _inchesInMM ??= Core.Instance.Units["Inches"].InMM;

    private static Units Current => Settings.Environment.CurrentUnits;

    public static float MMToCurr(float mm) => mm * Current.InMM;

    public static float CurrToMM(float curr) => curr * Current.MMInThis;

    public static int CurrToPx(float curr) => InchToPx(CurrToInch(curr));

    public static float PxToCurr(float px) => MMToCurr(PxToMM(px));

    public static float PxToInch(float px) => px / Settings.Environment.Dpi;

    public static int InchToPx(float inch) => (int)(inch * Settings.Environment.Dpi);

    public static float InchToMM(float inch) => inch * (1f / InchesInMM);

    public static float MMToInch(float mm) => mm * InchesInMM;

    public static int MMToPx(float mm) => InchToPx(MMToInch(mm));

    public static float PxToMM(float px) => InchToMM(PxToInch(px));

    public static float CurrToInch(float curr) => MMToInch(CurrToMM(curr));
}
