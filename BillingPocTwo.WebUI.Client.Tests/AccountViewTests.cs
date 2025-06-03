using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using RichardSzalay.MockHttp;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BillingPocTwo.WebUI.Client.Pages.Accounts;
using BillingPocTwo.Shared.DataObjects.Billing;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using BillingPocTwo.Shared.Entities.Billing;
using System.Text.Json;

namespace BillingPocTwo.WebUI.Client.Tests
{
    public class AccountViewTests : TestContext
    {
        private void SetupHttpClientFactory(AccountDetailsDto? accountDetails = null, List<TransactionDto>? transactions = null)
        {
            var handler = new MockHttpMessageHandler();

            handler.When("*/api/EntityRegister/details/dashboard/*")
                .Respond("application/json", JsonSerializer.Serialize(accountDetails ?? new AccountDetailsDto { FULL_NAME = "Test Account" }));

            handler.When("*/api/EntityAddress/addresses/*")
                .Respond("application/json", JsonSerializer.Serialize(new List<ENTITY_ADDRESS_INFO>()));

            handler.When("*/api/Transactions/byaccountnumber/*")
                .Respond("application/json", JsonSerializer.Serialize(transactions ?? new List<TransactionDto>()));

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            Services.AddSingleton(factory.Object);
        }

        [Fact]
        public void RendersAccountNameAndTabs()
        {
            // Arrange
            SetupHttpClientFactory();

            // Act
            var cut = RenderComponent<AccountView>(parameters => parameters.Add(p => p.AccountNumber, "123"));

            // Assert
            cut.Markup.Contains("Test Account");
            cut.Find("ul.nav-tabs");
        }

        [Fact]
        public void SwitchesTabsOnClick()
        {
            // Arrange
            SetupHttpClientFactory();

            var cut = RenderComponent<AccountView>(parameters => parameters.Add(p => p.AccountNumber, "123"));

            // Act
            cut.FindAll("a.nav-link")[1].Click(); // Balances tab

            // Wait for the UI to update and assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("active", cut.FindAll("a.nav-link")[1].ClassList.ToString());
            }, timeout: TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void ShowsModalWhenPolicyClicked()
        {
            // Arrange
            var transactions = new List<TransactionDto>
            {
                new TransactionDto { POLICY_NO = "POL123", TRANSACTION_TYPE = "TYPE1", TRANSACTION_EFF_DATE = DateTime.Now }
            };
            SetupHttpClientFactory(transactions: transactions);

            var cut = RenderComponent<AccountView>(parameters => parameters.Add(p => p.AccountNumber, "123"));

            // Wait for the policy link to appear and click the correct link
            cut.WaitForState(() => cut.FindAll("a").Any(a => a.TextContent == "POL123"), timeout: TimeSpan.FromSeconds(5));
            var policyLink = cut.FindAll("a").First(a => a.TextContent == "POL123");
            policyLink.Click();

            // Wait for the modal to appear
            cut.WaitForState(() => cut.Markup.Contains("Policy Details"), timeout: TimeSpan.FromSeconds(5));

            // Assert
            Assert.Contains("Policy Details", cut.Markup);
            Assert.Contains("POL123", cut.Markup);
        }


        [Fact]
        public void NavigatesToPolicyDetailsOnOpenPolicy()
        {
            // Arrange
            var transactions = new List<TransactionDto>
            {
                new TransactionDto { POLICY_NO = "POL123", TRANSACTION_TYPE = "TYPE1", TRANSACTION_EFF_DATE = DateTime.Now }
            };
            SetupHttpClientFactory(transactions: transactions);

            var navMan = Services.GetRequiredService<FakeNavigationManager>();

            var cut = RenderComponent<AccountView>(parameters => parameters.Add(p => p.AccountNumber, "123"));

            // Wait for the policy link to appear and click it
            cut.WaitForState(() => cut.FindAll("a").Any(a => a.TextContent.Contains("POL123")));
            var policyLink = cut.FindAll("a").First(a => a.TextContent.Contains("POL123"));
            policyLink.Click();

            // Wait for the modal and button, then click "Open Policy"
            cut.WaitForState(() => cut.FindAll("button.btn-primary").Any());
            var openPolicyButton = cut.Find("button.btn-primary");
            openPolicyButton.Click();

            // Wait for navigation to occur, with a longer timeout and debug output
            cut.WaitForAssertion(() =>
            {
                Assert.StartsWith("http://localhost/policy-details/POL123", navMan.Uri);
            }, timeout: TimeSpan.FromSeconds(10));
        }

    }
}
