﻿namespace Encoder.Filters.Video.Copy
{
   internal sealed class CopyVideoFilter : VideoFilter
   {
      public override string FilterName { get; } = "Copy";
      public override string CustomFilterArguments { get; } = "-c:v copy";
   }
}
