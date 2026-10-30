using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Support;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class SearchForDishByName(DishNameMatch match) : ITask
{
    public Task PerformAsAsync(Actor actor)
    {
        var dishName = actor.Recall<DishResponse>().Name;
        var searchTerm = match switch
        {
            DishNameMatch.FullName => dishName,
            DishNameMatch.NamePrefix => dishName[..Math.Min(10, dishName.Length)],
            _ => throw new NotSupportedException($"The search match '{match}' is not supported."),
        };

        return actor.AttemptsToAsync(new BrowseTheMenu(nameContains: searchTerm));
    }
}
