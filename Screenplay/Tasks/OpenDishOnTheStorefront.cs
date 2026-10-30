using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class OpenDishOnTheStorefront : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var dish = actor.Recall<DishResponse>();
        var menu = new MenuPage(actor.LookingAtThePage());
        await menu.NavigateAsync();
        await menu.Dish(dish.Name).OpenDetailsAsync();
    }
}
