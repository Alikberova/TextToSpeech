using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TextToSpeech.Infra;
using TextToSpeech.SeleniumTests.Extensions;
using static TextToSpeech.SeleniumTests.Constants.UiConstants;

namespace TextToSpeech.SeleniumTests.Pages;

public sealed class TtsFormPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public TtsFormPage(IWebDriver driver, WebDriverWait wait)
    {
        _driver = driver;
        _wait = wait;

        _wait.UntilVisible(By.CssSelector($"[data-testid='{DataTestId.TtsForm}']"));
    }

    private static By DownloadButtonBy => By.CssSelector($"button[data-testid='{DataTestId.DownloadBtn}']");
    private IWebElement ProviderSelect => _driver.FindElement(By.CssSelector($"mat-select[name='{NameAttributes.Provider}']"));
    private IWebElement? ModelSelect => _driver.FindElements(By.CssSelector($"mat-select[name='{NameAttributes.Model}']")).FirstOrDefault();
    private IWebElement? LanguageSelect => _driver.FindElements(By.CssSelector($"mat-select[name='{NameAttributes.Language}']")).FirstOrDefault();
    private IWebElement VoiceSelect => _driver.FindElement(By.CssSelector($"mat-select[name='{NameAttributes.Voice}']"));
    private IWebElement FileInput => _driver.FindElement(By.Id(Ids.FileInput));
    private IWebElement SampleTextArea => _driver.FindElement(By.CssSelector($"textarea[data-testid='{DataTestId.SampleTextArea}']"));
    private IWebElement? SamplePlayButton => _driver.FindElement(By.CssSelector($"button[data-testid='{DataTestId.SamplePlayButton}']"));
    private IWebElement SubmitButton => _driver.FindElement(By.CssSelector($"button[data-testid='{DataTestId.SubmitBtn}']"));
    private IWebElement DownloadButton => _wait.UntilVisibleAndEnabled(DownloadButtonBy);
    private IWebElement ClearButton => _driver.FindElement(By.CssSelector($"button[data-testid='{DataTestId.ClearBtn}']"));
    private IWebElement? CancelProcessingButton => _wait.UntilVisibleAndEnabled(
        By.CssSelector(Selectors.ProgressCancelButton));

    // Queries
    public bool IsVoiceDisabled() => VoiceSelect.GetAttribute("aria-disabled") == "true";
    public bool IsModelEnabled() => ModelSelect is not null && ModelSelect.GetAttribute("aria-disabled") == "false";
    public bool IsLanguageEnabled() => LanguageSelect is not null && LanguageSelect.GetAttribute("aria-disabled") == "false";
    public bool IsFormErrorBannerVisible() => _driver.FindElements(By.CssSelector($"[data-testid='{DataTestId.FormErrorBanner}']")).Count != 0;
    public bool IsFormElementErrorlVisible() => _driver.FindElements(By.TagName("mat-error")).Count != 0;
    public bool IsProgressPanelVisible() => _driver.FindElements(By.CssSelector(Selectors.ProgressPanel)).Count != 0;
    public bool IsCancelButtonVisible() => CancelProcessingButton is not null;

    public bool IsIconVisible(string icon)
    {
        return _wait.Until(_ => SamplePlayButton?.Text.Trim() == icon);
    }

    public bool IsDownloadHiddenOrDisabled()
    {
        var download = _driver.FindElement(DownloadButtonBy);
        var hasHiddenClass = (download.GetAttribute("class") ?? string.Empty).Split(' ').Contains("hidden");
        var isDisabled = string.Equals(download.GetAttribute("disabled"), "true", StringComparison.OrdinalIgnoreCase);

        return hasHiddenClass || isDisabled;
    }

    // Actions
    public void ClickCancel()
    {
        CancelProcessingButton!.Click();
    }

    public string? WaitStatusIconText(string status)
    {
        IWebElement? icon = null;

        try
        {
            icon = _wait.UntilElement(By.CssSelector(Selectors.StatusIcon),
                e => e.Text.Trim().Equals(status, StringComparison.OrdinalIgnoreCase));
        }
        catch (WebDriverTimeoutException)
        {
            icon = _driver.FindElement(By.CssSelector(Selectors.StatusIcon));
        }

        return icon?.Text.Trim();
    }

    public void OpenProviderOptions() => ProviderSelect.Click();
    public void OpenLanguageOptions() => LanguageSelect!.Click();
    public void OpenVoiceOptions() => VoiceSelect.Click();

    public void ClickDropdownOption(string text)
    {
        var option = GetDropdownOption(text);
        option!.Click();
    }

    public void SelectProvider(string provider)
    {
        OpenProviderOptions();
        GetDropdownOption(provider)!.Click();
    }

    public void SelectLanguage(string language)
    {
        OpenLanguageOptions();
        GetDropdownOption(language)!.Click();
    }

    public void SelectVoice(string voice)
    {
        OpenVoiceOptions();
        GetDropdownOption(voice)!.Click();
    }

    public void TypeSampleText()
    {
        SampleTextArea.Clear();
        SampleTextArea.SendKeys(TestData.TtsSampleRequest);
    }

    public void SelectAudioFormat(string format)
    {
        var button = _driver.FindElement(By.XPath($"//button[normalize-space()='{format}']"));
        button.Click();
    }

    public void UploadFile(string filePath)
    {
        FileInput.SendKeys(filePath);
    }

    public void ClickSubmit() => SubmitButton.Click();
    public void ClickClear() => ClearButton.Click();
    public void ClickDownload() => DownloadButton.Click();
    public void ClickPlayButton() => SamplePlayButton!.Click();

    public void RemoveUploadedFile()
    {
        var remove = _driver.FindElements(By.XPath("//div[contains(@class,'file-info')]//button")).FirstOrDefault();
        remove?.Click();
        // Wait until file info gone
        _wait.Until(d => d.FindElements(By.CssSelector(Selectors.FileInfoName)).Count == 0);
    }

    private IWebElement? GetDropdownOption(string buttonText)
    {
        return _wait.UntilVisibleAndEnabled(By.XPath($"//mat-option/span[contains(text(), '{buttonText}')]"));
    }
}
