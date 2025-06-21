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
    public class LoginViewModelTests
    {
        // Helper method to create a mocked HttpClient with a specific response
        private static (HttpClient Client, Mock<HttpMessageHandler> Handler) CreateMockHttpClient(HttpResponseMessage response)
        {
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

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
                BaseAddress = new Uri("http://dummyapi.com/") // Set a dummy base address
            };

            return (httpClient, mockHandler);
        }

        // --- Test cases for standard LoginAsync ---

        [Fact]
        public async Task LoginAsync_SuccessfulLogin_NavigatesToHome()
        {
            // Arrange
            
            var authServiceMock = new Mock<IAuthenticationService>();

            
            authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                           .ReturnsAsync(true); 

            var navigationServiceMock = new Mock<INavigationService>();
            navigationServiceMock.Setup(s => s.GoToAsync("//home"))
                                   .Returns(Task.CompletedTask)
                                   .Verifiable();

            
            var viewModel = new LoginViewModel(authServiceMock.Object, navigationServiceMock.Object)
            {
                Email = "test@example.com", 
                Password = "Password123!"
            };

            // Act
            viewModel.LoginCommand.Execute(null);

            // Assert
            Assert.False(viewModel.IsBusy);
            Assert.Equal(string.Empty, viewModel.ErrorMessage); // Expect no error message
            navigationServiceMock.Verify(s => s.GoToAsync("//home"), Times.Once());

            // Verify that LoginAsync was called exactly once with any LoginRequest
            authServiceMock.Verify(s => s.LoginAsync(It.IsAny<LoginRequest>()), Times.Once());

        }

        [Fact]
        public async Task LoginAsync_FailedLogin_SetsErrorMessageAndDoesNotNavigate()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var (mockHttpClient, mockHandler) = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("{\"message\":\"Invalid credentials\"}")
            });
            httpClientFactoryMock.Setup(f => f.CreateClient("ApiClient")).Returns(mockHttpClient);

            var authService = new AuthenticationService(httpClientFactoryMock.Object);
            var navigationServiceMock = new Mock<INavigationService>(); // Mock the interface

            var viewModel = new LoginViewModel(authService, navigationServiceMock.Object)
            {
                Email = "wrong@example.com",
                Password = "wrongpassword"
            };

            // Act
            viewModel.LoginCommand.Execute(null);

            // Assert
            Assert.False(viewModel.IsBusy);
            Assert.Equal("Er is een onverwachte fout opgetreden tijdens het inloggen. Probeer het later opnieuw.", viewModel.ErrorMessage);
            navigationServiceMock.Verify(s => s.GoToAsync(It.IsAny<string>()), Times.Never());
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri != null && req.RequestUri.ToString().Contains("api/Auth/login")),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        //[Fact]
        //public async Task LoginAsync_ExceptionOccurs_SetsErrorMessageAndDoesNotNavigate()
        //{
        //    // Arrange
        //    var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        //    var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        //    mockHandler
        //        .Protected()
        //        .Setup<Task<HttpResponseMessage>>(
        //            "SendAsync",
        //            ItExpr.IsAny<HttpRequestMessage>(),
        //            ItExpr.IsAny<CancellationToken>()
        //        )
        //        .ThrowsAsync(new HttpRequestException("Simulated network error"));

        //    var mockHttpClient = new HttpClient(mockHandler.Object)
        //    {
        //        BaseAddress = new Uri("http://dummyapi.com/")
        //    };
        //    httpClientFactoryMock.Setup(f => f.CreateClient("ApiClient")).Returns(mockHttpClient);

        //    var authService = new AuthenticationService(httpClientFactoryMock.Object);
        //    var navigationServiceMock = new Mock<INavigationService>();

        //    var viewModel = new LoginViewModel(authService, navigationServiceMock.Object)
        //    {
        //        Email = "test@example.com",
        //        Password = "Password123!"
        //    };

        //    // Act
        //    viewModel.LoginCommand.Execute(null);

        //    // Assert
        //    Assert.False(viewModel.IsBusy);
        //    Assert.Equal("Er is een onverwachte fout opgetreden tijdens het inloggen. Probeer het later opnieuw.", viewModel.ErrorMessage); // ViewModel catches and sets generic message
        //    navigationServiceMock.Verify(s => s.GoToAsync(It.IsAny<string>()), Times.Never());
        //    mockHandler.Protected().Verify(
        //        "SendAsync",
        //        Times.Once(),
        //        ItExpr.IsAny<HttpRequestMessage>(),
        //        ItExpr.IsAny<CancellationToken>()
        //    );
        //}

        // --- Test cases for LoginAnonymouslyAsync ---

        [Fact]
        public async Task LoginAnonymouslyAsync_SuccessfulLogin_NavigatesToHome()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthenticationService>();

            authServiceMock.Setup(s => s.LoginAsGuestAsync())
                   .ReturnsAsync(true);

            var navigationServiceMock = new Mock<INavigationService>();

            var viewModel = new LoginViewModel(authServiceMock.Object, navigationServiceMock.Object);

            // Act
            await viewModel.LoginAnonymouslyCommand.ExecuteAsync(null);

            // Assert
            Assert.False(viewModel.IsBusy);
            Assert.Equal(string.Empty, viewModel.ErrorMessage);
            navigationServiceMock.Verify(s => s.GoToAsync("//home"), Times.Once());
        
        }

        [Fact]
        public async Task LoginAnonymouslyAsync_FailedLogin_SetsErrorMessageAndDoesNotNavigate()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var (mockHttpClient, mockHandler) = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.Forbidden)); // Simulate failed guest login
            httpClientFactoryMock.Setup(f => f.CreateClient("ApiClient")).Returns(mockHttpClient);

            var authService = new AuthenticationService(httpClientFactoryMock.Object);
            var navigationServiceMock = new Mock<INavigationService>();

            var viewModel = new LoginViewModel(authService, navigationServiceMock.Object);

            // Act
            await viewModel.LoginAnonymouslyCommand.ExecuteAsync(null);

            // Assert
            Assert.False(viewModel.IsBusy);
            Assert.Equal("Er is een fout opgetreden bij anoniem melden", viewModel.ErrorMessage);
            navigationServiceMock.Verify(s => s.GoToAsync(It.IsAny<string>()), Times.Never());
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task LoginAnonymouslyAsync_ExceptionOccurs_SetsErrorMessageAndDoesNotNavigate()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthenticationService>();

            authServiceMock.Setup(s => s.LoginAsGuestAsync())
                   .ThrowsAsync(new Exception("Simulated anonymous login failure."));

            ;

            var navigationServiceMock = new Mock<INavigationService>();

            var viewModel = new LoginViewModel(authServiceMock.Object, navigationServiceMock.Object);

            // Act
            await viewModel.LoginAnonymouslyCommand.ExecuteAsync(null);

            // Assert
            Assert.False(viewModel.IsBusy);
            Assert.Contains("Er is een fout opgetreden bij anoniem melden", viewModel.ErrorMessage);
            navigationServiceMock.Verify(s => s.GoToAsync(It.IsAny<string>()), Times.Never());

        }

        // --- Test cases for NavigateToRegisterAsync ---

        [Fact]
        public async Task NavigateToRegisterAsync_NavigatesToRegisterView()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>(); // Still needed for authService constructor
            var authService = new AuthenticationService(httpClientFactoryMock.Object);
            var navigationServiceMock = new Mock<INavigationService>();
            navigationServiceMock.Setup(s => s.GoToAsync("//register"))
                                 .Returns(Task.CompletedTask)
                                 .Verifiable();

            var viewModel = new LoginViewModel(authService, navigationServiceMock.Object);

            // Act
            await viewModel.NavigateToRegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.False(viewModel.IsBusy); // Should be false after navigation
            navigationServiceMock.Verify(s => s.GoToAsync("//register"), Times.Once());
        }

        [Fact]
        public async Task NavigateToRegisterAsync_ExceptionDuringNavigation_HandlesError()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var authService = new AuthenticationService(httpClientFactoryMock.Object);
            var navigationServiceMock = new Mock<INavigationService>();
            navigationServiceMock.Setup(s => s.GoToAsync("//register"))
                                 .Throws(new InvalidOperationException("Navigation failed")); // Simulate navigation error

            var viewModel = new LoginViewModel(authService, navigationServiceMock.Object);

            // Act
            await viewModel.NavigateToRegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.False(viewModel.IsBusy); // Should return to false
            // The ViewModel's catch block only debugs, doesn't set ErrorMessage, so we can't assert on it.
            // But we can verify navigation was attempted.
            navigationServiceMock.Verify(s => s.GoToAsync("//register"), Times.Once());
            // Optionally, if you had logging, you'd verify the log entry.
        }

        // --- Test case for CanLogin (if applicable, typically part of LoginCommand) ---

        [Theory]
        [InlineData("test@example.com", "Password123!", false, true)] // Valid input
        [InlineData("", "Password123!", false, false)] // Empty email
        [InlineData("test@example.com", "", false, false)] // Empty password
        [InlineData("test@example.com", "Password123!", true, false)] // IsBusy is true
        public void CanLogin_ReturnsExpectedResult(string email, string password, bool isBusy, bool expected)
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var authService = new AuthenticationService(httpClientFactoryMock.Object);
            var navigationServiceMock = new Mock<INavigationService>();

            var viewModel = new LoginViewModel(authService, navigationServiceMock.Object)
            {
                Email = email,
                Password = password,
                IsBusy = isBusy // Directly manipulate IsBusy for this test
            };

            // Act
            var actual = viewModel.LoginCommand.CanExecute(null);

            // Assert
            Assert.Equal(expected, actual);

            // Verify no service methods were called
            navigationServiceMock.Verify(s => s.GoToAsync(It.IsAny<string>()), Times.Never());
            // No HTTP calls should be made if CanExecute is false, so no need to verify mockHandler
        }
    }
}