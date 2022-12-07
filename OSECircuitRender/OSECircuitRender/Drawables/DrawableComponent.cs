﻿using OSECircuitRender.Definitions;
using OSECircuitRender.Instructions;
using OSECircuitRender.Interfaces;
using OSECircuitRender.Items;
using OSECircuitRender.Sheet;
using System;

namespace OSECircuitRender.Drawables;

public class DrawableComponent : IDrawableComponent, IHaveAParent
{
    public DrawableComponent(Type type, IWorksheetItem? parentItem)
    {
        ParentItem = parentItem;
        Type = type.Name;
    }

    public Guid ComponentGuid { get; set; } = Guid.NewGuid();
    public DrawablePinList DrawablePins { get; set; } = new();
    public DrawInstructionsList DrawInstructions { get; set; } = new();
    public IWorksheetItem? ParentItem { get; set; }
    public Coordinate Position { get; set; } = new(0, 0, 0);
    public string RefName => ParentItem == null ? "" : ParentItem.RefName;
    public float Rotation { get; set; }
    public Coordinate Size { get; set; } = new(1, 1, 0);
    public string Type { get; }
    public Worksheet? Worksheet { get; set; }

    public void SetPosition(float x, float y)
    {
        Position.X = x;
        Position.Y = y;
    }

    public void SetSize(int width, int height)
    {
        Size.X = width;
        Size.Y = height;
    }
}