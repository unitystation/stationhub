using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Auth;
using Moq;
using Moq.Protected;

namespace UnitystationLauncher.UnitTests;

public static class MocksRepository
{
    public static HttpClient MockHttpClient()
    {
        Mock<HttpMessageHandler> mockHttpMessageHandler = new();

        // Mock the email blacklist response
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Get
                    && request.RequestUri.ToString()
                        .Equals("https://raw.githubusercontent.com/martenson/disposable-email-domains/master/disposable_email_blocklist.conf")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("invalid.com")
            });

        return new HttpClient(mockHttpMessageHandler.Object);
    }

    public static IFirebaseAuthProvider MockFirebaseAuthProvider()
    {
        Mock<IFirebaseAuthProvider> mockFirebaseAuthProvider = new();

        // Returns valid user
        mockFirebaseAuthProvider.Setup(mock =>
            mock.CreateUserWithEmailAndPasswordAsync(It.Is<string>(email => email.Equals(TestConsts.VALID_EMAIL)),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())
        ).ReturnsAsync(new FirebaseAuthLink(mockFirebaseAuthProvider.Object, MockFirebaseAuth(true, TestConsts.VALID_EMAIL)));

        // Returns invalid user
        mockFirebaseAuthProvider.Setup(mock =>
            mock.CreateUserWithEmailAndPasswordAsync(It.Is<string>(email => email.Equals(TestConsts.INVALID_EMAIL)),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())
        ).ReturnsAsync(new FirebaseAuthLink(mockFirebaseAuthProvider.Object, MockFirebaseAuth(false, TestConsts.INVALID_EMAIL)));

        return mockFirebaseAuthProvider.Object;
    }

    public static FirebaseAuth MockFirebaseAuth(bool valid, string email)
    {
        int expirySeconds = 20000;
        return new FirebaseAuth
        {
            FirebaseToken = valid ? TestConsts.VALID_FIREBASE_TOKEN : TestConsts.INVALID_FIREBASE_TOKEN,
            RefreshToken = valid ? TestConsts.VALID_REFRESH_TOKEN : TestConsts.INVALID_REFRESH_TOKEN,
            ExpiresIn = valid ? expirySeconds : 0,
            Created = DateTime.Now,
            User = new User
            {
                LocalId = TestConsts.LOCAL_ID,
                FederatedId = TestConsts.FEDERATED_ID,
                FirstName = TestConsts.FIRST_NAME,
                LastName = TestConsts.LAST_NAME,
                DisplayName = TestConsts.DISPLAY_NAME,
                Email = email,
                IsEmailVerified = true
            }
        };
    }
}