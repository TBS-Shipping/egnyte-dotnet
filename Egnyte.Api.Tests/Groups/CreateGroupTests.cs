﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Egnyte.Api.Tests.Groups
{
    [TestFixture]
    public class CreateGroupTests
    {
        const string CreateGroupResponseContent = @"
            {
                ""schemas"": [""urn:scim:schemas:core:1.0""],
                ""id"":""e3ba9d90-ebc7-483e-abaa-a84e92480c86"",
                ""displayName"":""Finance"",
                ""members"":
                [
                    {
                        ""username"":""test"",
                        ""value"":9967960066,
                        ""display"":""Test User""
                    },
                    {
                        ""username"":""jdoe"",
                        ""value"": 9967960068,
                        ""display"":""John Doe""
                    }
                ]
            }";

        const string CreateGroupRequestContent = @"
            {
                ""displayName"":""Finance"",
                ""members"":
                [
                    {""value"":9967960066},
                    {""value"":9967960068}
                ]
            }";

        const string CreateGroupWithoutMembersResponseContent = @"
            {
                ""schemas"": [""urn:scim:schemas:core:1.0""],
                ""id"":""e3ba9d90-ebc7-483e-abaa-a84e92480c86"",
                ""displayName"":""Finance"",
                ""members"": []
            }";

        const string CreateGroupWithoutMembersRequestContent = @"
            {
                ""displayName"":""Finance"",
                ""members"": []
            }";

        [Test]
        public async Task CreateGroup_ReturnsSuccess()
        {
            var httpHandlerMock = new HttpMessageHandlerMock();
            var httpClient = new HttpClient(httpHandlerMock);

            httpHandlerMock.SendAsyncFunc =
                (request, cancellationToken) =>
                Task.FromResult(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(
                                CreateGroupResponseContent,
                                Encoding.UTF8,
                                "application/json")
                    });

            var egnyteClient = new EgnyteClient("token", "acme", httpClient);
            var userList = await egnyteClient.Groups.CreateGroup("Finance",
                new List<long> { 9967960066, 9967960068 });

            var requestMessage = httpHandlerMock.GetHttpRequestMessage();
            Assert.AreEqual(
                "https://acme.egnyte.com/pubapi/v2/groups",
                requestMessage.RequestUri.ToString());
            Assert.AreEqual(HttpMethod.Post, requestMessage.Method);

            var content = httpHandlerMock.GetRequestContentAsString();
            Assert.AreEqual(
                TestsHelper.RemoveWhitespaces(CreateGroupRequestContent),
                TestsHelper.RemoveWhitespaces(content));

            Assert.AreEqual(1, userList.Schemas.Count);
            Assert.AreEqual("urn:scim:schemas:core:1.0", userList.Schemas[0]);
            Assert.AreEqual("e3ba9d90-ebc7-483e-abaa-a84e92480c86", userList.Id);
            Assert.AreEqual("Finance", userList.DisplayName);
            Assert.AreEqual(2, userList.Members.Count);

            var firstMember = userList.Members.FirstOrDefault(
                u => u.Value == 9967960066);
            Assert.IsNotNull(firstMember);
            Assert.AreEqual("test", firstMember.Username);
            Assert.AreEqual("Test User", firstMember.Display);

            var secondMember = userList.Members.FirstOrDefault(
                u => u.Value == 9967960068);
            Assert.IsNotNull(secondMember);
            Assert.AreEqual("jdoe", secondMember.Username);
            Assert.AreEqual("John Doe", secondMember.Display);
        }

        [Test]
        public async Task CreateGroup_WithoutMembers_ReturnsSuccess()
        {
            var httpHandlerMock = new HttpMessageHandlerMock();
            var httpClient = new HttpClient(httpHandlerMock);

            httpHandlerMock.SendAsyncFunc =
                (request, cancellationToken) =>
                Task.FromResult(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(
                                CreateGroupWithoutMembersResponseContent,
                                Encoding.UTF8,
                                "application/json")
                    });

            var egnyteClient = new EgnyteClient("token", "acme", httpClient);
            var userList = await egnyteClient.Groups.CreateGroup("Finance", new List<long>());

            var requestMessage = httpHandlerMock.GetHttpRequestMessage();
            Assert.AreEqual(
                "https://acme.egnyte.com/pubapi/v2/groups",
                requestMessage.RequestUri.ToString());
            Assert.AreEqual(HttpMethod.Post, requestMessage.Method);

            var content = httpHandlerMock.GetRequestContentAsString();
            Assert.AreEqual(
                TestsHelper.RemoveWhitespaces(CreateGroupWithoutMembersRequestContent),
                TestsHelper.RemoveWhitespaces(content));

            Assert.AreEqual(1, userList.Schemas.Count);
            Assert.AreEqual("urn:scim:schemas:core:1.0", userList.Schemas[0]);
            Assert.AreEqual("e3ba9d90-ebc7-483e-abaa-a84e92480c86", userList.Id);
            Assert.AreEqual("Finance", userList.DisplayName);
            Assert.AreEqual(0, userList.Members.Count);
        }

        [Test]
        public async Task CreateGroup_WhenDisplayNameEmpty_ThrowsException()
        {
            var httpClient = new HttpClient(new HttpMessageHandlerMock());
            var egnyteClient = new EgnyteClient("token", "acme", httpClient);

            var exception = await AssertExtensions.ThrowsAsync<ArgumentNullException>(
                            () => egnyteClient.Groups.CreateGroup(string.Empty, new List<long>()));

            Assert.IsTrue(exception.Message.Contains("displayName"));
            Assert.IsNull(exception.InnerException);
        }

        [Test]
        public async Task CreateGroup_WhenMembersAreNull_ThrowsException()
        {
            var httpClient = new HttpClient(new HttpMessageHandlerMock());
            var egnyteClient = new EgnyteClient("token", "acme", httpClient);

            var exception = await AssertExtensions.ThrowsAsync<ArgumentNullException>(
                            () => egnyteClient.Groups.CreateGroup("name", null));

            Assert.IsTrue(exception.Message.Contains("members"));
            Assert.IsNull(exception.InnerException);
        }
    }
}
