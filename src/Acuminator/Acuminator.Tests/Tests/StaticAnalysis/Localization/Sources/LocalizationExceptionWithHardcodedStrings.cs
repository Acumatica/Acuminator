using PX.Common;
using PX.Data;

namespace Acuminator.Tests.Sources
{
    public class LocalizationExceptions
    {
        public void ExceptionsLocalization(bool condition)
        {
            PXException e = new PXException("Usernames cannot contain spaces.");

            if (condition)
            {
                throw e;
            }

            throw new PXArgumentException(nameof(ExceptionsLocalization), "Usernames cannot contain commas.");
        }

		public void CheckUserName_Bad(string userName)
		{
			PXException e = new PXException("Username {0} starts with PX prefix.", userName);

			if (userName.StartsWith("PX"))
			{
				throw e;
			}

			if (userName.Contains(","))
				throw new PXArgumentException(nameof(ExceptionsLocalization), "Username {0} contains comma.", userName);
		}

		public void CheckUserName_StringFormatBad(string userName)
		{
			PXException e = new PXException(string.Format(Messages.ErrorPXPrefixFormatMsg, userName));

			if (userName.StartsWith("PX"))
			{
				throw e;
			}

			if (userName.Contains(","))
				throw new PXArgumentException(nameof(ExceptionsLocalization), string.Format(Messages.ErrorCommaFormatMsg, userName));
		}

		public void CheckUserName_Good(string userName)
		{
			PXException e = new PXException(Messages.ErrorPXPrefixFormatMsg, userName);

			if (userName.StartsWith("PX"))
			{
				throw e;
			}

			if (userName.Contains(","))
				throw new PXArgumentException(nameof(ExceptionsLocalization), Messages.ErrorCommaFormatMsg, userName);
		}		
	}

	// Acuminator disable once PX1063 ExceptionWithoutSerializationConstructor [Justification]
	// Acuminator disable once PX1064 ExceptionWithNewFieldsAndNoGetObjectDataOverride [Justification]
	public class DetailNonLocalizableBypassedException : PXException
    {
        public object ItemToBypass { get; }
        public DetailNonLocalizableBypassedException(object itemToBypass)
            : base("Hardcoded String, Ups!")
        {
            ItemToBypass = itemToBypass;
        }

		public DetailNonLocalizableBypassedException(string userName)
			: base(string.Format(Messages.ErrorCommaFormatMsg, userName))
		{
			ItemToBypass = userName;
		}
	}

	[PXLocalizable]
	public static class Messages
	{
		public const string ErrorPXPrefixFormatMsg = "Username {0} starts with PX prefix.";
		public const string ErrorCommaFormatMsg = "Username {0} contains comma.";
	}
}
