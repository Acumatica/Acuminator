using PX.Common;

namespace Acuminator.Tests.Sources
{
    [PXLocalizable]
    public class MyMessages
    {
        public const string CommasInUserName = "Usernames cannot contain commas.";
        public const string SomeString = "Some string";
        public const string StringToFormat = "Text with placeholder {0}";
    }

    [PXLocalizable]
    public class ComplexMessages
    {
        public const string DocDiscountExceedLimit = "Total Group and Document discount exceeds limit configured for this customer class ({0:F2}%).";
        public const string DateTimeStr = @"{0:\Year\:YYYY, \Mon\t\h\: MM, Da\y: dd}";
    }

    public static class NonLocalizableMessages
    {
        public const string CommasInUserName = "Usernames cannot contain commas.";
        public const string StringToFormat = "Text with placeholder {0}";
    }
}

namespace InnerNamespace
{
    public static class NonLocalizableMessagesInNamespace
    {
        public const string CommasInUserName = "Usernames cannot contain commas.";
        public const string StringToFormat = "Text with placeholder {0}";
    }
}
