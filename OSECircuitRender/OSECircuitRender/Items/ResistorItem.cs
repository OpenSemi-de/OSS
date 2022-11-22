﻿using OSECircuitRender.Drawables;

namespace OSECircuitRender.Items;

public sealed class ResistorItem : WorksheetItem
{
    public ResistorItem()
    {
        DrawableComponent = new ResistorDrawable(this, DefaultValue, 0, 0 );
    }

    public static string DefaultValue { get; set; } = "10k";

    public new static bool IsInsertable { get; set; } = true;


    public ResistorItem(string value, float x, float y)
    {
        DrawableComponent = new ResistorDrawable(this, value, x, y);
        Value = value;
    }

    public ResistorItem(string value)
    {
        DrawableComponent = new ResistorDrawable(this, value, 0, 0);
        Value = value;
    }

    public string Value { get; set; }
}