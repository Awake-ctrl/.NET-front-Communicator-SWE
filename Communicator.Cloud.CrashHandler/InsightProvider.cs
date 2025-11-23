// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.GenAI;

namespace Communicator.Cloud.CrashHandler;
public class InsightProvider
{
    private Client _client;

    private string _apiKey;

    private bool _connectionFlag;

    private string _deploymentModel = "gemini-2.5-flash";

    public InsightProvider()
    {
        try
        {
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (_apiKey == null)
            {
                throw new Exception("APIKEY not found");
            }
            _client = new Client(apiKey: _apiKey);
            _connectionFlag = true;
        }
        catch 
        {
            _connectionFlag = false;
        }
    }

    public async Task<string> GetInsights(string exceptionPrompt)
    {
        Google.GenAI.Types.GenerateContentResponse response;

        try
        {
            if (!_connectionFlag)
            {
                throw new Exception("Connection flag was reset");
            }

            response = await _client.Models.GenerateContentAsync(
                model: _deploymentModel, contents: "Analyze what went wrong:" + exceptionPrompt);

            return response.Candidates[0].Content.Parts[0].Text;
        }
        catch (Exception e)
        {
            return "No respones, NOJOY..." + e.Message;
        }
    }
}
