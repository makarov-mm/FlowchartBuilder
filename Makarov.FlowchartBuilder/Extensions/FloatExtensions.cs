namespace Makarov.FlowchartBuilder.Extensions
{

    public static class FloatExtensions
    {
        #region Convertions

        public static float MMToCurr(this float mm)
        {
            return UnitsConverter.MMToCurr(mm);
        }

        public static float CurrToMM(this float curr)
        {
            return UnitsConverter.CurrToMM(curr);
        }

        public static int CurrToPx(this float curr)
        {
            return UnitsConverter.CurrToPx(curr);
        }

        public static float PxToCurr(this float px)
        {
            return UnitsConverter.PxToCurr(px);
        }

        public static float PxToInch(this float px)
        {
            return UnitsConverter.PxToInch(px);
        }

        public static int InchToPx(this float inch)
        {
            return UnitsConverter.InchToPx(inch);
        }

        public static float InchToMM(this float inch)
        {
            return UnitsConverter.InchToMM(inch);
        }

        public static float MMToInch(this float mm)
        {
            return UnitsConverter.MMToInch(mm);
        }

        public static int MMToPx(this float mm)
        {
            return UnitsConverter.MMToPx(mm);
        }

        public static float PxToMM(this float px)
        {
            return UnitsConverter.PxToMM(px);
        }

        public static float InchToCurr(this float inch)
        {
            return UnitsConverter.InchToCurr(inch);
        }

        public static float CurrToInch(this float curr)
        {
            return UnitsConverter.CurrToInch(curr);
        }
        #endregion

        #region Scaling

        public static float Scale(this float val)
        {
            return val * Core.Instance.CurrentDocument.DocumentSheet.ScaleFactor;
        }

        public static float Unscale(this float val)
        {
            return val / Core.Instance.CurrentDocument.DocumentSheet.ScaleFactor;
        }
        #endregion
    }
}
