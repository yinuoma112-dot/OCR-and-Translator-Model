using AIDevGallery.Sample.Utils;
using Microsoft.Extensions.AI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WindowsAISample.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class TranslatorPage : Page
{
    private const int _defaultMaxLength = 10000;
    private IChatClient? chatClient;
    private CancellationTokenSource? cts;

    public TranslatorPage()
    {
        this.Unloaded += (s, e) => CleanUp();
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        try
        {
            await Task.Delay(300);
            chatClient = await PhiSilicaClient.CreateAsync();
            InputTextBox.MaxLength = _defaultMaxLength;
        }
        catch (Exception ex)
        {
            App.Window?.ShowException(ex);
        }
    }

    private void CleanUp()
    {
        CancelTranslation();
        chatClient?.Dispose();
    }

    public bool IsProgressVisible
    {
        get => isProgressVisible;
        set
        {
            isProgressVisible = value;
            DispatcherQueue.TryEnqueue(() =>
            {
                OutputProgressBar.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                StopIcon.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
            });
        }
    }

    public void TranslateText(string text)
    {
        if (chatClient == null || LanguageBox.SelectedItem == null)
        {
            return;
        }

        if (LanguageBox.SelectedItem is string language)
        {
            TranslatedTextBlock.Text = string.Empty;
            Task.Run(
                async () =>
                {
                    string targetLanguage = language.ToString();
                    string systemPrompt = "You translate user provided text. Do not reply with any extraneous content besides the translated text itself.";
                    string userPrompt = $@"Translate the following text to {targetLanguage}: '{text}'";

                    cts = new CancellationTokenSource();

                    IsProgressVisible = true;

                    await foreach (var messagePart in chatClient.GetStreamingResponseAsync(
                        [
                            new ChatMessage(ChatRole.System, systemPrompt),
                            new ChatMessage(ChatRole.User, userPrompt)
                        ],
                        null,
                        cts.Token))
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            if (isProgressVisible)
                            {
                                IsProgressVisible = false;
                            }

                            TranslatedTextBlock.Text += messagePart;

                        });
                    }

                    cts?.Dispose();
                    cts = null;

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StopBtn.Visibility = Visibility.Collapsed;
                        TranslateButton.Visibility = Visibility.Visible;
                    });
                });
        }
    }
    private void TranslateButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.InputTextBox.Text.Length > 0)
        {
            TranslateButton.Visibility = Visibility.Collapsed;
            IsProgressVisible = true;
            StopBtn.Visibility = Visibility.Visible;
            TranslateText(InputTextBox.Text);
        }
    }

    private void CancelTranslation()
    {
        StopBtn.Visibility = Visibility.Collapsed;
        IsProgressVisible = false;
        TranslateButton.Visibility = Visibility.Visible;
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    private readonly List<string> languages =
    [
       "Afrikaans",
        "Arabic",
        "Czech",
        "Danish",
        "Dutch",
        "English",
        "Filipino",
        "Finnish",
        "French",
        "German",
        "Greek",
        "Hindi",
        "Indonesian",
        "Italian",
        "Japanese",
        "Korean",
        "Mandarin",
        "Polish",
        "Portuguese",
        "Romanian",
        "Russian",
        "Serbian",
        "Slovak",
        "Spanish",
        "Thai",
        "Turkish",
        "Vietnamese"
    ];
    private bool isProgressVisible;

    private void StopBtn_Click(object sender, RoutedEventArgs e)
    {
        CancelTranslation();
    }
    private void InputBox_Changed(object sender, TextChangedEventArgs e)
    {
        var inputLength = InputTextBox.Text.Length;
        if (inputLength > 0)
        {
            if (inputLength >= _defaultMaxLength)
            {
                InputTextBox.Description = $"{inputLength} of {_defaultMaxLength}. Max characters reached.";
            }
            else
            {
                InputTextBox.Description = $"{inputLength} of {_defaultMaxLength}";
            }

            TranslateButton.IsEnabled = inputLength <= _defaultMaxLength;
        }
        else
        {
            InputTextBox.Description = string.Empty;
            TranslateButton.Visibility = Visibility.Visible;
        }
    }
}
