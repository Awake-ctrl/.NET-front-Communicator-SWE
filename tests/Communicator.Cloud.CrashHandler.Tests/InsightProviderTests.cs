// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Communicator.Cloud.CloudFunction.FunctionLibrary;
using Communicator.Cloud.CrashHandler;


namespace Communicator.Cloud.CrashHandlerTests;

public class InsightProviderTests
{
    [Fact]
    public void TestInsightProvider()
    {
        InsightProvider insightProvider = new InsightProvider();
        string response = insightProvider.GetInsights("Null data, dont generate anything").GetAwaiter().GetResult();
    }
}
