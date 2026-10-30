using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

public sealed class TheLoginFormIsShown : IQuestion<bool>
{
    public Task<bool> AnsweredByAsync(Actor actor) =>
        new LoginPage(actor.LookingAtThePage()).EmailInput.IsVisibleAsync();
}
