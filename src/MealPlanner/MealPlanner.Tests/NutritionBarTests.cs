using MealPlanner.Services;

namespace MealPlanner.Tests
{
    [TestFixture]
    public class NutritionBarTests
    {
        [Test]
        public async Task GetsPercentage()
        {
            Assert.That(56.0f == NutritionBarService.GetBarPercent(560, 1000));
        }

        [Test]
        public async Task PercentageCappedAt100()
        {
            Assert.That(100f == NutritionBarService.GetBarPercent(2, 1));
        }
    }
}