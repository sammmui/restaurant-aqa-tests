using Restaurant_Aqa_Framework.Api.Dtos.Dishes;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

public sealed class TheRenderedDishesOutsideCategory(
    DishType category,
    IReadOnlyList<StorefrontDish> renderedDishes) : IQuestion<IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> AnsweredByAsync(Actor actor)
    {
        var response = actor.Recall<StorefrontMenuResult>();
        IReadOnlyList<string> dishesOutsideCategory = renderedDishes
            .Where(rendered => !response.Dishes.Any(dish =>
                dish.Name == rendered.Name && dish.DishType == category.ToLabel()))
            .Select(dish => dish.Name)
            .ToList();
        return Task.FromResult(dishesOutsideCategory);
    }
}
