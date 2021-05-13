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
		public const bool DefaultPX1007DocumentationDiagnosticEnabled = false;

		public static CodeAnalysisSettings Default => 
			new CodeAnalysisSettings(
				DefaultRecursiveAnalysisEnabled,
				DefaultISVSpecificAnalyzersEnabled,
				DefaultStaticAnalysisEnabled,
				DefaultSuppressionMechanismEnabled,
				DefaultPX1007DocumentationDiagnosticEnabled);

		public virtual bool RecursiveAnalysisEnabled { get; }

		public virtual bool IsvSpecificAnalyzersEnabled { get; }

		public virtual bool StaticAnalysisEnabled { get; }

		public virtual bool SuppressionMechanismEnabled { get; }

		public virtual bool PX1007DocumentationDiagnosticEnabled { get; }

		protected CodeAnalysisSettings()
		{
		}

		internal CodeAnalysisSettings(bool recursiveAnalysisEnabled, bool isvSpecificAnalyzersEnabled, bool staticAnalysisEnabled, 
									  bool suppressionMechanismEnabled, bool px1007DocumentationDiagnosticEnabled)
		{
			RecursiveAnalysisEnabled = recursiveAnalysisEnabled;
			IsvSpecificAnalyzersEnabled = isvSpecificAnalyzersEnabled;
			StaticAnalysisEnabled = staticAnalysisEnabled;
			SuppressionMechanismEnabled = suppressionMechanismEnabled;
			PX1007DocumentationDiagnosticEnabled = px1007DocumentationDiagnosticEnabled;
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
			new CodeAnalysisSettings(value, IsvSpecificAnalyzersEnabled, StaticAnalysisEnabled, SuppressionMechanismEnabled, 
									 PX1007DocumentationDiagnosticEnabled);

		public CodeAnalysisSettings WithIsvSpecificAnalyzersEnabled()
		{
			return WithIsvSpecificAnalyzersEnabledValue(true);
		}

		public CodeAnalysisSettings WithIsvSpecificAnalyzersDisabled()
		{
			return WithIsvSpecificAnalyzersEnabledValue(false);
		}

		protected virtual CodeAnalysisSettings WithIsvSpecificAnalyzersEnabledValue(bool value) =>
			new CodeAnalysisSettings(RecursiveAnalysisEnabled, value, StaticAnalysisEnabled, SuppressionMechanismEnabled, 
									 PX1007DocumentationDiagnosticEnabled);

		public CodeAnalysisSettings WithStaticAnalysisEnabled()
		{
			return WithStaticAnalysisEnabledValue(true);
		}

		public CodeAnalysisSettings WithStaticAnalysisDisabled()
		{
			return WithStaticAnalysisEnabledValue(false);
		}

		protected virtual CodeAnalysisSettings WithStaticAnalysisEnabledValue(bool value) =>
			new CodeAnalysisSettings(RecursiveAnalysisEnabled, IsvSpecificAnalyzersEnabled, value, SuppressionMechanismEnabled, 
									 PX1007DocumentationDiagnosticEnabled);



		public CodeAnalysisSettings WithSuppressionMechanismEnabled()
		{
			return WithSuppressionMechanismEnabledValue(true);
		}

		public CodeAnalysisSettings WithSuppressionMechanismDisabled()
		{
			return WithSuppressionMechanismEnabledValue(false);
		}

		protected virtual CodeAnalysisSettings WithSuppressionMechanismEnabledValue(bool value) =>
			new CodeAnalysisSettings(RecursiveAnalysisEnabled, IsvSpecificAnalyzersEnabled, StaticAnalysisEnabled, value, 
									 PX1007DocumentationDiagnosticEnabled);


		public CodeAnalysisSettings WithPX1007DocumentationDiagnosticEnabled()
		{
			return WithPX1007DocumentationDiagnosticEnabledValue(true);
		}

		public CodeAnalysisSettings WithPX1007DocumentationDiagnosticDisabled()
		{
			return WithPX1007DocumentationDiagnosticEnabledValue(false);
		}

		protected virtual CodeAnalysisSettings WithPX1007DocumentationDiagnosticEnabledValue(bool value) =>
			new CodeAnalysisSettings(RecursiveAnalysisEnabled, IsvSpecificAnalyzersEnabled, StaticAnalysisEnabled, SuppressionMechanismEnabled,
									 value);
	}
}
