#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Acuminator.Utilities.Common;

using static System.Net.Mime.MediaTypeNames;

namespace Acuminator.Vsix.ToolWindows.CodeMap;

/// <summary>
/// Interaction logic for CodeMapSearchBox.xaml
/// </summary>
public partial class CodeMapSearchBoxControl : UserControl
{
	private static readonly DependencyPropertyKey HasTextPropertyKey =
		DependencyProperty.RegisterReadOnly("HasText", typeof(bool), typeof(CodeMapSearchBoxControl),
											new FrameworkPropertyMetadata(defaultValue: false, FrameworkPropertyMetadataOptions.AffectsRender));

	/// <summary>
	/// Identifies the <see cref="HasText" /> dependency property.
	/// </summary>
	public static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

	/// <summary>
	/// Gets a value indicating whether this control has non-empty text.
	/// </summary>
	[Browsable(false)]
	public bool HasText
	{
		get => (bool)GetValue(HasTextProperty);
		private set => SetValue(HasTextPropertyKey, value);
	}

	public CodeMapSearchBoxControl()
	{
		InitializeComponent();
	}

	private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is TextBox searchTextBox)
		{
			HasText = searchTextBox.Text.IsNullOrEmpty();
		}
	}

	private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (sender is not TextBox searchTextBox)
			return;

		if (e.Key == Key.Escape)
		{
			searchTextBox.Text = string.Empty;
			e.Handled = true;
		}
	}
}
