using PX.Common;
using PX.Data;
using PX.Objects.Common.Exceptions;

namespace Acuminator.Tests.Sources
{
    public class LocalizationExceptions
    {
        public void ExceptionsLocalization()
        {
            PXExceptionInfo e = new PXExceptionInfo("Usernames cannot contain spaces.");
            throw new PXArgumentException(nameof(ExceptionsLocalization), e.MessageFormat);
        }

		public void CheckUserName_Bad(string userName)
		{
			PXExceptionInfo e = new PXExceptionInfo(PXErrorLevel.Error, "Username {0} starts with PX prefix.", userName);

			if (userName.Contains(","))
				throw new PXArgumentException(nameof(ExceptionsLocalization), e.MessageFormat, userName);
		}

		public void CheckUserName_StringFormatBad(string userName)
		{
			PXExceptionInfo e = new PXExceptionInfo(string.Format(Messages.ErrorPXPrefixFormatMsg, userName));

			if (userName.Contains(","))
				throw new PXArgumentException(nameof(ExceptionsLocalization), string.Format(e.MessageFormat, e.MessageArguments));
		}

		public void CheckUserName_Good(string userName)
		{
			PXExceptionInfo e = new PXExceptionInfo(Messages.ErrorPXPrefixFormatMsg, userName);

			if (userName.Contains(","))
				throw new PXArgumentException(nameof(ExceptionsLocalization), e.MessageFormat, userName);
		}		
	}

	[PXLocalizable]
	public static class Messages
	{
		public const string ErrorPXPrefixFormatMsg = "Username {0} starts with PX prefix.";
		public const string ErrorCommaFormatMsg = "Username {0} contains comma.";
	}
}
