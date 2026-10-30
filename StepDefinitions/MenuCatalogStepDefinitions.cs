using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Screenplay;
using Restaurant_Aqa_Framework.Screenplay.Questions;
using Restaurant_Aqa_Framework.Screenplay.Tasks;
using Restaurant_Aqa_Framework.Support;

namespace Restaurant_Aqa_Framework.StepDefinitions;

/// <summary>What guests see on the menu: browsing, sorting, searching and opening a dish. The dish "the kitchen
/// has added" is a Screenplay task and is remembered, so any step in any class can refer to "that dish".</summary>
[Binding]
public sealed class MenuCatalogStepDefinitions(Actor actor) : StepDefinitionBase(actor)
{
    [Given("the restaurant has a location")]
    public Task GivenRestaurantHasALocation() => Actor.AttemptsToAsync(new ChooseAnExistingLocation());

    [Given("the kitchen has added a {dish_type} dish")]
    public Task GivenKitchenHasAddedADish(DishType type) => Actor.AttemptsToAsync(new AddDish(type));

    [Given("the menu has dishes with a known popularity difference")]
    public Task GivenMenuHasDishesWithKnownPopularityDifference() =>
        Actor.AttemptsToAsync(new SetUpPopularityFixture());

    [Given("two main course dishes have been added")]
    public Task GivenTwoMainCourseDishesHaveBeenAdded() =>
        Actor.AttemptsToAsync(new AddDish(DishType.MainCourse), new AddDish(DishType.MainCourse));

    [When("a guest browses {dish_type} dishes")]
    public Task WhenGuestBrowsesDishes(DishType type) => Actor.AttemptsToAsync(new BrowseTheMenu(category: type));

    [When("a guest sorts the menu by {sort_order}")]
    public Task WhenGuestSortsTheMenu(DishSortOrder order) => Actor.AttemptsToAsync(new BrowseTheMenu(sortedBy: order));

    [When("a guest browses {dish_type} dishes sorted by {sort_order}")]
    public Task WhenGuestBrowsesDishesSorted(DishType type, DishSortOrder order) =>
        Actor.AttemptsToAsync(new BrowseTheMenu(category: type, sortedBy: order));

    [When("a guest searches the menu for that dish by name")]
    public Task WhenGuestSearchesForThatDish() =>
        Actor.AttemptsToAsync(new BrowseTheMenu(nameContains: Recall<DishResponse>().Name));

    [When("a guest searches the menu for the {name_match} of that dish's name")]
    public Task WhenGuestSearchesForDishName(DishNameMatch match) =>
        Actor.AttemptsToAsync(new SearchForDishByName(match));

    [When("a guest opens that dish")]
    public Task WhenGuestOpensThatDish() => Actor.AttemptsToAsync(new OpenTheDish());

    [When("a guest filters the storefront menu by {dish_type}")]
    public Task WhenGuestFiltersStorefrontByCategory(DishType category) =>
        Actor.AttemptsToAsync(new FilterTheStorefrontByCategory(category));

    [When("a guest sorts the storefront menu by {sort_order}")]
    public Task WhenGuestSortsStorefront(DishSortOrder sorting) =>
        Actor.AttemptsToAsync(new SortTheStorefront(sorting));

    [When("a guest opens that dish on the storefront")]
    public Task WhenGuestOpensDishOnStorefront() =>
        Actor.AttemptsToAsync(new OpenDishOnTheStorefront());

    [When("a guest opens a {item} that does not exist")]
    public Task WhenGuestOpensMissingItem(string item) => Actor.AttemptsToAsync(new OpenSomethingThatDoesNotExist(item));

    [Then("all the dishes shown are {dish_type} dishes")]
    public async Task ThenAllDishesAreOfType(DishType type)
    {
        var dishes = await Actor.AsksForAsync(new TheDishesShown());
        await Assert.That(dishes.Where(d => d.DishType != type.ToLabel()).Select(d => d.Name).ToList()).IsEmpty();
    }

    [Then("the dishes are listed {price_order}")]
    public async Task ThenDishesAreListedByPrice(SortDirection direction)
    {
        var prices = (await Actor.AsksForAsync(new TheDishesShown())).Select(d => d.Price).ToList();
        var expected = direction == SortDirection.Ascending ? prices.Order().ToList() : prices.OrderDescending().ToList();

        await Assert.That(prices.SequenceEqual(expected)).IsTrue().Because($"prices were [{string.Join(", ", prices)}]");
    }

    [Then("the fixture dishes are listed {popularity_order}")]
    public async Task ThenFixtureDishesAreListedByPopularity(SortDirection direction)
    {
        var fixture = Recall<PopularityFixture>();
        var ids = (await Actor.AsksForAsync(new TheDishesShown())).Select(dish => dish.Id).ToList();
        var morePopularIndex = ids.IndexOf(fixture.MorePopular.Id);
        var lessPopularIndex = ids.IndexOf(fixture.LessPopular.Id);
        var correctlyOrdered = direction == SortDirection.Ascending
            ? lessPopularIndex >= 0 && lessPopularIndex < morePopularIndex
            : morePopularIndex >= 0 && morePopularIndex < lessPopularIndex;

        await Assert.That(correctlyOrdered).IsTrue().Because(
            $"more popular dish index was {morePopularIndex}; less popular dish index was {lessPopularIndex}");
    }

