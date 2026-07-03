namespace Makarov.FlowchartBuilder
{

    public sealed class Units
    {
        #region Exceptions

        public class UnitsInvalidValueException : InvalidValueException
        {

            public UnitsInvalidValueException(string entity, string entityValue)
                : base(string.Format("Units.{0}", entity), entityValue)
            { }
        }
        #endregion

        #region Constructors

        public Units(string Name, string ShortName, float InMM, int BigStep, int SmallStep)
        {
            if (string.IsNullOrEmpty(Name)) throw new UnitsInvalidValueException("Name", "null");
            this.Name = Name;

            if (string.IsNullOrEmpty(ShortName)) throw new UnitsInvalidValueException("ShortName", "null");
            this.ShortName = ShortName;

            if (InMM <= 0) throw new UnitsInvalidValueException("InMM", InMM.ToString());
            this.InMM = InMM;

            if (BigStep <= 0) throw new UnitsInvalidValueException("BigStep", BigStep.ToString());
            this.BigStep = BigStep;

            if (SmallStep <= 0) throw new UnitsInvalidValueException("SmallStep", SmallStep.ToString());
            this.SmallStep = SmallStep;
        }
        #endregion

        #region Properties

        public string Name
        {
            get; private set;
        }

        public string ShortName
        {
            get; private set;
        }

        public float InMM
        {
            get; private set;
        }

        public float MMInThis
        {
            get { return 1f / InMM; }
        }

        public int BigStep
        {
            get; private set;
        }

        public int SmallStep
        {
            get; private set;
        }
        #endregion

        #region Public methods

        public override string ToString()
        {
            return string.Format("Units '{0}' (short name:'{1}', InMM:{2}, big step: {3}, small step: {4})",
                                 Name, ShortName, InMM, BigStep, SmallStep);
        }

        public float ToMM(float units)
        {
            return units / InMM;
        }

        public float MMToThis(float mm)
        {
            return mm / MMInThis;
        }
        #endregion

        #region Operators
        public static implicit operator float(Units units)
        {
            return units.MMInThis;
        }

        public static implicit operator string(Units units)
        {
            return units.ToString();
        }
        #endregion
    }
}
