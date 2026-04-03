public class NutritionBarService
{
    public static float GetBarPercent(int cur, int max)
    {
        float average = (float)cur / max * 100;

        if (average > 100)
        {
            average = 100;
        }

        return average;
    }
}