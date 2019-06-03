using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;

namespace Acuminator.Utilities
{
	[Export]
	public class CodeAnalysisSettings
	{
		public const bool DefaultRecursiveAnalysisEnabled = true;
		public const bool DefaultISVSpecificAnalyzersEnabled = false;
		public const bool DefaultSuppressionMechanismEnabled = true;
		public const bool DefaultStaticAnalysisEnabled = true;

		public static CodeAnalysisSettings Default => 
			new CodeAnalysisSettings(
				DefaultRecursiveAnalysisEnabled,
				DefaultISVSpecificAnalyzersEnabled,
				DefaultStaticAnalysisEnabled,
				DefaultSuppressionMechanismEnabled);

		public virtual bool RecursiveAnalysisEnabled { get; }

		public virtual bool IsvSpecificAnalyzersEnabled { get; }

		public virtual bool StaticAnalysisEnabled { get; }

		public virtual bool SuppressionMechanismEnabled { get; }

		protected CodeAnalysisSettings()
		{
		}

		private CodeAnalysisSettings(bool recursiveAnalysisEnabled, bool isvSpecificAnalyzersEnabled, bool staticAnalysisEnabled, 
									 bool suppressionMechanismEnabled)
		{
			RecursiveAnalysisEnabled = recursiveAnalysisEnabled;
			IsvSpecificAnalyzersEnabled = isvSpecificAnalyzersEnabled;
			StaticAnalysisEnabled = staticAnalysisEnabled;
			SuppressionMechanismEnabled = suppressionMechanismEnabled;
		}	

		public CodeAnalysisSettings WithRecursiveAnalysisEnabled()
		{
			return WithRecursiveAnalysisEnabledValue(true);
		}

		public CodeAnalysisSettings WithRecursiveAnalysisDisabled()
		{
			return WithRecursiveAnalysisEnabledValue(false);
		}

		protected virtual CodeAnalysisSettings WithRecursiveAnalysisEnabledValue(bool value) =>
			new CodeAnalysisSettings(value, IsvSpecificAnalyzersEnabled, StaticAnalysisEnabled, SuppressionMechanismEnabled);



		public CodeAnalysisSettings WithIsvSpecificAnalyzersEnabled()
		{
			return WithIsvSpecificAnalyzersEnabledValue(true);
		}

		public CodeAnalysisSettings WithIsvSpecificAnalyzersDisabled()
		{
			return WithIsvSpecificAnalyzersEnabledValue(false);
		}

		protected virtual CodeAnalysisSettings WithIsvSpecificAnalyzersEnabledValue(bool value) =>
			new CodeAnalysisSettings(RecursiveAnalysisEnabled, value, StaticAnalysisEnabled, SuppressionMechanismEnabled);



		public CodeAnalysisSettings WithStaticAnalysisEnabled()
		{
			return WithStaticAnalysisEnabledValue(true);
		}

		public CodeAnalysisSettings WithStaticAnalysisDisabled()
		{
			return WithStaticAnalysisEnabledValue(false);
		}

		protected virtual CodeAnalysisSettings WithStaticAnalysisEnabledValue(bool value) =>
			new CodeAnalysisSettings(RecursiveAnalysisEnabled, IsvSpecificAnalyzersEnabled, value, SuppressionMechanismEnabled);



		public CodeAnalysisSettings WithSuppressionMechanismEnabled()
		{
			return WithSuppressionMechanismEnabledValue(true);
		}

		public CodeAnalysisSettings WithSuppressionMechanismDisabled()
		{
			return WithSuppressionMechanismEnabledValue(false);
		}

		protected virtual CodeAnalysisSettings WithSuppressionMechanismEnabledValue(bool value) =>
			new CodeAnalysisSettings(RecursiveAnalysisEnabled, IsvSpecificAnalyzersEnabled, StaticAnalysisEnabled, value);	
	}
}
