/******************************************************************************
 * Filename    = ICrashHandler.cs
 * Author      = Soorayanarayanan Ganesh
 * Product     = cloud-function-app
 * Project     = Comm-Uni-Cator
 * Description = Interface for crash handler.
 *****************************************************************************/

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communicator.Cloud.CrashHandler;

/// <summary>
/// Interface providing the function to start the crash handler.
/// </summary>
public interface ICrashHandler
{
    void StartCrashHandler();
}
