namespace BookToAudio.Selenium.Tests.RegisterPageTests;

internal class RegisterPageTests: BaseClass
{
    [Test]
    [TestCase("http://localhost:4200/register")]
    public void  RagisterTests(string url)
    {
        driver.Navigate().GoToUrl(url);
        var registerPage = new RegisterPageLogic(driver);
        string name = "Anton";
        string password = "Fynjy1997";
        registerPage.RegisterUser(name, password);
    }
}
