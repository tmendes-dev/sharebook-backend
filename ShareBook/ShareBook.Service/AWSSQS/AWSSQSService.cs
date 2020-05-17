﻿using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using ShareBook.Service.AWSSQS.Dto;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service.AWSSQS
{
    public class AWSSQSService : IAWSSQSService
    {
        private readonly AWSSQSSettings _AWSSQSSettings;
        private readonly AmazonSQSClient _amazonSQSClient;

        public AWSSQSService(IOptions<AWSSQSSettings> AWSSQSSettings)
        {
            _AWSSQSSettings = AWSSQSSettings.Value;

            var awsCreds = new BasicAWSCredentials(AWSSQSSettings.Value.AccessKey, AWSSQSSettings.Value.SecretKey);
            _amazonSQSClient = new AmazonSQSClient(awsCreds, Amazon.RegionEndpoint.SAEast1);
        }

        public async Task DeleteNewBookNotifyFromAWSSQSAsync(string receiptHandle)
        {
            var deleteMessageRequest = new DeleteMessageRequest();

            deleteMessageRequest.QueueUrl = _AWSSQSSettings.QueueUrl;
            deleteMessageRequest.ReceiptHandle = receiptHandle + "aaa";

            await _amazonSQSClient.DeleteMessageAsync(deleteMessageRequest);
        }

        public async Task<AWSSQSMessageNewBookNotifyResponse> GetNewBookNotifyFromAWSSQSAsync()
        {
            var receiveMessageRequest = new ReceiveMessageRequest(_AWSSQSSettings.QueueUrl);

            var result = await _amazonSQSClient.ReceiveMessageAsync(receiveMessageRequest);

            if (result.Messages.Count > 0)
            {
                var firstMessageTemp = result.Messages[0].Body;
                var firstMessage = System.Text.Json.JsonSerializer.Deserialize<AWSSQSMessageNewBookNotifyResponse>(firstMessageTemp);
                firstMessage.ReceiptHandle = result.Messages[0].ReceiptHandle;
                return firstMessage;
            }
            else
            {
                return null;
            }
        }

        public async Task SendNewBookNotifyToAWSSQSAsync(AWSSQSMessageNewBookNotifyRequest message)
        {
            var request = new SendMessageRequest
            {
                DelaySeconds = (int)TimeSpan.FromSeconds(5).TotalSeconds,
                MessageBody = System.Text.Json.JsonSerializer.Serialize(message),
                QueueUrl = _AWSSQSSettings.QueueUrl
            };

            await _amazonSQSClient.SendMessageAsync(request);
        }
    }
}