using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communicator.Cloud.CloudFunction.FunctionLibrary;
/// <summary>
/// Interface for Testing
/// </summary>
public interface ICloudFunctionLibrary
{
    /// <summary>
    /// Method for testing Telemetry
    /// </summary>
    Task SendLogAsync(string moduleName, string severity, string message, CancellationToken cancellationToken = default);
}
