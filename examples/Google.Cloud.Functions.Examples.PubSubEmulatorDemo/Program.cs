// Copyright 2021, Google LLC
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

using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using System;
using System.Threading;

namespace Google.Cloud.Functions.Examples.PubSubEmulatorDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Expected arguments: <topic-name> <push-url> <message count>");
                return;
            }

            string topicName = args[0];
            string pushUrl = args[1];
            int messageCount = int.Parse(args[2]);

            // Create the clients.
            var publisherClient = new PublisherServiceApiClientBuilder
            {
                EmulatorDetection = EmulatorDetection.EmulatorOnly
            }.Build();

            var subscriberClient = new SubscriberServiceApiClientBuilder
            {
                EmulatorDetection = EmulatorDetection.EmulatorOnly
            }.Build();

            // Create the topic and subscription.
            var topic = publisherClient.CreateTopic(topicName);
            var subscription = new Subscription
            {
                TopicAsTopicName = topic.TopicName,
                PushConfig = new PushConfig { PushEndpoint = pushUrl }
            };
            subscriberClient.CreateSubscription(subscription);

            // Publish the messages, one per batch, waiting a second between each message.
            for (int i = 0; i < messageCount; i++)
            {
                var batch = new[]
                {
                    new PubsubMessage { Data = ByteString.CopyFromUtf8($"Message {i}") }
                };
                publisherClient.Publish(topic.TopicName, batch);
                Thread.Sleep(1000);
            }
        }
    }
}
