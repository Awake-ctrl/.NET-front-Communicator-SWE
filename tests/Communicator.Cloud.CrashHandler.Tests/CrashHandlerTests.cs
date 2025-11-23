// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Communicator.Cloud.CloudFunction.FunctionLibrary;
using Communicator.Cloud.CrashHandler;


namespace Communicator.Cloud.CrashHandlerTests;

public class CrashHandlerTests
{
    CloudFunctionLibrary _cloudFunctionLibrary = new CloudFunctionLibrary();
    [Fact]
    public void TestSingletonCrashHandler()
    {
        CrashHandler.CrashHandler testCrashHandler = new CrashHandler.CrashHandler(_cloudFunctionLibrary);

        testCrashHandler.StartCrashHandler();

        testCrashHandler.StartCrashHandler();

    }

    [Fact]
    public void TestCrashHandler()
    {
        var testCrashHandler = new CrashHandler.CrashHandler(_cloudFunctionLibrary);

        testCrashHandler.StartCrashHandler();

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Exception ex = e.ExceptionObject as Exception;
        };

        var crashingThread = new Thread(() =>
        {
            throw new Exception("Intentional Crashing...");
        }) {
            Name = "CrashingTestThread"
        };

        crashingThread.Start();
        crashingThread.Join();
    }
}
