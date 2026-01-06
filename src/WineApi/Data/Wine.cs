using System;
using System.Collections.Generic;

namespace WineApi.Data;

public partial class Wine
{
    public int Wineid { get; set; }

    public string? Vineyard { get; set; }

    public string? Label { get; set; }

    public string? Varietal { get; set; }

    public int? Vintage { get; set; }

    public DateTime TsDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<Bottle> Bottles { get; set; } = new List<Bottle>();
}
