using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace PlaywrightScraper
{
    [TestFixture]
    public class Program
    {
        private IPlaywright playwright;
        private IBrowser browser;
        private IPage page;

        [SetUp]
        public async Task SetUp()
        {
            // Browser Management Configuration
            playwright = await Playwright.CreateAsync();
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                SlowMo = 50
            });
            // Open new tab
            page = await browser.NewPageAsync();

            // Clear cookies and cache
            await page.Context.ClearCookiesAsync();
            await page.Context.ClearPermissionsAsync();

            await page.GotoAsync("https://www.rockharborlodge.com/lodging/rock-harbor-lodge/#rooms", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        }

        [TearDown]
        public async Task TearDown()
        {
            await browser.CloseAsync();
        }

        [Test]
        public async Task CheckRockHarbor()
        {
            await SelectLodgeOption("Rock Harbor Lodge");

            // Enter number of Adults
            var adultsDropdown = await page.WaitForSelectorAsync("select[id='container-widget-hero_Adults']");
            Assert.IsNotNull(adultsDropdown, "Adult Dropdown not found");
            await adultsDropdown.ClickAsync();
            await adultsDropdown.SelectOptionAsync("1");

            // Enter number of Children
            var childrenDropdown = await page.WaitForSelectorAsync("select[id='container-widget-hero_Children']");
            Assert.IsNotNull(childrenDropdown, "Children Dropdown not found");
            await childrenDropdown.ClickAsync();
            await childrenDropdown.SelectOptionAsync("0");

            // Click on calendar
            var calendarDropdown = await page.WaitForSelectorAsync("//*[@id=\"container-widget-hero\"]/form/div[8]/div/div");
            Assert.IsNotNull(calendarDropdown, "Calendar Dropdown not found");
            await calendarDropdown.ClickAsync();

            for (int i = 0; i < 8; i++)
            {
                //await Task.Delay(2000);
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await CheckEachCellInTable("Rock Harbor");
                var NextMonthCalendarButton = await page.WaitForSelectorAsync("//*[@id=\"ui-datepicker-div\"]/div[1]/a[2]");
                Assert.IsNotNull(NextMonthCalendarButton, "Next Month button not found");
                await NextMonthCalendarButton.ClickAsync();
            }
        }

        [Test]
        public async Task CheckWashingtonHarbor()
        {
            await SelectLodgeOption("Washington Harbor Camper Cabins");

            // Enter number of Adults
            var adultsDropdown = await page.WaitForSelectorAsync("select[id='container-widget-hero_Adults']");
            Assert.IsNotNull(adultsDropdown, "Adult Dropdown not found");
            await adultsDropdown.ClickAsync();
            await adultsDropdown.SelectOptionAsync("1");

            // Enter number of Children
            var childrenDropdown = await page.WaitForSelectorAsync("select[id='container-widget-hero_Children']");
            Assert.IsNotNull(childrenDropdown, "Children Dropdown not found");
            await childrenDropdown.ClickAsync();
            await childrenDropdown.SelectOptionAsync("0");

            // Click on calendar
            var calendarDropdown = await page.WaitForSelectorAsync("//*[@id=\"container-widget-hero\"]/form/div[8]/div/div");
            Assert.IsNotNull(calendarDropdown, "Calendar Dropdown not found");
            await calendarDropdown.ClickAsync();

            for (int i = 0; i < 8; i++)
            {
                //await Task.Delay(5000);
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);                
                await CheckEachCellInTable("Washington Harbor");
                var NextMonthCalendarButton = await page.WaitForSelectorAsync("//*[@id=\"ui-datepicker-div\"]/div[1]/a[2]");
                Assert.IsNotNull(NextMonthCalendarButton, "Next Month button not found");
                await NextMonthCalendarButton.ClickAsync();
            }
        }

        public async Task SelectLodgeOption(string location)
        {
            // Lodging dropdown
            var lodgingDropdown = await page.WaitForSelectorAsync("select[id='container-widget-hero_InitialProductSelection']");
            Assert.IsNotNull(lodgingDropdown, $"{location} not found in lodging dropdown");

            await lodgingDropdown.ClickAsync();
            Debug.WriteLine("Lodging dropdown clicked");

            // Click on Rock Harbor Lodge
            await lodgingDropdown.SelectOptionAsync(location);
        }

        public async Task CheckEachCellInTable(string location)
        {
            // Wait for the month dropdown to be present
            var monthDropdown = await page.WaitForSelectorAsync("//*[@id='ui-datepicker-div']/div[1]/div/select[1]");
            Assert.IsNotNull(monthDropdown, "Month dropdown not found");

            // Get the selected month
            var selectedMonth = await monthDropdown.EvaluateAsync<string>("el => el.value");

            switch (selectedMonth)
            {
                case "0":
                    selectedMonth = "Jan";
                    break;
                case "1":
                    selectedMonth = "Feb";
                    break;
                case "2":
                    selectedMonth = "Mar";
                    break;
                case "3":
                    selectedMonth = "Apr";
                    break;
                case "4":
                    selectedMonth = "May";
                    break;
                case "5":
                    selectedMonth = "Jun";
                    break;
                case "6":
                    selectedMonth = "Jul";
                    break;
                case "7":
                    selectedMonth = "Aug";
                    break;
                case "8":
                    selectedMonth = "Sep";
                    break;
                case "9":
                    selectedMonth = "Oct";
                    break;
                case "10":
                    selectedMonth = "Nov";
                    break;
                case "11":
                    selectedMonth = "Nov";
                    break;
            }

            // Wait for the table body to be present
            var tableBody = await page.WaitForSelectorAsync("//*[@id='ui-datepicker-div']/table/tbody", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });

            Assert.IsNotNull(tableBody, "Table body not found");

            // Get all the rows in the table body
            var rows = await page.QuerySelectorAllAsync("//*[@id='ui-datepicker-div']/table/tbody/tr");

            foreach (var row in rows)
            {
                // Get all the cells in the current row
                var cells = await row.QuerySelectorAllAsync("td");

                foreach (var cell in cells)
                {
                    // Check if the cell contains an <a> tag
                    var aTag = await cell.QuerySelectorAsync("a");

                    if (aTag != null)
                    {
                        // Get the day from the cell
                        var day = await aTag.TextContentAsync();
                        Console.WriteLine($"{location} on {selectedMonth } {day} is available");

                        // Fail the test with an error message
                        Assert.Fail($"{location} on {selectedMonth} {day} is available");
                    }
                }
            }
        }
    }
}