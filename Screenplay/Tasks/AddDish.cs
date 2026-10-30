using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Api.Dtos.Locations;
using Restaurant_Aqa_Framework.Api.Seeding;
using Restaurant_Aqa_Framework.Screenplay.Questions;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Adds a sample dish of the given category to a location: the one named, or else the one this scenario
/// set up most recently.</summary>
public sealed class AddDish(DishType type = DishType.MainCourse, string? toLocation = null) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var location = actor.Recall<LocationResponse>(toLocation);
        var waiter = await actor.AsksForAsync(new TheWaiterSession());
        var api = new DishesApi(actor.UsingTheApi());
        var response = actor.TheLastResponse();

        var request = SampleDataFactory.CreateDishRequest([location.Id], type) with
        {
            Name = $"{SampleDataFactory.Prefix}{Guid.NewGuid():N}",
        };
        await response.CaptureAsync(await api.CreateDishAsync(request));
        if (!response.Succeeded)
            throw new InvalidOperationException($"Adding the dish failed with {response.Status}: {response.Body}");
        var dish = response.Json<DishResponse>();
        actor.Remember(dish);

        actor.Recall<CleanupScope>().TrackDelete(
            $"dish:{dish.Id}",
            () => api.DeleteDishAsync(dish.Id, force: true, bearerToken: waiter.IdToken));
    }
}
