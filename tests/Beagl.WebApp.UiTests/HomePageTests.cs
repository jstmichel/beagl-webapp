using Microsoft.Playwright;

namespace Beagl.WebApp.UiTests;

public class HomePageTests
{
    [Fact]
    [Trait("Category", "UI")]
    public async Task HomePage_ShouldContainLocalizedHomeKicker()
    {
        using IPlaywright playwright = await Playwright.CreateAsync();
        await using IBrowser browser = await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        IPage page = await browser.NewPageAsync();
        await page.GotoAsync("http://localhost:5129");

        // Assert the page shows localized home content in either supported language.
        string content = await page.ContentAsync();
        bool hasEnglishKicker = content.Contains("Beagl control center", StringComparison.OrdinalIgnoreCase);
        bool hasFrenchKicker = content.Contains("Centre de contrôle Beagl", StringComparison.OrdinalIgnoreCase);

        Assert.True(hasEnglishKicker || hasFrenchKicker);
    }
}
