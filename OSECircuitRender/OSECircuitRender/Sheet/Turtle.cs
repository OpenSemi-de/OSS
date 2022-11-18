﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using OSECircuitRender.Definitions;
using OSECircuitRender.Drawables;
using OSECircuitRender.Interfaces;
using OSECircuitRender.Items;
using OSECircuitRender.Scene;
using Color = Microsoft.Maui.Graphics.Color;

namespace OSECircuitRender.Sheet;

public class Turtle
{
    private static readonly Point[][] DirectionalTriangles = {
        new Point[]
        {
            new(0,0),
            new(1,0),
            new(0.5f,0.5f),
        },
        new Point[]
        {
            new(1,0),
            new(1,1),
            new(0.5f,0.5f),
        },
        new Point[]
        {
            new(1,1),
            new(0,1),
            new(0.5f,0.5f),
        },
        new Point[]
        {
            new(0,1),
            new(0,0),
            new(0.5f,0.5f),
        },
    };

    private static readonly Point[][] DirectionalTrianglesMax = {
        new Point[]
        {
            new(0,0),
            new(int.MaxValue,0),
            new(int.MaxValue /2,int.MaxValue /2),
        },
        new Point[]
        {
            new(int.MaxValue,0),
            new(int.MaxValue, int.MaxValue),

            new(int.MaxValue /2,int.MaxValue /2),
        },
        new Point[]
        {
            new(int.MaxValue,int.MaxValue),
            new(0,int.MaxValue),

            new(int.MaxValue /2,int.MaxValue /2),
        },
        new Point[]
        {
            new(0,int.MaxValue),
            new(0, 0),

            new(int.MaxValue /2,int.MaxValue /2),
        },
    };

    private static readonly Point[] DirectionPoints = {
        new Point(0,-1),
        new Point(1,0),
        new Point(0,1),
        new Point(-1,0),
    };

    private readonly List<RectFr> _collisionRectangles = new();
    private readonly WorksheetItemList _items;
    private readonly WorksheetItemList _nets;
    private readonly Coordinate _sheetSize;
    private readonly WorksheetItemList _traces;

    public Turtle(WorksheetItemList items, WorksheetItemList nets, Coordinate sheetSize, WorksheetItemList traces)
    {
        _items = items;
        _nets = nets;
        _sheetSize = sheetSize;
        _traces = traces;
    }

    public ICanvas? DebugCanvas { get; set; }

    public static Direction LineIntersectsRect(Point p1, Point p2, RectFr r)
    {
        if (LineIntersectsLine(
             p1,
             p2,
             new Point(r.X1, r.Y1),
             new Point(r.X2, r.Y2)))
            return Direction.Top;

        if (LineIntersectsLine(
            p1,
            p2,
            new Point(r.X2, r.Y2),
            new Point(r.X3, r.Y3)))
            return Direction.Right;

        if (LineIntersectsLine(
                p1,
                p2,
                new Point(r.X3, r.Y3),
                new Point(r.X4, r.Y4)))
            return Direction.Bottom;

        if (LineIntersectsLine(
                p1,
                p2,
                new Point(r.X4, r.Y4),
                new Point(r.X1, r.Y1)))
            return Direction.Left;

        int hitContains = 0;
        if (PointInTriangle(p1, new Point(r.X1, r.Y1), new Point(r.X2, r.Y2), new Point(r.X4, r.Y4)))
            hitContains++;
        if (PointInTriangle(p2, new Point(r.X1, r.Y1), new Point(r.X2, r.Y2), new Point(r.X4, r.Y4)))
            hitContains++;
        if (PointInTriangle(p1, new Point(r.X1, r.Y1), new Point(r.X2, r.Y2), new Point(r.X3, r.Y3)))
            hitContains++;
        if (PointInTriangle(p2, new Point(r.X1, r.Y1), new Point(r.X2, r.Y2), new Point(r.X3, r.Y3)))
            hitContains++;

        if (hitContains > 1)
        {
            return Direction.Contains;
        }

        return Direction.None;
    }

