/********************************************************************************
 * Filename    = InsightProvider.cs
 * Author      = Soorayanarayanan Ganesh
 * Product     = cloud-function-app
 * Project     = Comm-Uni-Cator
 * Description = Connects to a Gemini model and generates AI response for crashes.
 ********************************************************************************/

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

/// <summary>
/// Object which is the Azure-OpenAI client.
/// Used to provide insights from exceptions.
/// </summary>
public class InsightProvider
{
    /// <summary>
    /// OpenAI client.
    /// </summary>
    private Client _client;

    /// <summary>
    /// ApiKey for connecting to GEMINI.
    /// </summary>
    private string _apiKey;

    /// <summary>
    /// Connection flag to check AI connection.
    /// </summary>
    private bool _connectionFlag;

    /// <summary>
    /// Deployment model available in Google gemini.
    /// </summary>
    private string _deploymentModel = "gemini-2.5-flash";

    /// <summary>
    /// Constructor for Insight Provider.
    /// </summary>
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
    /// <summary>
    /// Function to get the AI insights on crashes and execptions.
    /// </summary>
    /// <param name="exceptionPrompt"> Crash data from the crashhandler. </param>
    /// <returns>AI response string</returns>
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
