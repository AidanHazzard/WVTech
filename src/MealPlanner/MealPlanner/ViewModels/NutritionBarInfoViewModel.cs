using MealPlanner.Services;
public class NutritionBarInfoViewModel
{
    public NutritionBarInfoViewModel(int calCur, int calMax, int proCur, int proMax, int fatCur, int fatMax, int carbCur, int carbMax)
    {
        CaloriesCurrent = calCur;
        CaloriesMax = calMax;
        CaloriesAverage = NutritionBarService.BarAverage(calCur, calMax);
        ProteinCurrent = proCur;
        ProteinMax = proMax;
        ProteinAverage = NutritionBarService.BarAverage(proCur, proMax);
        FatsCurrent = fatCur;
        FatsMax = fatMax;
        FatsAverage = NutritionBarService.BarAverage(fatCur, fatMax);
        CarbsCurrent = carbCur;
        CarbsMax = carbMax;
        CarbsAverage = NutritionBarService.BarAverage(carbCur, carbMax);
    }
    public int CaloriesCurrent { get; set; }
    public int CaloriesMax { get; set; }
    
    public float CaloriesAverage { get; set; }
    public int ProteinCurrent { get; set; }
    public int ProteinMax { get; set; }
    public float ProteinAverage { get; set; }
    public int FatsCurrent { get; set;}
    public int FatsMax { get; set;}
    public float FatsAverage { get; set;}
    public int CarbsCurrent { get; set; }
    public int CarbsMax { get; set; }
    public float CarbsAverage { get; set; }
}