
namespace EviLA.AddOns.RPGPack.Experience
{
    public interface IExperienceCalculatorStrategy
    {
        void SetBaseXP(double baseXP);
        double CalculateRequiredXPForNextLevel(int currentLevel);
        void DisplaySampleLevels();
    }
}