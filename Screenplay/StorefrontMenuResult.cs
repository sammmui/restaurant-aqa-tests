using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using System.Text.Json;

namespace Restaurant_Aqa_Framework.Screenplay;

public sealed record StorefrontMenuResult(IReadOnlyList<DishListResponse> Dishes)
{
    public static async Task<StorefrontMenuResult> FromResponseAsync(IResponse response)
    {
        if (!response.Ok)
            throw new InvalidOperationException(
                $"Loading the storefront menu failed with HTTP {response.Status}: {await response.TextAsync()}");

        var body = await response.TextAsync();
        var dishes = JsonSerializer.Deserialize<List<DishListResponse>>(body, JsonDefaults.Options)
            ?? throw new InvalidOperationException(
                $"The storefront menu response could not be deserialized. Body: {body}");

        return new StorefrontMenuResult(dishes);
    }
}
