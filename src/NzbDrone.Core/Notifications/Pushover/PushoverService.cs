﻿using System;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common;
using RestSharp;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.Notifications.Pushover
{
    public interface IPushoverProxy
    {
        void SendNotification(string title, string message, string apiKey, string userKey, PushoverPriority priority, string sound);
        ValidationFailure Test(PushoverSettings settings);
    }

    public class PushoverProxy : IPushoverProxy
    {
        private readonly Logger _logger;
        private const string URL = "https://api.pushover.net/1/messages.json";

        public PushoverProxy(Logger logger)
        {
            _logger = logger;
        }

        public void SendNotification(string title, string message, string apiKey, string userKey, PushoverPriority priority, string sound)
        {
            var client = RestClientFactory.BuildClient(URL);
            var request = new RestRequest(Method.POST);
            request.AddParameter("token", apiKey);
            request.AddParameter("user", userKey);
            request.AddParameter("title", title);
            request.AddParameter("message", message);
            request.AddParameter("priority", (int)priority);

            if (!sound.IsNullOrWhiteSpace()) request.AddParameter("sound", sound);


            client.ExecuteAndValidate(request);
        }

        public ValidationFailure Test(PushoverSettings settings)
        {
            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from NzbDrone";

                SendNotification(title, body, settings.ApiKey, settings.UserKey, (PushoverPriority)settings.Priority, settings.Sound);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to send test message: " + ex.Message, ex);
                return new ValidationFailure("ApiKey", "Unable to send test message");
            }

            return null;
        }
    }
}
