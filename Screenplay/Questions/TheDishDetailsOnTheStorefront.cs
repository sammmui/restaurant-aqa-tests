using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

public sealed class TheDishDetailsOnTheStorefront : IQuestion<bool>
{
    public async Task<bool> AnsweredByAsync(Actor actor)
    {
        var dish = actor.Recall<DishResponse>();
        var modalName = new MenuPage(actor.LookingAtThePage()).DishModalName(dish.Name);
        try
        {
            await modalName.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            return await modalName.IsVisibleAsync();
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}
