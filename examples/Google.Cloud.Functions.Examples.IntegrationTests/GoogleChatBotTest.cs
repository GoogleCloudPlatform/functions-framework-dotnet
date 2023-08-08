// Copyright 2023, Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Google.Cloud.Functions.Testing;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Google.Cloud.Functions.Examples.IntegrationTests;

public class GoogleChatBotTest
{
    [Fact]
    public async Task CanChat()
    {
        using (var server = new FunctionTestServer<GoogleChatBot.TypedFunction>())
        {
            JToken expected = JToken.Parse(@"{
                cardsV2: [
                    {
                    cardId: ""avatarCard"",
                        card: {
                            name: ""Avatar Card"",
                            header: {
                                title: ""Hello janedoe!"",
                            },
                            sections: [
                                {
                                    widgets: [
                                        {
                                            textParagraph: {
                                                text: ""Your avatar picture: "",
                                            },
                                        },
                                        {
                                            image: {
                                                imageUrl: ""example.com/avatar.png"",
                                            },
                                        },
                                    ],
                                },
                            ],
                        },
                    },
                ],
            }");

            var client = server.CreateClient();
            var content = new StringContent(@"{
                ""message"": {
                    ""sender"": {
                        ""displayName"": ""janedoe"",
                        ""imageUrl"": ""example.com/avatar.png""
                    }
                }
            }", Encoding.UTF8, "application/json");
            var response = await client.PostAsync("sample-path", content);
            response.EnsureSuccessStatusCode();
            JToken actual = JToken.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(JToken.DeepEquals(actual, expected), string.Format("JSON response {0} did not match expected {1}", actual, expected));
        }
    }
}
