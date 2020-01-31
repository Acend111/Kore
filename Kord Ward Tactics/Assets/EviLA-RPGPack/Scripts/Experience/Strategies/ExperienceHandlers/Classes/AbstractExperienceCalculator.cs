namespace EviLA.AddOns.RPGPack.Experience
{
    public abstract class AbstractExperienceCalculator : IExperienceCalculatorStrategy
    {
        protected double baseXP;
        protected double previousLevelXP;
        protected double incrementFactor;

        public abstract double CalculateRequiredXPForNextLevel(int currentLevel);
        public abstract void DisplaySampleLevels();

        public void SetBaseXP(double baseXP)
        {
            this.baseXP = baseXP;
        }

        public double IncrementFactor
        {
            get { return incrementFactor; }
        }
    }
}
