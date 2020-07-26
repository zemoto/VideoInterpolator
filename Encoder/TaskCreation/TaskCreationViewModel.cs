using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Encoder.Encoding.Tasks;
using Encoder.Filters.Audio;
using Encoder.Filters.Audio.Copy;
using Encoder.Filters.Video;
using Encoder.Filters.Video.Copy;
using Encoder.Operations;
using ZemotoCommon.UI;

namespace Encoder.TaskCreation
{
   internal sealed class TaskCreationViewModel : ViewModelBase
   {
      public IEnumerable<EncodingTaskBase> GetEncodingTasks()
      {
         var operation = GetOperation();
         var encodingTasks = SelectedFiles.SelectMany( x => operation.GetEncodingTasks( x ) ).ToList();
         
         SelectedFiles.Clear();
         OperationType = OperationType.Filters;
         VideoFilterType = VideoFilterType.Copy;
         AudioFilterType = AudioFilterType.Copy;

         return encodingTasks.Any( x => x == null ) ? null : encodingTasks;
      }

      private Operation GetOperation()
      {
         switch ( OperationType )
         {
            case OperationType.Filters:
               return new ApplyFiltersOperation( VideoFilter, AudioFilter );
            case OperationType.Separate:
               return new SeparateOperation();
            default:
               throw new ArgumentOutOfRangeException();
         }
      }

      public ObservableCollection<string> SelectedFiles { get; } = new ObservableCollection<string>();

      private OperationType _operationType;
      public OperationType OperationType
      {
         get => _operationType;
         set => SetProperty( ref _operationType, value );
      }

      private VideoFilterType _videoFilterType = VideoFilterType.Copy;
      public VideoFilterType VideoFilterType
      {
         get => _videoFilterType;
         set
         {
            if ( SetProperty( ref _videoFilterType, value ) )
            {
               VideoFilter = VideoFilter.GetFilterForType( value );
            }
         }
      }

      private VideoFilter _videoFilter = new CopyVideoFilter();
      public VideoFilter VideoFilter
      {
         get => _videoFilter;
         private set => SetProperty( ref _videoFilter, value );
      }

      private AudioFilterType _audioFilterType = AudioFilterType.Copy;
      public AudioFilterType AudioFilterType
      {
         get => _audioFilterType;
         set
         {
            if ( SetProperty( ref _audioFilterType, value ) )
            {
               AudioFilter = AudioFilter.GetFilterForType( value );
            }
         }
      }

      private AudioFilter _audioFilter = new CopyAudioFilter();
      public AudioFilter AudioFilter
      {
         get => _audioFilter;
         private set => SetProperty( ref _audioFilter, value );
      }

      public RelayCommand SelectFilesCommand { get; set; }
      public RelayCommand<string> RemoveFileCommand { get; set; }
      public RelayCommand CreateTasksCommand { get; set; }
   }
}
