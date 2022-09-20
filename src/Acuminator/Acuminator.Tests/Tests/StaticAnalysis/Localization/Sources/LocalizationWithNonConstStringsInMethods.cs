using PX.Data;

namespace Acuminator.Tests.Sources
{
    public class LocalizationWithNonConstStringsInMethods
	{
		public string CheckNonConstantMessages()
		{
			string localizedString;
			var messages = new NonConstantMessages();

			localizedString = PXLocalizer.Localize(NonLocalizableMessages.StringProperty);

			localizedString = PXLocalizer.Localize(NonConstantMessages.StaticStringProperty);
			localizedString = PXLocalizer.Localize(messages.InstanceStringProperty);

			localizedString = PXLocalizer.Localize(NonConstantMessages.StaticReadOnlyField);
			localizedString = PXLocalizer.Localize(messages.InstanceReadOnlyField);
			
			localizedString = PXLocalizer.Localize(NonConstantMessages.StaticMethod());
			localizedString = PXLocalizer.Localize(messages.InstanceMethod());

			string localMessage = "Message";
			localizedString = PXLocalizer.Localize(localMessage);

			return localizedString;
		}
	}
}