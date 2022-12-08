﻿using System.Collections.Generic;
using ACDCs.CircuitRenderer.Interfaces;
using ACDCs.CircuitRenderer.Sheet;

namespace ACDCs.CircuitRenderer.Drawables;

public sealed class DrawableComponentList : List<IDrawableComponent>
{
    public DrawableComponentList(Worksheet worksheet)
    {
        Worksheet = worksheet;
    }

    public Worksheet? Worksheet { get; set; }

    public new void Add(IDrawableComponent component)
    {
        component.Worksheet = Worksheet;
        base.Add(component);
    }
}
