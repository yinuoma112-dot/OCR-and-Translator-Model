// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using WindowsAISample.Models.Contracts;

namespace WindowsAISample.ViewModels;

/// <summary>
/// Our root model to expose all the AI Fabric API's
/// </summary>
internal class CopilotRootViewModel
{
    internal CopilotRootViewModel()
    {
        TextRecognizer = new(new Models.TextRecognizerModel());
    }

    public TextRecognizerViewModel TextRecognizer { get; }

}
