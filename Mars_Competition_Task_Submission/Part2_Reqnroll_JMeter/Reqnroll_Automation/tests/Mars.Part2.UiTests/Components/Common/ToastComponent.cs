using OpenQA.Selenium;

namespace Mars.Part2.UiTests.Components.Common;

public sealed class ToastComponent : BaseComponent
{
    private static readonly By[] ToastLocators =
    {
        By.CssSelector(".ns-box"),
        By.CssSelector(".toast-message"),
        By.CssSelector(".ui.message")
    };

    public ToastComponent(Models.UiTestContext context) : base(context)
    {
    }

    public string GetLatestMessage()
    {
        var toast = Element.FindVisible("toast message", ToastLocators);
        return toast.Text.Trim();
    }

    public bool TryGetLatestMessage(out string message)
    {
        try
        {
            message = GetLatestMessage();
            return !string.IsNullOrWhiteSpace(message);
        }
        catch (Exception)
        {
            message = string.Empty;
            return false;
        }
    }
}
