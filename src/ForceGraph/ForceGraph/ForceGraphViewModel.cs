using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace ForceGraph
{
	public class ForceGraphViewModel : INotifyPropertyChanged
	{
		private ForceGraphScene _forceGraphScene;

		public event EventHandler OnForceGraphUpdated;

		public ForceGraphScene ForceGraphScene
		{
			get => _forceGraphScene;
			set
			{
				if (!ReferenceEquals(_forceGraphScene, value))
				{
					_forceGraphScene = value;
					NotifyPropertyChanged();
					OnForceGraphUpdated?.Invoke(this, EventArgs.Empty);			
				}
			}
		}

		public ForceGraph ForceGraph => ForceGraphScene._forceGraph;

		public event PropertyChangedEventHandler PropertyChanged;


		public virtual void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
		{
			if (string.IsNullOrWhiteSpace(propertyName))
				return;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public virtual void RaiseAllPropertyChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
	}
}
