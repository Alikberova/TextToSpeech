using TextToSpeech.Core.Config;
using TextToSpeech.SeleniumTests.Pages;
using Xunit.Abstractions;
using static TextToSpeech.Infra.TestData;

namespace TextToSpeech.SeleniumTests.Unit;

public sealed class TtsFormTests(ITestOutputHelper output) : TestBase(output)
{
    private TtsFormPage CreatePage() => new(Driver, Wait);

    [Fact]
    public void Initial_State_Has_Disabled_Voice_And_Hidden_Download()
    {
        var page = CreatePage();
        Assert.True(page.IsVoiceDisabled());
        // Download button starts hidden/disabled when no result
        Assert.True(page.IsDownloadHiddenOrDisabled());
    }

    [Fact]
    public void Selecting_OpenAI_Enables_Voice_And_Shows_Model()
    {
        var page = CreatePage();
        page.SelectProvider("OpenAI");
        Assert.False(page.IsVoiceDisabled());
        Assert.True(page.IsModelEnabled());
    }

    [Fact]
    public void Selecting_Narakeet_Shows_Language_Then_Voice()
    {
        var page = CreatePage();
        page.SelectProvider("Narakeet");
        Assert.True(page.IsLanguageEnabled());

        page.SelectLanguage(TextToSpeechFormConstants.GermanStandard);
        // Then choose known seeded voice 'Hans'
        page.SelectVoice(TextToSpeechFormConstants.NarakeetVoiceHans);
    }

    [Fact]
    public void Language_Toggle_Filters_Voices_And_Enables_VoiceSelect()
    {
        var page = CreatePage();
        page.SelectProvider(SharedConstants.Narakeet);

        // Voice should be disabled until language is selected
        Assert.True(page.IsVoiceDisabled());

        page.SelectLanguage(TextToSpeechFormConstants.GermanStandard);
        page.OpenVoiceOptions();
        // Close dropdown by selecting the same option
        page.ClickDropdownOption(TextToSpeechFormConstants.NarakeetVoiceHans);

        page.SelectLanguage(TextToSpeechFormConstants.English);
        page.OpenVoiceOptions();
        page.ClickDropdownOption(TextToSpeechFormConstants.NarakeetVoiceAmanda);
    }

    [Fact]
    public void Upload_And_Remove_File()
    {
        var page = CreatePage();
        var path = Path.Combine(TestDirectory, "upload.txt");
        File.WriteAllText(path, "abc");
        page.UploadFile(path);
        page.RemoveUploadedFile();
    }

    [Fact]
    public void Submit_With_Missing_Fields_Shows_Error_Banner()
    {
        var page = CreatePage();
        page.ClickSubmit();
        Assert.True(page.IsFormErrorBannerVisible());
    }

    [Fact]
    public void Sample_Play_With_Missing_Fields_Shows_Validation()
    {
        var page = CreatePage();
        page.ClickPlay();
        Assert.True(page.IsFormElementErrorlVisible());
    }

    [Fact]
    public void Sample_Play_Pause_Toggles_Icon()
    {
        var page = CreatePage();

        page.SelectProvider(SharedConstants.OpenAI);
        page.SelectVoice(TextToSpeechFormConstants.OpenAiVoiceAlloy);

        page.TypeSampleText();

        page.ClickPlay();
        Assert.True(page.IsPauseButtonVisible());

        page.ClickPause();
        Assert.True(page.IsPlayButtonVisible());

        page.ClickPlay();
        Assert.True(page.IsPauseButtonVisible());
    }
}

