using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Moq;
using Moq.Protected; // Crucial for mocking protected HttpMessageHandler.SendAsync
using WijkMeld.App.Model;
using WijkMeld.App.Services;
using WijkMeld.App.ViewModels;
using Xunit;

namespace WijkMeld.Tests
{
    public class RegisterViewModelTests
    {
        // Helper method to create a mocked HttpClient with a specific response
        // It now returns a tuple containing both the HttpClient and its associated Mock<HttpMessageHandler>.
        private static (HttpClient Client, Mock<HttpMessageHandler> Handler) CreateMockHttpClient(HttpResponseMessage response)
        {
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            // Setup the protected SendAsync method using Moq.Protected
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHandler.Object)
            {
                // Set a dummy base address if your AuthenticationService expects one
                BaseAddress = new Uri("http://dummyapi.com/")
            };

            return (httpClient, mockHandler); // Return both the client and its handler mock
        }

        // Test case: Successful registration
        [Fact]
        public async Task RegisterAsync_SuccessfulRegistration_NavigatesToLogin()
        {
            // Arrange


           
          
            var successHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("") // Empty content for success
            };

           
            var (mockHttpClient, mockHandler) = CreateMockHttpClient(successHttpResponse);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient("ApiClient")).Returns(mockHttpClient);


            // Create a concrete instance of AuthenticationService, injecting the mocked IHttpClientFactory
            var authenticationService = new AuthenticationService(httpClientFactoryMock.Object);

            // Mock NavigationService
            var navigationServiceMock = new Mock<INavigationService>(); 
            navigationServiceMock.Setup(s => s.GoToAsync("//login"))
                                 .Returns(Task.CompletedTask)
                                 .Verifiable();


            var viewModel = new RegisterViewModel(
                authenticationService, // Pass the concrete AuthenticationService instance
                navigationServiceMock.Object
            )
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserName = "TestUser"
            };

            // Act
            await viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            
            Assert.False(viewModel.IsBusy);
            Assert.Equal(string.Empty, viewModel.ErrorMessage);
            navigationServiceMock.Verify(s => s.GoToAsync("//login"), Times.Once());

            // Corrected: Verify that the protected SendAsync method on the HttpMessageHandler was called
            // Now 'mockHandler' is correctly in scope due to destructuring
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(), // Ensure it was called exactly once
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri != null && req.RequestUri.ToString().Contains("api/Users") &&
                    req.Content is JsonContent), 
                ItExpr.IsAny<CancellationToken>()
            );
        }

        // Test case: Invalid input - Empty fields
        [Fact]
        public async Task RegisterAsync_InvalidInputEmptyFields_SetsErrorMessageAndDoesNotCallServices()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            // When services are NOT called, the actual HttpClient won't be used,
            // so we don't strictly need to setup its behavior, but we need the objects for the constructor.
            var (mockHttpClient, mockHandler) = CreateMockHttpClient(new HttpResponseMessage()); // Dummy response for setup
            httpClientFactoryMock.Setup(f => f.CreateClient("ApiClient")).Returns(mockHttpClient);

            var authenticationService = new AuthenticationService(httpClientFactoryMock.Object);
            var navigationServiceMock = new Mock<INavigationService>();

            var viewModel = new RegisterViewModel(
                authenticationService,
                navigationServiceMock.Object
            )
            {
                Email = "", // Invalid: Empty Email
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserName = "TestUser"
            };

            // Act
            await viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Vul alle verplichte velden in.", viewModel.ErrorMessage);
            Assert.False(viewModel.IsBusy);

            // Verify services were NOT called
          
            mockHandler.Protected().Verify( // No SendAsync should have been called
                "SendAsync", Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()
            );
            navigationServiceMock.Verify(s => s.GoToAsync(It.IsAny<string>()), Times.Never());
        }

        // Test case: Invalid input - Passwords do not match
        [Fact]
        public async Task RegisterAsync_PasswordsDoNotMatch_SetsErrorMessageAndDoesNotCallServices()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var (mockHttpClient, mockHandler) = CreateMockHttpClient(new HttpResponseMessage()); // Dummy response
            httpClientFactoryMock.Setup(f => f.CreateClient("ApiClient")).Returns(mockHttpClient);

            var authenticationService = new AuthenticationService(httpClientFactoryMock.Object);
            var navigationServiceMock = new Mock<INavigationService>();

            var viewModel = new RegisterViewModel(
                authenticationService,
                navigationServiceMock.Object
            )
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "DifferentPassword!", // Invalid: Passwords don't match
                UserName = "TestUser"
            };

            // Act
            await viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Wachtwoorden komen niet overeen.", viewModel.ErrorMessage);
            Assert.False(viewModel.IsBusy);

            // Verify services were not called
           
            mockHandler.Protected().Verify(
                "SendAsync", Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()
            );
            navigationServiceMock.Verify(s => s.GoToAsync(It.IsAny<string>()), Times.Never());
        }

        // Test case: AuthenticationService returns false (registration failed due to API)
        [Fact]
        public async Task RegisterAsync_ApiReturnsFailure_SetsErrorMessage()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();

            // Simulate any non-success HTTP status code from the API (e.g., 400 Bad Request)
            var failureHttpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"message\":\"Some API error details\"}") // Content won't be used by VM for error message
            };
            var (mockHttpClient, mockHandler) = CreateMockHttpClient(failureHttpResponse);
            httpClientFactoryMock.Setup(f => f.CreateClient("ApiClient")).Returns(mockHttpClient);

            var authenticationService = new AuthenticationService(httpClientFactoryMock.Object);
            var navigationServiceMock = new Mock<INavigationService>();

            var viewModel = new RegisterViewModel(
                authenticationService,
                navigationServiceMock.Object
            )
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserName = "TestUser"
            };

            // Act
            await viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            // Now, assert for the specific generic message the ViewModel sets
            Assert.Equal("Registratie mislukt. Probeer het opnieuw.", viewModel.ErrorMessage);
            Assert.False(viewModel.IsBusy);

            // Navigation should NOT have occurred
            navigationServiceMock.Verify(s => s.GoToAsync(It.IsAny<string>()), Times.Never());

            // Verify that the HTTP client call was made
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri != null && req.RequestUri.ToString().Contains("api/Users")),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        // Test case: Exception occurs during registration (e.g., network error)
        [Fact]
        public async Task RegisterAsync_ExceptionOccurs_SetsErrorMessage()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();

            // Setup the HttpMessageHandler mock to throw an exception when SendAsync is called.
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Simulated network error during API call")); // Simulate network error

            var mockHttpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("http://dummyapi.com/")
            };
            httpClientFactoryMock.Setup(f => f.CreateClient("ApiClient")).Returns(mockHttpClient);

            var authenticationService = new AuthenticationService(httpClientFactoryMock.Object);
            var navigationServiceMock = new Mock<INavigationService>();

            var viewModel = new RegisterViewModel(
                authenticationService,
                navigationServiceMock.Object
            )
            {
                Email = "error@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserName = "ErrorUser"
            };

            // Act
            await viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.Contains("Er is een fout opgetreden: Simulated network error during API call", viewModel.ErrorMessage);
            Assert.False(viewModel.IsBusy);

            // Navigation should NOT have occurred
            navigationServiceMock.Verify(s => s.GoToAsync(It.IsAny<string>()), Times.Never());

            // Verify HTTP client call was attempted
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()
            );
        }

        // Test case: CanRegister method logic
        [Theory]
        [InlineData("test@example.com", "Password123!", "Password123!", "User1", false, true)] // Valid input
        [InlineData("", "Password123!", "Password123!", "User1", false, false)] // Empty email
        [InlineData("test@example.com", "", "Password123!", "User1", false, false)] // Empty password
        [InlineData("test@example.com", "Password123!", "", "User1", false, false)] // Empty confirm password
        [InlineData("test@example.com", "Password123!", "Password123!", "", false, false)] // Empty username
        [InlineData("test@example.com", "Password123!", "WrongPassword!", "User1", false, false)] // Passwords mismatch
        [InlineData("test@example.com", "Password123!", "Password123!", "User1", true, false)] // IsBusy is true
        public void CanRegister_ReturnsExpectedResult(string email, string password, string confirmPassword, string userName, bool isBusy, bool expected)
        {
            // Arrange
            // Even for this test, we need to satisfy the ViewModel's constructor.
            // We can provide minimal mocks as the service methods aren't called.
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var authenticationService = new AuthenticationService(httpClientFactoryMock.Object);
            var navigationServiceMock = new Mock<INavigationService>();

            var viewModel = new RegisterViewModel(
                authenticationService,
                navigationServiceMock.Object
            )
            {
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword,
                UserName = userName,
                IsBusy = isBusy // This property is directly manipulated by the InlineData
            };

            // Act
            var actual = viewModel.RegisterCommand.CanExecute(null);

            // Assert
            Assert.Equal(expected, actual);

            // Verify no service methods were called
            //httpClientFactoryMock.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never());
            navigationServiceMock.Verify(s => s.GoToAsync(It.IsAny<string>()), Times.Never());
        }
    }
}