    public static bool PointInTriangle(Point pt, Point v1, Point v2, Point v3)
    {
        var d1 = Sign(pt, v1, v2);
        var d2 = Sign(pt, v2, v3);
        var d3 = Sign(pt, v3, v1);

        var hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        var hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    public static float Sign(Point p1, Point p2, Point p3)
    {
        return Convert.ToSingle((p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y));
    }

    public WorksheetItemList GetTraces()
    {
        foreach (var item in _items)
        {
            var rect = new RectFr
            {
                X1 = item.X,
                Y1 = item.Y,
                X2 = item.X + item.Width,
                Y2 = item.Y,
                X3 = item.X + item.Width,
                Y3 = item.Y + item.Height,
                X4 = item.X,
                Y4 = item.Y + item.Height
            };

            if (item.Rotation != 0)
            {
                var rotation = item.Rotation;
                float centerX = rect.X1 + (rect.X3 - rect.X1) / 2;
                float centerY = rect.Y1 + (rect.Y3 - rect.Y1) / 2;

                Coordinate rotatedItemPos1 =
                    RotateCoordinate(rect.X1, rect.Y1, centerX, centerY, rotation);
                Coordinate rotatedItemPos2 =
                    RotateCoordinate(rect.X2, rect.Y2, centerX, centerY, rotation);
                Coordinate rotatedItemPos3 =
                    RotateCoordinate(rect.X3, rect.Y3, centerX, centerY, rotation);
                Coordinate rotatedItemPos4 =
                    RotateCoordinate(rect.X4, rect.Y4, centerX, centerY, rotation);

                var itemPositions = new List<Coordinate>()
                {
                    rotatedItemPos1,
                    rotatedItemPos2,
                    rotatedItemPos3,
                    rotatedItemPos4
                };

                rect.X1 = itemPositions[0].X;
                rect.Y1 = itemPositions[0].Y;
                rect.X2 = itemPositions[1].X;
                rect.Y2 = itemPositions[1].Y;
                rect.X3 = itemPositions[2].X;
                rect.Y3 = itemPositions[2].Y;
                rect.X4 = itemPositions[3].X;
                rect.Y4 = itemPositions[3].Y;
            }

            _collisionRectangles.Add(rect);
            DebugDrawRectangle(rect);
        }

        foreach (var net in _nets)
        {
            var trace = new TraceItem();
            for (var i = 0; i < net.Pins.Count; i++)
            {
                var pin1 = net.Pins[i];
                var pin2 = i < net.Pins.Count - 1 ? net.Pins[i + 1] : net.Pins[0];
                var pin1drawable = pin1.BackRef.DrawableComponent;
                var pin2drawable = pin2.BackRef.DrawableComponent;

                SetColorAndScaling(pin1drawable, pin2drawable);
                var pin1X = pin1.Position.X;
                var pin1Y = pin1.Position.Y;
                var position1X = pin1drawable.Position.X + (pin1.Position.X * pin1drawable.Size.X);
                var position1Y = pin1drawable.Position.Y + (pin1.Position.Y * pin1drawable.Size.Y);
                if (pin1drawable.Rotation != 0)
                {
                    float centerX = pin1drawable.Position.X + pin1drawable.Size.X / 2;
                    float centerY = pin1drawable.Position.Y + pin1drawable.Size.Y / 2;
                    Coordinate rotatedPinPos =
                        RotateCoordinate(position1X, position1Y, centerX, centerY, pin1drawable.Rotation);
                    position1X = rotatedPinPos.X;
                    position1Y = rotatedPinPos.Y;
                    Coordinate rotatedPinRelPos = RotateCoordinate(pin1X, pin1Y, 0.5f, 0.5f, pin1drawable.Rotation);
                    pin1X = rotatedPinRelPos.X;
                    pin1Y = rotatedPinRelPos.Y;
                }

                Direction direction1 = GetDirection(pin1X, pin1Y);

                var pin2X = pin2.Position.X;
                var pin2Y = pin2.Position.Y;
                var position2X = pin2drawable.Position.X + (pin2.Position.X * pin2drawable.Size.X);
                var position2Y = pin2drawable.Position.Y + (pin2.Position.Y * pin2drawable.Size.Y);

                if (pin2drawable.Rotation != 0)
                {
                    float centerX = pin2drawable.Position.X + pin2drawable.Size.X / 2;
                    float centerY = pin2drawable.Position.Y + pin2drawable.Size.Y / 2;
                    Coordinate rotatedPinPos =
                        RotateCoordinate(position2X, position2Y, centerX, centerY, pin2drawable.Rotation);
                    position2X = rotatedPinPos.X;
                    position2Y = rotatedPinPos.Y;
                    Coordinate rotatedPinRelPos = RotateCoordinate(pin2X, pin2Y, 0.5f, 0.5f, pin1drawable.Rotation);
                    pin2X = rotatedPinRelPos.X;
                    pin2Y = rotatedPinRelPos.Y;
                }

                Direction direction2 = GetDirection(pin2X, pin2Y);

                DebugDrawLine(
                    position1X,
                    position1Y,
                    Convert.ToSingle(position1X + DirectionPoints[(int)direction1].X / 2),
                    Convert.ToSingle(position1Y + DirectionPoints[(int)direction1].Y / 2)
                );

                var currentPoint = new Point(
                    Convert.ToSingle(position1X + DirectionPoints[(int)direction1].X / 2),
                    Convert.ToSingle(position1Y + DirectionPoints[(int)direction1].Y / 2));
                trace.AddPart(new Coordinate(position1X, position1Y, 0), Coordinate.FromPoint(currentPoint));

                var targetPoint = new Point(
                    Convert.ToSingle(Math.Ceiling(position2X + DirectionPoints[(int)direction2].X / 2)),
                    Convert.ToSingle(Math.Ceiling(position2Y + DirectionPoints[(int)direction2].Y / 2)));
                if (i != net.Pins.Count - 1)
                {
                    Direction nextDirection = GetDirectionMax(currentPoint, targetPoint);
                    bool found = false;
                    for (var f = 0; f < 1000 && !found; f++)
                    {
                        if (currentPoint == targetPoint)
                            found = true;

                        var stepPoint = GetNextStepPoint(currentPoint, targetPoint, ref nextDirection);

                        Console.WriteLine(f + "-" + pin1.ComponentGuid + "-" + nextDirection);

                        DebugDrawLine(Convert.ToSingle(currentPoint.X),
                                Convert.ToSingle(currentPoint.Y),
                                Convert.ToSingle(stepPoint.X), Convert.ToSingle(stepPoint.Y));

                        trace.AddPart(Coordinate.FromPoint(currentPoint), Coordinate.FromPoint(stepPoint));

                        currentPoint = stepPoint;
                    }
                }
            }

            _traces.AddItem(trace);
        }

        return _traces;
    }

    private static bool LineIntersectsLine(Point line1Point1, Point line1Point2, Point line2Point1, Point line2Point2)
    {
        float q = Convert.ToSingle((line1Point1.Y - line2Point1.Y) * (line2Point2.X - line2Point1.X) - (line1Point1.X - line2Point1.X) * (line2Point2.Y - line2Point1.Y));
        float d = Convert.ToSingle((line1Point2.X - line1Point1.X) * (line2Point2.Y - line2Point1.Y) - (line1Point2.Y - line1Point1.Y) * (line2Point2.X - line2Point1.X));

        if (d == 0)
        {
            return false;
        }

        float r = q / d;

        q = Convert.ToSingle((line1Point1.Y - line2Point1.Y) * (line1Point2.X - line1Point1.X) - (line1Point1.X - line2Point1.X) * (line1Point2.Y - line1Point1.Y));
        float s = q / d;

        if (r < 0 || r > 1 || s < 0 || s > 1)
        {
            return false;
        }

        return true;
    }

    private static Coordinate RotateCoordinate(float posX, float posY, float centerX, float centerY,
                double angleInDegrees)
    {
        var angleInRadians = angleInDegrees * (Math.PI / 180);
        var cosTheta = Math.Cos(angleInRadians);
        var sinTheta = Math.Sin(angleInRadians);
        return new Coordinate
        {
            X =
                Convert.ToSingle(
                (cosTheta * (posX - centerX) -
                    sinTheta * (posY - centerY) + centerX)),
            Y =
                Convert.ToSingle(
                (sinTheta * (posX - centerX) +
                 cosTheta * (posY - centerY) + centerY))
        };
    }

    private Point CheckCollision(Point currentPoint, Direction direction, out RectFr? collisionRect)
    {
        var globalStepPoint = new Point(Convert.ToSingle(currentPoint.X + DirectionPoints[(int)direction].X / 2),
            Convert.ToSingle(currentPoint.Y + DirectionPoints[(int)direction].Y / 2));

        collisionRect = _collisionRectangles
            .FirstOrDefault(cr =>
            {
                var intersect = LineIntersectsRect(currentPoint, globalStepPoint, cr);
                if (intersect == Direction.None)
                {
                    return false;
                }

                return true;
            });
        return globalStepPoint;
    }

    private void DebugDrawLine(float position1X, float position1Y, float position2X, float position2Y)
    {
        DebugCanvas?.DrawLine(position1X * DrawableScene.Zoom * DrawableScene.BaseGridSize, position1Y * DrawableScene.Zoom * DrawableScene.BaseGridSize, position2X * DrawableScene.Zoom * DrawableScene.BaseGridSize, position2Y * DrawableScene.Zoom * DrawableScene.BaseGridSize);
    }

    private void DebugDrawRectangle(RectFr rect)
    {
        DebugCanvas?.DrawLine(
            rect.X1 * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.Y1 * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.X2 * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.Y2 * DrawableScene.Zoom * DrawableScene.BaseGridSize
        );

        DebugCanvas?.DrawLine(
            rect.X2 * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.Y2 * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.X3 * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.Y3 * DrawableScene.Zoom * DrawableScene.BaseGridSize
        );

        DebugCanvas?.DrawLine(
            rect.X3 * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.Y3 * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.X4 * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.Y4 * DrawableScene.Zoom * DrawableScene.BaseGridSize
        );

        DebugCanvas?.DrawLine(
            rect.X4 * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.Y4 * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.X1 * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.Y1 * DrawableScene.Zoom * DrawableScene.BaseGridSize
        );
    }

    private void DebugDrawRectangle(RectF rect)
    {
        DebugCanvas?.DrawRectangle(
            rect.X * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.Y * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.Width * DrawableScene.Zoom * DrawableScene.BaseGridSize,
            rect.Height * DrawableScene.Zoom * DrawableScene.BaseGridSize
            );
    }

    private void DebugFillRect(RectF rect, int offset = 0)
    {
        DebugCanvas?.FillRectangle(
            rect.X * DrawableScene.Zoom * DrawableScene.BaseGridSize + offset,
            rect.Y * DrawableScene.Zoom * DrawableScene.BaseGridSize + offset,
            rect.Width * DrawableScene.Zoom * DrawableScene.BaseGridSize - 2 * offset,
            rect.Height * DrawableScene.Zoom * DrawableScene.BaseGridSize - 2 * offset

            );
    }

    private Direction GetDirection(float posX, float posY)
    {
        Direction direction = Direction.None;

        int pos = 0;
        foreach (var triangle in DirectionalTriangles)
        {
            if (PointInTriangle(
                    new Point(posX, posY),
                    triangle[0],
                    triangle[1],
                    triangle[2]
                ))
            {
                return (Direction)pos;
            }
            pos++;
        }

        return direction;
    }

    private Direction GetDirectionMax(Point centerPoint, Point measurePoint, Direction lastDirection = Direction.None)
    {
        Direction direction = default;

        var posX = measurePoint.X - centerPoint.X;
        var posY = measurePoint.Y - centerPoint.Y;
        int pos = 0;

        var directions = new List<Direction>();
        foreach (var triangle in DirectionalTrianglesMax)
        {
            if (PointInTriangle(
                    new Point(posX + int.MaxValue / 2, posY + int.MaxValue / 2),
                    triangle[0],
                    triangle[1],
                    triangle[2]
                ))
            {
                directions.Add((Direction)pos);
            }
            pos++;
        }

        if (direction == default)
        {
            direction = directions.FirstOrDefault();
        }

        return direction;
    }

    private Point GetNextStepPoint(Point currentPoint, Point targetPoint, ref Direction nextDirection)
    {
        Point stepPoint;
        RectFr? collisionRect;

        var lastDirectionStepPoint = CheckCollision(currentPoint, nextDirection, out collisionRect);
        if (lastDirectionStepPoint.X != targetPoint.X && lastDirectionStepPoint.Y != targetPoint.Y && currentPoint.X != targetPoint.X && currentPoint.Y != targetPoint.Y)
        {
            if (collisionRect == null || (lastDirectionStepPoint.X == targetPoint.X && lastDirectionStepPoint.Y == targetPoint.Y))
            {
                return lastDirectionStepPoint;
            }
        }

        var globalDirection = GetDirectionMax(currentPoint, targetPoint);

        var globalStepPoint = CheckCollision(currentPoint, globalDirection, out collisionRect);
        var lastDirection = nextDirection;
        stepPoint = globalStepPoint;

        if (globalStepPoint.X == targetPoint.X && globalStepPoint.Y == targetPoint.Y)
        {
            return globalStepPoint;
        }

        if (collisionRect != null)
        {
            while (collisionRect != null && nextDirection != lastDirection)
            {
                Direction newDirection =
                    (int)globalDirection < (int)lastDirection ? nextDirection + 1 : nextDirection - 1;
                var testStepPoint = CheckCollision(currentPoint, newDirection, out collisionRect);

                if (testStepPoint.X == targetPoint.X && testStepPoint.Y == targetPoint.Y)
                {
                    return testStepPoint;
                }

                lastDirection = newDirection;
                nextDirection = newDirection;
                stepPoint = testStepPoint;
            }
        }
        else
        {
            nextDirection = globalDirection;
            stepPoint = globalStepPoint;
        }

        return stepPoint;
    }

    private void SetColorAndScaling(IDrawableComponent pin1drawable, IDrawableComponent pin2drawable)
    {
        var color = Color.FromRgb(red: Convert.ToInt32(pin2drawable.Position.X * 100 % 256),
            Convert.ToInt32(pin2drawable.Position.Y * 100 % 256), Convert.ToInt32(pin1drawable.Position.X * 100 % 256));
        ((ScalingCanvas)DebugCanvas).StrokeColor = color;
        ((ScalingCanvas)DebugCanvas).StrokeSize = 3;
    }
}