namespace BookToAudio.SeleniumTests.RegisterPageTests;

internal sealed class RegisterPageTests: TestBase
{
   // [Test]
  //  [TestCase("http://localhost:4000/register")]
    public void  RagisterTests(string url)
    {
        Driver.Navigate().GoToUrl(url);
        var registerPage = new RegisterLogic(Driver);
        string name = "selenium@ua.test";
        string password = "Selenium_test";
        registerPage.RegisterUser(name, password);
    }
}
