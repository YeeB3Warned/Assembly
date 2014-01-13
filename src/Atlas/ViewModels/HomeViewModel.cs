﻿using System;
using System.Windows;
using System.Windows.Forms;
using Atlas.Models;
using Atlas.Pages;

namespace Atlas.ViewModels
{
	public class HomeViewModel : Base
	{
		#region Properties

		#region Application Stuff

		public string Status
		{
			get { return _status; }
			set { SetField(ref _status, value); }
		}
		private string _status = "Ready...";

		public string ApplicationTitle
		{
			get { return _applicationTitle; }
			set { SetField(ref _applicationTitle, value); }
		}
		private string _applicationTitle = "Welcome";

		public Thickness ApplicationBorderThickness
		{
			get { return _applicationBorderThickness; }
			set { SetField(ref _applicationBorderThickness, value); }
		}
		private Thickness _applicationBorderThickness;

		#endregion

		#region Window Masks

		public Visibility MaskVisibility
		{
			get { return _maskVisibility; }
			set { SetField(ref _maskVisibility, value); }
		}
		private Visibility _maskVisibility = Visibility.Collapsed;

		#endregion

		#region Window Actions/Resizing

		public Visibility ActionRestoreVisibility
		{
			get { return _actionRestoreVisibility; }
			set { SetField(ref _actionRestoreVisibility, value); }
		}
		private Visibility _actionRestoreVisibility = Visibility.Collapsed;

		public Visibility ActionMaximizeVisibility
		{
			get { return _actionMaximizeVisibility; }
			set { SetField(ref _actionMaximizeVisibility, value); }
		}
		private Visibility _actionMaximizeVisibility = Visibility.Collapsed;

		public Visibility ResizingVisibility
		{
			get { return _resizingVisibility; }
			set { SetField(ref _resizingVisibility, value); }
		}
		private Visibility _resizingVisibility = Visibility.Collapsed;

		#endregion

		#region Content

		public IAssemblyPage AssemblyPage
		{
			get { return _assemblyPage; }
			set
			{
				// try closing current page
				if (!_assemblyPage.Close()) return;

				// aite, we can
				SetField(ref _assemblyPage, value);
			}
		}
		private IAssemblyPage _assemblyPage = new StartPage();
		#endregion

		#endregion

		#region Events

		#region Overrides
		public void OnStateChanged(object sender, EventArgs eventArgs)
		{
			switch ((WindowState)sender)
			{
				case WindowState.Normal:
					ApplicationBorderThickness = new Thickness(1, 1, 1, 25);
					ActionRestoreVisibility = Visibility.Collapsed;
					ActionMaximizeVisibility = 
						ResizingVisibility = Visibility.Visible;
					break;
				case WindowState.Maximized:
					ApplicationBorderThickness = new Thickness(0, 0, 0, 25);
					ActionRestoreVisibility = Visibility.Visible;
					ActionMaximizeVisibility =
						ResizingVisibility = Visibility.Collapsed;
					break;
			}
		}
		#endregion

		#endregion

		#region Dialog Management

		private int _maskCount;

		public void ShowDialog(bool showMask = true)
		{
			if (showMask)
				_maskCount++;

			if (_maskCount > 0)
				MaskVisibility = Visibility.Visible;
		}

		public void HideDialog(bool maskShown = true)
		{
			if (maskShown)
				_maskCount--;

			if (_maskCount == 0)
				MaskVisibility = Visibility.Collapsed;
		}
		
		#endregion

		#region Helpers

		public enum Type
		{
			BlamCache,
			MapInfo,
			MapImage,
			Campaign,
			Patch,

			Other
		}

		public void OpenFile(Type type)
		{
			var openFileDialog = new OpenFileDialog
			{
				Filter = "Blam Cache Files (*.map)|*.map|" +
				         "Blam Map Info (*.mapinfo)|*.mapinfo|" +
				         "Blam Map Image (*.blf)|*.blf|" +
				         "Blam Campaign (*.campaign)|*.campaign|" +
				         "Assembly Patch (*.asmp)|*.asmp|" +
				         "All Files (*.*)|*.*",
				FilterIndex = (int) type
			};

			if (openFileDialog.ShowDialog() != DialogResult.OK)
				return;

			// TODO: process type, and open in the correct editor
			AssemblyPage = new CachePage(openFileDialog.FileName);
		}

		#endregion
	}
}