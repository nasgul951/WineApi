using System;
using System.Collections.Generic;

namespace WineApi.Data;

public partial class Storage
{
    public int Storageid { get; set; }

    public string? StorageDescription { get; set; }

    public string? StorageAddr1 { get; set; }

    public string? StorageAddr2 { get; set; }

    public string? StorageCity { get; set; }

    public string? StorageState { get; set; }

    public int? StorageZip { get; set; }

    public DateTime TsDate { get; set; }

    public virtual ICollection<Bottle> Bottles { get; set; } = new List<Bottle>();
}
