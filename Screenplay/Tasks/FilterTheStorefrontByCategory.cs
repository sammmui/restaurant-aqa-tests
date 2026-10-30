using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class FilterTheStorefrontByCategory(DishType category) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var menu = new MenuPage(actor.LookingAtThePage());
        await menu.NavigateAsync();
        var categoryLabel = category switch
        {
            DishType.Appetizer => "Appetizers",
            DishType.MainCourse => "Main Courses",
            DishType.Dessert => "Desserts",
            _ => throw new NotSupportedException($"The category '{category}' is not supported."),
        };
        var response = await menu.FilterByCategoryAsync(categoryLabel, category);
        actor.Remember(await StorefrontMenuResult.FromResponseAsync(response));
    }
}
