/******************************************************************************
 * Filename    = CrashHandler.cs
 * Author      = Soorayanarayanan Ganesh
 * Product     = cloud-function-app
 * Project     = Comm-Uni-Cator
 * Description = Handles crashes and stores exceptions with java crash handler.
 *****************************************************************************/

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Nodes;
using Communicator.Cloud.CloudFunction.DataStructures;
using Communicator.Cloud.CloudFunction.FunctionLibrary;


namespace Communicator.Cloud.CrashHandler;

public class CrashHandler : ICrashHandler
{

    private static bool s_isCreated = false;

    private CloudFunctionLibrary _cloudFunctionLibrary;

    private static string s_collection = "Exception";

    private static int s_exceptionId = 1;

    private static int s_successCode = 200;

    public CrashHandler(CloudFunctionLibrary cloudFunctionLibrary)
    {
        _cloudFunctionLibrary = cloudFunctionLibrary;
    }

    public void StartCrashHandler()
    {
        if (s_isCreated)
        {
            return;
        }

        s_isCreated = true;

        InsightProvider insightProvider = new InsightProvider();

        try
        {
            JsonElement emptyJson = JsonDocument.Parse("{}").RootElement.Clone();

            CloudResponse responseCreate = _cloudFunctionLibrary.CloudCreateAsync(
                new Entity("CLOUD", s_collection, null, null, -1, null, emptyJson)).GetAwaiter().GetResult();

            CloudResponse responseGet = _cloudFunctionLibrary.CloudGetAsync(
                new Entity("CLOUD", s_collection, null, null, 1, null, emptyJson)).GetAwaiter().GetResult();

            if (responseCreate.StatusCode != s_successCode || responseGet.StatusCode != s_successCode)
            {
                throw new Exception("Cloud Error...");
            }

            s_exceptionId = int.Parse(responseGet.Data[0].GetProperty("id").GetString()!);
        }
        catch (Exception e)
        {
            // Do nothing...
        }

        AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
            Exception? exception = e.ExceptionObject as Exception;

            string exceptionName = exception.GetType().FullName;
            string timeStamp = DateTime.UtcNow.ToString();
            string exceptionMessage = exception.Message;
            string exceptionString = exception.ToString();
            string exceptionStackTrace = exception.StackTrace;

            JsonNode exceptionJsonNode = ToJsonNode(exceptionName, timeStamp, exceptionMessage, exceptionString, exceptionStackTrace);
            string jsonString = exceptionJsonNode.ToJsonString();
            using JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement exceptionJsonElement = doc.RootElement.Clone();

            try
            {
                string response = insightProvider.GetInsights(exceptionJsonNode.ToJsonString()).GetAwaiter().GetResult();
                exceptionJsonNode["AIResponse"] = response;
                StoreDataToFile(exceptionJsonNode.ToJsonString());

                Entity exceptionEntity = new Entity(
                    "CLOUD",
                    s_collection,
                    (++s_exceptionId).ToString(),
                    null,
                    -1,
                    null,
                    exceptionJsonElement
                );

                CloudResponse responsePost = _cloudFunctionLibrary.CloudPostAsync(exceptionEntity).GetAwaiter().GetResult();

            }
            catch { }
        };
    }

    private static JsonNode ToJsonNode(string eName, string timeStamp, string eMsg, string eDetails, string eTrace)
    {
        var payload = new {
            ExceptionName = eName,
            TimestampUtc = timeStamp,
            ExceptionMessage = eMsg,
            ExceptionDetails = eDetails,
            StackTrace = eTrace
        };

        JsonNode jsonNode = JsonSerializer.Serialize(payload);

        return jsonNode;
    }

    private void StoreDataToFile(string data)
    {
        File.AppendAllText("exception_log.jsonl", data + Environment.NewLine);
    }
}
