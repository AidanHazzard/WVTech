public class NutritionBarInfoViewModel
{
    public NutritionBarInfoViewModel(int calCur, int calMax, int proCur, int proMax, int fatCur, int fatMax, int carbCur, int carbMax)
    {
        CaloriesCurrent = calCur;
        CaloriesMax = calMax;
        ProteinCurrent = proCur;
        ProteinMax = proMax;
        FatsCurrent = fatCur;
        FatsMax = fatMax;
        CarbsCurrent = carbCur;
        CarbsMax = carbMax;
    }
    public int CaloriesCurrent { get; set; }
    public int CaloriesMax { get; set; }
    public int ProteinCurrent { get; set; }
    public int ProteinMax { get; set; }
    public int FatsCurrent { get; set;}
    public int FatsMax { get; set;}
    public int CarbsCurrent { get; set; }
    public int CarbsMax { get; set; }
}