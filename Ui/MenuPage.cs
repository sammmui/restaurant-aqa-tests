using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Ui.Components;

namespace Restaurant_Aqa_Framework.Ui;

public sealed class MenuPage(IPage page) : BasePage(page)
{
    public HeaderComponent Header { get; } = new(page);

    private ILocator CategoryButtons => Page.Locator(".category-buttons button");
    public ILocator DishCards => Page.Locator("app-dish-card");
    public ILocator DishNames => DishCards.Locator(".dish-name");
    private ILocator SortSelect => Page.GetByLabel("Sort by:");

    public Task NavigateAsync() => NavigateToAsync("/menu");

    /// <summary>Pre-order mode is entered via query params, carrying the reservation the dishes
    /// will be attached to — there is no in-app link to it from the plain menu page.</summary>
    public Task NavigateForPreOrderAsync(string reservationId, string reservationLabel) =>
        NavigateToAsync($"/menu?preOrder=true&reservationId={Uri.EscapeDataString(reservationId)}&reservationLabel={Uri.EscapeDataString(reservationLabel)}");

    public async Task<IResponse> FilterByCategoryAsync(string categoryLabel, DishType category)
    {
        var responseTask = WaitForMenuResponseAsync($"dishType={category.ToQueryValue()}");
        await CategoryButtons.Filter(new() { HasText = categoryLabel }).ClickAsync();
        return await responseTask;
    }

    public async Task<IResponse> SelectSortAsync(DishSortOrder sorting)
    {
        var responseTask = WaitForMenuResponseAsync($"sortBy={sorting.ToQueryValue()}");
        await SortSelect.SelectOptionAsync(sorting.ToQueryValue());
        return await responseTask;
    }

    public async Task<IReadOnlyList<(string Name, string Price)>> ReadDishesAsync()
    {
        var dishes = await DishCards.EvaluateAllAsync<string[][]>(
            "cards => cards.map(card => [card.querySelector('.dish-name')?.textContent?.trim() ?? '', card.querySelector('.dish-price')?.textContent?.trim() ?? ''])");
        return dishes.Select(dish => (dish[0], dish[1])).ToList();
    }

    public DishCardComponent Dish(string dishName) => new(DishCards.Filter(new() { HasText = dishName }));

    public ILocator DishModalName(string dishName) =>
        Page.Locator("app-dish-modal .modal-content .dish-name").Filter(new() { HasText = dishName });

    private Task<IResponse> WaitForMenuResponseAsync(string queryParameter) =>
        Page.WaitForResponseAsync(response =>
            response.Url.Contains("/dishes?", StringComparison.Ordinal) &&
            response.Url.Contains(queryParameter, StringComparison.Ordinal));
}
