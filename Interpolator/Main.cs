﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Interpolator.Encoding;
using Interpolator.TaskCreation;
using Interpolator.Utils;

namespace Interpolator
{
   internal sealed class Main : IDisposable
   {
      private readonly MainWindowViewModel _model;
      private readonly EncodingManager _encodingManager;

      public Main()
      {
         _encodingManager = new EncodingManager();

         _model = new MainWindowViewModel( _encodingManager.Model )
         {
            NewTasksCommand = new RelayCommand( CreateAndStartNewTasks )
         };
      }

      public void Dispose()
      {
         _encodingManager?.Dispose();
      }

      public void ShowDialog()
      {
         var window = new MainWindow( _model );
         window.Closing += OnMainWindowClosing;
         window.ShowDialog();
      }

      private async void OnMainWindowClosing( object sender, CancelEventArgs e )
      {
         if ( !_encodingManager.Model.Tasks.Any() )
         {
            return;
         }

         var result = MessageBox.Show( "Files are still being encoded, wait for cancellation and cleanup?", "Exiting", MessageBoxButton.YesNoCancel );
         if ( result == MessageBoxResult.Cancel )
         {
            e.Cancel = true;
         }
         else if ( result == MessageBoxResult.Yes )
         {
            e.Cancel = true;
            foreach ( var task in _encodingManager.Model.Tasks )
            {
               task.CancelTaskCommand.Execute( null );
            }
            while ( _encodingManager.Model.Tasks.Any() )
            {
               await Task.Delay( 300 );
            }
            ( sender as Window ).Close();
         }
      }

      private void CreateAndStartNewTasks()
      {
         var taskWizard = new TaskCreationWizard();
         var tasks = taskWizard.CreateEncodingTasks();

         foreach ( var task in tasks )
         {
            _encodingManager.Model.Tasks.Add( task );
         }
      }
   }
}
