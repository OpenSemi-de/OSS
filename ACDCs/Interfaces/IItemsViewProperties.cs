﻿using Sharp.UI;

namespace ACDCs.Interfaces;

[BindableProperties]
public interface IItemsViewProperties
{
    double ButtonHeight { get; set; }
    double ButtonWidth { get; set; }
}
