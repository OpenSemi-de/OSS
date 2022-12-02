﻿using OSECircuitRender.Items;
using System;

namespace OSECircuitRender.Interfaces;

public interface IWorksheetItem
{
    static string DefaultValue { get; set; }
    static bool IsInsertable { get; set; }
    IDrawableComponent DrawableComponent { get; set; }
    int Height { get; }
    public Guid ItemGuid { get; set; }
    public string Name { get; set; }
    public DrawablePinList Pins { get; set; }
    string RefName { get; set; }
    float Rotation { get; set; }
    int Width { get; }
    int X { get; }
    int Y { get; }
}