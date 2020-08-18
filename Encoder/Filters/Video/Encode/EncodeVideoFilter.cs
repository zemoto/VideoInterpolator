﻿using Encoder.Encoding;
using ZemotoCommon.Utils;

namespace Encoder.Filters.Video.Encode
{
   internal sealed class EncodeVideoFilter : VideoFilter
   {
      public override FilterViewModel ViewModel { get; } = new EncodeVideoFilterViewModel();
      public override string FilterName { get; } = "Encode";

      public override string CustomFilterArguments
      {
         get
         {
            var codec = ( (EncodeVideoFilterViewModel)ViewModel ).Codec;
            return $"-c:v {codec.GetAttribute<FilterEnumValueAttribute>().ParameterValue}";
         }
      }

      public override void Initialize( string file, VideoMetadata metadata )
      {
         base.Initialize( file, metadata );

         var model = ( (EncodeVideoFilterViewModel)ViewModel );
         TargetBitrate = model.UseCustomBitrate ? ( model.CustomBitrate * 1000 ) /*Model has kbps but backend wants bps*/ : metadata.Bitrate;
      }

      public override string GetFilterTargetExtension( string currentTargetExtension ) => ( (EncodeVideoFilterViewModel)ViewModel ).GetTargetExtension();
   }
}
