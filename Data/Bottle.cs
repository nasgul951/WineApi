using System;
using System.Collections.Generic;

namespace WineApi.Data;

public partial class Bottle
{
    public int Bottleid { get; set; }

    public int Wineid { get; set; }

    public int Storageid { get; set; }

    public sbyte Consumed { get; set; }

    public int BinX { get; set; }

    public int BinY { get; set; }

    public int Depth { get; set; }

    public DateTime TsDate { get; set; }

    public DateTime? ConsumedDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Storage Storage { get; set; } = null!;

    public virtual Wine Wine { get; set; } = null!;
}
