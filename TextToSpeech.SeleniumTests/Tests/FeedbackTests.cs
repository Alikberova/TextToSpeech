using TextToSpeech.Core.Dto;
using TextToSpeech.SeleniumTests.Pages;

namespace TextToSpeech.SeleniumTests.Tests;

public sealed class FeedbackTests : TestBase
{
    private const string SuccessMessage = "Your feedback was sent. Thanks!";

    [Fact]
    public void SubmitFeedback()
    {
        var request = new EmailRequest()
        {
            Name = "Jane",
            UserEmail = "jane@qwerty.com",
            Message = "Lorem ipsum"
        };

        var page = new FeedbackPage(Driver, Wait);
        page.ClickMenu();
        page.FillForm(request);
        page.Submit();

        Assert.Contains(SuccessMessage, page.GetSnackBarText());
    }
}
