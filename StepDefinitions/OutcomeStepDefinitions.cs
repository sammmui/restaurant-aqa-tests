using Restaurant_Aqa_Framework.Screenplay;
using Restaurant_Aqa_Framework.Screenplay.Questions;

namespace Restaurant_Aqa_Framework.StepDefinitions;

/// <summary>Business-level outcomes shared by several features ("the visitor is told ...", "could not be
/// found"). How an outcome shows up on the wire — which status code, which error body — is decided here,
/// so feature files never mention it.</summary>
[Binding]
public sealed class OutcomeStepDefinitions(Actor actor) : StepDefinitionBase(actor)
{
    [Then("the visitor is told {string}")]
    [Scope(Tag = "api")]
    public async Task ThenVisitorIsTold(string message)
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());

        await Assert.That(outcome.Succeeded).IsFalse().Because($"the request unexpectedly succeeded: {outcome.Body}");
        await Assert.That(outcome.ErrorMessage).IsEqualTo(message);
    }

    [Then("sign-in is refused")]
    [Scope(Tag = "api")]
    public async Task ThenSignInIsRefused()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsEqualTo(401).Because($"response body was: {outcome.Body}");
    }

    [Then("sign-in is refused")]
    [Scope(Tag = "ui")]
    public async Task ThenSignInIsRefusedOnTheLoginForm() =>
        await Assert.That(await Actor.AsksForAsync(new TheLoginFormIsShown()))
            .IsTrue()
            .Because("the login form remains visible after the credentials are refused");

    [Then("the login form shows {string}")]
    public async Task ThenLoginFormShows(string message) =>
        await Assert.That(await Actor.AsksForAsync(new TheLoginFormMessages()))
            .Contains(message)
            .Because("the login form should explain why sign-in did not proceed");

    [Then("the guest is told that the {item} could not be found")]
    public async Task ThenGuestIsToldNotFound(string item)
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());

        await Assert.That(outcome.Status).IsEqualTo(404).Because($"response body was: {outcome.Body}");
        await Assert.That(outcome.ErrorMessage).Contains("was not found");
    }

    [Then("the location is not created")]
    public async Task ThenLocationIsNotCreated()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(400).Because($"the location was created: {outcome.Body}");
    }

    [Then("the information is available")]
    [Then("the menu is shown")]
    public async Task ThenInformationIsAvailable()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Succeeded).IsTrue().Because($"response status {outcome.Status}, body: {outcome.Body}");
    }
}
