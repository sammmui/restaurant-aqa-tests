using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class SortTheStorefront(DishSortOrder sorting) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var menu = new MenuPage(actor.LookingAtThePage());
        await menu.NavigateAsync();
        var response = await menu.SelectSortAsync(sorting);
        actor.Remember(await StorefrontMenuResult.FromResponseAsync(response));
    }
}
