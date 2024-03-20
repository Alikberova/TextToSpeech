namespace BookToAudio.SeleniumTests.FeedbackTests;

internal class FeedbackTests : BaseClass
{
    [Test]
    public void TtsFormPageTests()
    {
        var feedback = new Feedback(_driver, _wait);

        feedback.Send();
    }
}
