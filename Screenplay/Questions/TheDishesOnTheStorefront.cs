using static Microsoft.Playwright.Assertions;
using Restaurant_Aqa_Framework.Ui;
using System.Globalization;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

public sealed class TheDishesOnTheStorefront(IReadOnlyList<string>? dishNamesToWaitFor = null)
    : IQuestion<IReadOnlyList<StorefrontDish>>
{
    private static readonly CultureInfo CurrencyCulture = CultureInfo.GetCultureInfo("en-US");

    public async Task<IReadOnlyList<StorefrontDish>> AnsweredByAsync(Actor actor)
    {
        var menu = new MenuPage(actor.LookingAtThePage());
        await Expect(menu.DishCards.First).ToBeVisibleAsync();

        if (actor.TryRecall<StorefrontMenuResult>(out var responseDishes) && responseDishes.Dishes.Count > 0)
        {
            await Expect(menu.DishNames.First).ToHaveTextAsync(responseDishes.Dishes[0].Name);
            await Expect(menu.DishNames.Last).ToHaveTextAsync(responseDishes.Dishes[^1].Name);
        }

        foreach (var dishName in dishNamesToWaitFor ?? [])
            await Expect(menu.Dish(dishName).Name).ToBeVisibleAsync();

        var renderedDishes = await menu.ReadDishesAsync();
        return renderedDishes.Select(dish =>
        {
            if (!decimal.TryParse(dish.Price, NumberStyles.Currency, CurrencyCulture, out var price))
                throw new FormatException($"Could not parse rendered price '{dish.Price}' for dish '{dish.Name}'.");

            return new StorefrontDish(dish.Name, price);
        }).ToList();
    }
}
