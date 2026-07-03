namespace Makarov.FlowchartBuilder.Extensions
{

    public static class Int32Extensions
    {
        #region Convertions

        public static float MMToCurr(this int mm)
        {
            return UnitsConverter.MMToCurr(mm);
        }

        public static float CurrToMM(this int curr)
        {
            return UnitsConverter.CurrToMM(curr);
        }

        public static int CurrToPx(this int curr)
        {
            return UnitsConverter.CurrToPx(curr);
        }

        public static float PxToCurr(this int px)
        {
            return UnitsConverter.PxToCurr(px);
        }

        public static float PxToInch(this int px)
        {
            return UnitsConverter.PxToInch(px);
        }

        public static int InchToPx(this int inch)
        {
            return UnitsConverter.InchToPx(inch);
        }

        public static float InchToMM(this int inch)
        {
            return UnitsConverter.InchToMM(inch);
        }

        public static float MMToInch(this int mm)
        {
            return UnitsConverter.MMToInch(mm);
        }

        public static int MMToPx(this int mm)
        {
            return UnitsConverter.MMToPx(mm);
        }

        public static float PxToMM(this int px)
        {
            return UnitsConverter.PxToMM(px);
        }

        public static float InchToCurr(this int inch)
        {
            return UnitsConverter.InchToCurr(inch);
        }

        public static float CurrToInch(this int curr)
        {
            return UnitsConverter.CurrToInch(curr);
        }
        #endregion

        #region Scaling

        public static int Scale(this int val)
        {
            return (int)(val * Core.Instance.CurrentDocument.DocumentSheet.ScaleFactor);
        }

        public static int Unscale(this int val)
        {
            return (int)(val / Core.Instance.CurrentDocument.DocumentSheet.ScaleFactor);
        }
        #endregion
    }
}
