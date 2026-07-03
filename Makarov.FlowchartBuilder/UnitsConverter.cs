namespace Makarov.FlowchartBuilder
{

    public static class UnitsConverter
    {

        public static float MMToCurr(float mm)
        {
            return mm * Settings.Environment.CurrentUnits.InMM;
        }

        public static float CurrToMM(float curr)
        {
            return curr * Settings.Environment.CurrentUnits.MMInThis;
        }

        public static int CurrToPx(float curr)
        {
            return InchToPx(CurrToInch(curr));
        }

        public static float PxToCurr(float px)
        {
            return MMToCurr(PxToMM(px));
        }

        public static float PxToInch(float px)
        {
            return px / Settings.Environment.DPI;
        }

        public static int InchToPx(float inch)
        {
            return (int)(inch * Settings.Environment.DPI);
        }

        public static float InchToMM(float inch)
        {
            return inch * Core.Instance.Units["Inches"].MMInThis;
        }

        public static float MMToInch(float mm)
        {
            return mm * Core.Instance.Units["Inches"].InMM;
        }

        public static int MMToPx(float mm)
        {
            return InchToPx(MMToInch(mm));
        }

        public static float PxToMM(float px)
        {
            return InchToMM(PxToInch(px));
        }

        public static float InchToCurr(float inch)
        {
            return MMToCurr(InchToMM(inch));
        }

        public static float CurrToInch(float curr)
        {
            return MMToInch(CurrToMM(curr));
        }
    }
}
