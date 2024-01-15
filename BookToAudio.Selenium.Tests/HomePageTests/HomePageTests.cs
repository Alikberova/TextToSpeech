namespace BookToAudio.Selenium.Tests.HomePageTests;

public class Tests: BaseClass
{
    [Test]
    [TestCase("http://localhost:4200/home")]
    public void  HomePageTests(string url)
    {
        driver.Navigate().GoToUrl(url);
        var homePageLogic = new HomePageLogic(driver);
        homePageLogic.SelectedVoice();
        homePageLogic.SelectedTextFile();
        homePageLogic.ClickSendBtn();
    }
}