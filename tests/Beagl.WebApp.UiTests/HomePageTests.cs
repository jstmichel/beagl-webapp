using Microsoft.Playwright;

namespace Beagl.WebApp.UiTests;

public class HomePageTests
{
    [Fact]
    [Trait("Category", "UI")]
    public async Task HomePage_ShouldContainWelcomeText()
    {
        using IPlaywright playwright = await Playwright.CreateAsync();
        await using IBrowser browser = await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        IPage page = await browser.NewPageAsync();
        await page.GotoAsync("http://localhost:5129");

        // Assert the text is present anywhere on the page
        string content = await page.ContentAsync();
        Assert.Contains("Welcome to your new app.", content, StringComparison.OrdinalIgnoreCase);
    }
}
