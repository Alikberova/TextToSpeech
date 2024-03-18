namespace BookToAudio.SeleniumTests.RegisterPageTests;

internal sealed class RegisterPageTests: BaseClass
{
   // [Test]
  //  [TestCase("http://localhost:4000/register")]
    public void  RagisterTests(string url)
    {
        driver.Navigate().GoToUrl(url);
        var registerPage = new RegisterLogic(driver);
        string name = "selenium@ua.test";
        string password = "Selenium_test";
        registerPage.RegisterUser(name, password);
    }
}