    [Then("that dish appears in the results")]
    public async Task ThenThatDishAppearsInResults() =>
        await Assert.That((await Actor.AsksForAsync(new TheDishesShown())).Select(d => d.Id)).Contains(Recall<DishResponse>().Id);

    [Then("that dish does not appear in the results")]
    public async Task ThenThatDishDoesNotAppearInResults() =>
        await Assert.That((await Actor.AsksForAsync(new TheDishesShown())).Select(d => d.Id)).DoesNotContain(Recall<DishResponse>().Id);

    [Then("the dish is shown with:")]
    public async Task ThenDishIsShownWith(Fields expected)
    {
        foreach (var key in expected.Keys.ToList())
            if (expected[key] == "(sample dish name)")
                expected[key] = Recall<DishResponse>().Name;

        await Assert.That(expected.Mismatches(await Actor.AsksForAsync(new TheResponseJson()))).IsEmpty();
    }

    [When("a guest views the menu")]
    public Task WhenGuestViewsTheMenu() => Actor.AttemptsToAsync(new OpenTheMenuPage());

    [Then("that dish is visible on the menu")]
    public async Task ThenThatDishIsVisibleOnTheMenu() =>
        await Assert.That(await Actor.AsksForAsync(new TheDishOnTheMenu(Recall<DishResponse>().Name))).IsTrue();

    [Then("every dish shown on the storefront belongs to {dish_type}")]
    public async Task ThenEveryStorefrontDishBelongsToCategory(DishType category)
    {
        var ownedDishNames = Actor.RecallAll<DishResponse>().Select(dish => dish.Name).ToList();
        var renderedDishes = await Actor.AsksForAsync(new TheDishesOnTheStorefront(ownedDishNames));
        var dishesOutsideCategory = await Actor.AsksForAsync(
            new TheRenderedDishesOutsideCategory(category, renderedDishes));

        await Assert.That(dishesOutsideCategory).IsEmpty().Because(
            $"rendered dishes outside the {category.ToLabel()} category were [{string.Join(", ", dishesOutsideCategory)}]");
    }

    [Then("the dishes added by this scenario are shown")]
    public async Task ThenScenarioDishesAreShownOnStorefront()
    {
        var ownedDishNames = Actor.RecallAll<DishResponse>().Select(dish => dish.Name).ToList();
        var renderedDishes = await Actor.AsksForAsync(new TheDishesOnTheStorefront(ownedDishNames));

        await Assert.That(renderedDishes.Select(dish => dish.Name).Intersect(ownedDishNames).Count())
            .IsEqualTo(ownedDishNames.Count)
            .Because($"scenario dishes were [{string.Join(", ", ownedDishNames)}]");
    }

    [Then("the storefront prices are listed {price_order}")]
    public async Task ThenStorefrontPricesAreOrdered(SortDirection direction)
    {
        var ownedDishNames = Actor.RecallAll<DishResponse>().Select(dish => dish.Name).ToList();
        var prices = (await Actor.AsksForAsync(new TheDishesOnTheStorefront(ownedDishNames)))
            .Select(dish => dish.Price)
            .ToList();
        var correctlyOrdered = direction == SortDirection.Ascending
            ? prices.Zip(prices.Skip(1), (left, right) => left <= right).All(isOrdered => isOrdered)
            : prices.Zip(prices.Skip(1), (left, right) => left >= right).All(isOrdered => isOrdered);

        await Assert.That(correctlyOrdered).IsTrue().Because(
            $"rendered prices were [{string.Join(", ", prices)}]");
    }

    [Then("the fixture dishes are listed {popularity_order} on the storefront")]
    public async Task ThenFixtureDishesAreOrderedByPopularityOnStorefront(SortDirection direction)
    {
        var fixture = Recall<PopularityFixture>();
        var renderedDishes = await Actor.AsksForAsync(new TheDishesOnTheStorefront(
            [fixture.MorePopular.Name, fixture.LessPopular.Name]));
        var names = renderedDishes.Select(dish => dish.Name).ToList();
        var morePopularIndex = names.IndexOf(fixture.MorePopular.Name);
        var lessPopularIndex = names.IndexOf(fixture.LessPopular.Name);
        var correctlyOrdered = direction == SortDirection.Ascending
            ? lessPopularIndex >= 0 && lessPopularIndex < morePopularIndex
            : morePopularIndex >= 0 && morePopularIndex < lessPopularIndex;

        await Assert.That(correctlyOrdered).IsTrue().Because(
            $"more popular dish index was {morePopularIndex}; less popular dish index was {lessPopularIndex}");
    }

    [Then("that dish's details are shown on the storefront")]
    public async Task ThenDishDetailsAreShownOnStorefront() =>
        await Assert.That(await Actor.AsksForAsync(new TheDishDetailsOnTheStorefront())).IsTrue();
}
