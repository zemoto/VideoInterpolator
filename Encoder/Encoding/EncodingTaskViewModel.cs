﻿using System;
using System.IO;
using System.Threading;
using System.Windows;
using Encoder.Filters;
using Encoder.Utils;

namespace Encoder.Encoding
{
   internal sealed class EncodingTaskViewModel : ViewModelBase, IDisposable
   {
      private readonly Filter _filter;
      private bool _reencode;
      private double _sourceFrameRate;
      private TimeSpan _sourceDuration;
      public CancellationTokenSource CancelToken { get; }

      public EncodingTaskViewModel( string sourceFile, bool reencode, Filter filter )
      {
         SourceFile = sourceFile;
         _filter = filter;
         _reencode = reencode;

         CancelToken = new CancellationTokenSource();
      }

      public void Dispose()
      {
         CancelToken?.Dispose();
      }

      private bool ShouldApplyFilter() => _reencode && _filter != null && _filter.ShouldApplyFilter();

      public string GetEncodingArguments()
      {
         if ( ShouldApplyFilter() )
         {
            return FilterArgumentBuilder.GetFilterArguments( _filter );
         }

         if ( _reencode )
         {
            return "-c:v libx264";
         }

         return "-c:v copy";
      }

      public bool Initialize()
      {
         bool success = VideoMetadataReader.GetVideoInfo( SourceFile, out var sourceFrameRate, out var sourceDuration );
         if ( !success )
         {
            MessageBox.Show( $"Could not read video file: {SourceFile}" );
            return false;
         }

         _sourceFrameRate = sourceFrameRate;
         _sourceDuration = sourceDuration;
         TargetFile = Path.Combine( Path.GetDirectoryName( SourceFile ), $"{Path.GetFileNameWithoutExtension( SourceFile )}_done.mp4" );

         _filter?.Initialize( sourceFrameRate, sourceDuration );
         
         OnPropertyChanged( null );

         return true;
      }

      private void UpdateTimeRemaining()
      {
         if ( !Started || HasNoDurationData || FramesDone == 0 )
         {
            return;
         }

         var ellapsed = DateTime.Now - _startTime;
         _timeRemaining = TimeSpan.FromSeconds( ellapsed.TotalSeconds / FramesDone * ( TargetTotalFrames - FramesDone ) );

         OnPropertyChanged( nameof( TimeRemainingString ) );
      }

      public string FilterName => ShouldApplyFilter() ? _filter.FilterName : "None";
      public string SourceFile { get; }
      public string FileName => Path.GetFileName( SourceFile );
      public bool HasNoDurationData => _sourceDuration == TimeSpan.Zero && !Finished;

      public string TargetFile { get; private set; }

      public int TargetTotalFrames => ShouldApplyFilter() ? _filter.GetTargetFrameCount() : (int)( _sourceDuration.TotalSeconds * _sourceFrameRate );

      private DateTime _startTime;
      private TimeSpan _timeRemaining = TimeSpan.Zero;
      public string TimeRemainingString => _timeRemaining == TimeSpan.Zero ? "N/A" : _timeRemaining.ToString( @"hh\:mm\:ss" );

      private int _cpuUsage;
      public int CpuUsage
      {
         get => _cpuUsage;
         set => SetProperty( ref _cpuUsage, value );
      }

      private int _framesDone;
      public int FramesDone
      {
         get => _framesDone;
         set
         {
            if ( SetProperty( ref _framesDone, value ) )
            {
               if ( TargetTotalFrames != 0 )
               {
                  Progress = Math.Round( value / (double)TargetTotalFrames * 100, 2 );
                  UpdateTimeRemaining();
               }
            }
         }
      }

      private double _progress;
      public double Progress
      {
         get => _progress;
         private set => SetProperty( ref _progress, value );
      }

      private bool _finished;
      public bool Finished
      {
         get => _finished;
         set
         {
            if ( SetProperty( ref _finished, value ) )
            {
               OnPropertyChanged( nameof( HasNoDurationData ) );
               if ( value )
               {
                  FramesDone = TargetTotalFrames;
                  Progress = 100;
               }
            }
         }
      }

      private bool _started;
      public bool Started
      {
         get => _started;
         set
         {
            if ( SetProperty( ref _started, value ) && value )
            {
               _startTime = DateTime.Now;
            }
         }
      }
   }
}
