using BookToAudio.Core.Dto;
using BookToAudio.SeleniumTests.Pages;

namespace BookToAudio.SeleniumTests.Tests;

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
