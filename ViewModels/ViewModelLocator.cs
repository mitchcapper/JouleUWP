using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JouleUWP.ViewModels {
	public class ViewModelLocator {
		private static ViewModelLocator _instance;
		public static ViewModelLocator instance {
			get { return _instance ?? (_instance = new ViewModelLocator()); }
		}


		public static bool IsDesignData { get; private set; } = ViewModelBase.IsInDesignModeStatic;
		public static bool IsTenFootExperience => IsXBox;//will want to let the user enable this in options
		public static bool IsXBox => (Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues?["DeviceFamily"] == "Xbox") || true;
		public static bool IsTv => IsTenFootExperience;
		public ViewModelLocator() {
			if (!IsDesignData) {
				//IsDesignData = true;
			}

			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
			bool first = _instance == null;
			_instance = this;
			if (first) {
				//				SimpleIoc.Default.Register<MainViewModel>();
			}
		}

		
		//public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
		
		public static void Cleanup() {
			// TODO Clear the ViewModels
		}
	}
}
