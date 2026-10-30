using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Auth;
using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Api.Dtos.Locations;
using Restaurant_Aqa_Framework.Api.Dtos.Orders;
using Restaurant_Aqa_Framework.Api.Dtos.Reservations;
using Restaurant_Aqa_Framework.Api.Dtos.Tables;
using Restaurant_Aqa_Framework.Api.Seeding;
using Restaurant_Aqa_Framework.Contexts;
using Restaurant_Aqa_Framework.Screenplay.Questions;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class SetUpPopularityFixture : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var api = actor.UsingTheApi();
        var response = actor.TheLastResponse();
        var location = actor.Recall<LocationResponse>();
        var waiter = await actor.AsksForAsync(new TheWaiterSession());
        var cleanup = actor.Recall<CleanupScope>();
        var dishesApi = new DishesApi(api);

        var morePopularRequest = SampleDataFactory.CreateDishRequest([location.Id], DishType.MainCourse) with
        {
            Name = $"{SampleDataFactory.Prefix}{Guid.NewGuid():N}",
        };
        await response.CaptureAsync(await dishesApi.CreateDishAsync(morePopularRequest));
        EnsureSucceeded(response, "Adding the more popular fixture dish");
        var morePopular = response.Json<DishResponse>();
        cleanup.TrackDelete(
            $"dish:{morePopular.Id}",
            () => dishesApi.DeleteDishAsync(morePopular.Id, force: true, bearerToken: waiter.IdToken));

        var lessPopularRequest = SampleDataFactory.CreateDishRequest([location.Id], DishType.MainCourse) with
        {
            Name = $"{SampleDataFactory.Prefix}{Guid.NewGuid():N}",
        };
        await response.CaptureAsync(await dishesApi.CreateDishAsync(lessPopularRequest));
        EnsureSucceeded(response, "Adding the less popular fixture dish");
        var lessPopular = response.Json<DishResponse>();
        cleanup.TrackDelete(
            $"dish:{lessPopular.Id}",
            () => dishesApi.DeleteDishAsync(lessPopular.Id, force: true, bearerToken: waiter.IdToken));

        var tablesApi = new TablesApi(api);
        await response.CaptureAsync(await tablesApi.CreateTableAsync(
            SampleDataFactory.CreateTableRequest(location.Id), waiter.IdToken));
        EnsureSucceeded(response, "Adding a table for the popularity fixture");
        var table = response.Json<TableResponse>();
        cleanup.TrackDelete(
            $"table:{table.Id}",
            () => tablesApi.DeleteTableAsync(table.Id, waiter.IdToken));

        var visitor = SampleDataFactory.RegisterRequest();
        var authApi = new AuthApi(api);
        await response.CaptureAsync(await authApi.RegisterAsync(visitor));
        EnsureSucceeded(response, "Registering a customer for the popularity fixture");

        await response.CaptureAsync(await authApi.SignInAsync(new SignInRequest(visitor.Email, visitor.Password)));
        EnsureSucceeded(response, "Signing in the popularity fixture customer");
        var customerToken = response.Json<SignInResponse>().IdToken;

        var reservationsApi = new ReservationsApi(api);
        var startTime = DateTime.Now.AddHours(1);
        await response.CaptureAsync(await reservationsApi.CreateReservationAsync(
            new CreateReservationRequest(table.Id, startTime, startTime.AddHours(1), GuestCount: 2),
            customerToken));
        EnsureSucceeded(response, "Creating a reservation for the popularity fixture");
        var reservation = response.Json<ReservationResponse>();
        cleanup.TrackDelete(
            $"reservation:{reservation.Id}",
            () => reservationsApi.CancelReservationAsync(reservation.Id, customerToken));

        var ordersApi = new OrdersApi(api);
        await response.CaptureAsync(await ordersApi.CreateOrderAsync(
            new CreateOrderRequest(reservation.Id,
            [
                new OrderItemRequest(morePopular.Id, 1),
                new OrderItemRequest(morePopular.Id, 1),
                new OrderItemRequest(lessPopular.Id, 1),
            ]),
            customerToken));
        EnsureSucceeded(response, "Recording the popularity fixture order");
        cleanup.TrackDelete(
            $"order:{reservation.Id}",
            () => ordersApi.DeleteOrderAsync(reservation.Id, customerToken));

        actor.Remember(new PopularityFixture(morePopular, lessPopular));
    }

    private static void EnsureSucceeded(ApiResponseContext response, string action)
    {
        if (!response.Succeeded)
            throw new InvalidOperationException($"{action} failed with {response.Status}: {response.Body}");
    }
}
