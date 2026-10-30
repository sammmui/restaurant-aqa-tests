using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

public sealed class TheLoginFormMessages : IQuestion<IReadOnlyList<string>>
{
    public async Task<IReadOnlyList<string>> AnsweredByAsync(Actor actor)
    {
        var messages = new LoginPage(actor.LookingAtThePage()).FieldErrors;
        try
        {
            await messages.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
        }
        catch (TimeoutException)
        {
            return [];
        }

        return (await messages.AllTextContentsAsync()).Select(text => text.Trim()).ToList();
    }
}
