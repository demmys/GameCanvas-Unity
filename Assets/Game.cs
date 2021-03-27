#nullable enable
using GameCanvas;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public sealed class Game : GameBase
{
    private enum Status
    {
        Game, Success, Fail
    }
    private Background? background;
    private Bifidus? bifidus;
    private SwipeUp? swipeUp;
    private int2 screen;
    private float goal;
    private float position = 0;
    private Status status = Status.Game;

    public override void InitGame()
    {
        screen = gc.DeviceScreenSize * 2;
        gc.ChangeCanvasSize(screen.x, screen.y);
        background = new Background(screen);
        bifidus = new Bifidus(gc, screen, 100, (int)(gc.TargetFrameRate * 0.2f));
        swipeUp = new SwipeUp();
        goal = screen.y * 10;
    }

    public override void UpdateGame()
    {
        if (position >= goal)
        {
            swipeUp?.StopInertia();
            status = Status.Success;
        }
        else if (bifidus?.LifeCount == 0)
        {
            swipeUp?.StopInertia();
            status = Status.Fail;
        }
        else
        {
            position += swipeUp?.CalcMove(gc) ?? 0;
            bifidus?.Update(gc);
        }
    }

    public override void DrawGame()
    {
        background?.Draw(gc, position);
        bifidus?.Draw(gc);

        var rest = (int)math.max(0, math.ceil((goal - position) / goal * 6000));
        gc.StringAnchor = GcAnchor.UpperCenter;
        gc.FontSize = (int)(screen.y * 0.03f);
        gc.SetColor(255, 255, 255);
        gc.DrawString("ゴールまで: " + rest + "mm", screen.x / 2, screen.y * 0.02f);

        gc.FontSize = (int)(screen.y * 0.1f);
        switch (status)
        {
            case Status.Fail:
                gc.DrawString("Failed...", screen.x / 2, screen.y / 2);
                break;
            case Status.Success:
                gc.DrawString("Success!!!", screen.x / 2, screen.y / 2);
                break;
        }
    }
}

internal class SwipeUp
{
    private float pointBegan;
    private float frameBegan;
    private float lastTouch;
    private float inertia;

    internal SwipeUp() { }

    internal float CalcMove(IGameCanvas gc)
    {
        float2 point;
        float move = 0;
        if (gc.IsTouchBegan(out point))
        {
            pointBegan = point.y;
            frameBegan = gc.CurrentFrame;
            inertia = 0;
        }

        if (gc.IsTouched(out point))
        {
            if (lastTouch > 0)
            {
                move = lastTouch - point.y;
            }
            lastTouch = point.y;
        }
        else if (inertia > 0)
        {
            inertia *= 0.9f;
            move = inertia;
        }

        if (gc.IsTouchEnded(out point))
        {
            inertia = (pointBegan - point.y) / (gc.CurrentFrame - frameBegan);
        }
        return move;
    }

    internal void StopInertia()
    {
        inertia = 0;
    }
}

internal class Background
{
    private int2 frame;
    private int nubsize;
    private int interval;

    internal Background(int2 screenSize)
    {
        frame = screenSize;
        nubsize = (int)(screenSize.y * 0.1f);
        interval = (int)(screenSize.y * 0.2f);
    }

    internal void Draw(IGameCanvas gc, float position)
    {
        var margin = position % (nubsize + interval);
        gc.SetBackgroundColor(0.890f, 0.525f, 0.572f);
        gc.ClearScreen();
        gc.SetColor(155, 91, 99);
        gc.RectAnchor = GcAnchor.UpperLeft;
        for (var s = -margin; s < frame.y; s += nubsize + interval)
        {
            var size = math.min(math.min(nubsize, nubsize + s), frame.y - s);
            if (size > 0)
            {
                gc.FillRect(0, math.max(s, 0), frame.x, size);
            }
        }
    }
}

internal class Bifidus
{
    private struct Life {
        internal int X { get; set; }
        internal int Y { get; set; }
        internal int R { get; set; }
        internal int I { get; }
        internal bool D { get; set; }
        internal Life(int x, int y, int r, int i)
        {
            X = x;
            Y = y;
            R = r;
            I = i;
            D = i % 2 == 0;
        }
    }

    private int wide;
    private int cycleFrame;
    private int deathCycle;
    private List<Life> lifes;

    internal int LifeCount { get => lifes.Count; }

    internal Bifidus(IGameCanvas gc, int2 screenSize, int initialLife, int deathCycle)
    {
        this.deathCycle = deathCycle;
        wide = math.min(screenSize.x, screenSize.y);
        cycleFrame = gc.TargetFrameRate / 2;
        lifes = new List<Life>();
        int max = (int)math.floor(wide * 0.3f);
        int2 center = screenSize / 2;
        for (var i = 0; i < initialLife; ++i)
        {
            int r = gc.Random(0, max);
            int x = gc.Random(-r, r);
            int y = (int)math.sqrt(math.pow(r, 2) - math.pow(x, 2)) * (x % 2 == 0 ? -1 : 1);
            lifes.Add(new Life(center.x + x, center.y + y, gc.Random(0, 60), gc.Random(cycleFrame + 1, cycleFrame * 10)));
        }
    }

    internal void Update(IGameCanvas gc)
    {
        if (lifes.Count == 0) return;
        if (gc.CurrentFrame % deathCycle == 0)
        {
            lifes.RemoveAt(gc.Random(0, lifes.Count - 1));
        }
        for (var i = 0; i < lifes.Count; ++i)
        {
            var life = lifes[i];
            var cycle = (cycleFrame + gc.CurrentFrame) % life.I;
            if (cycle < cycleFrame)
            {
                life.X += life.D ? 1 : -1;
            }
            else if (cycle == cycleFrame)
            {
                life.D = !life.D;
            }
            lifes[i] = life;
        }
    }

    internal void Draw(IGameCanvas gc)
    {
        if (lifes.Count == 0) return;
        var w = wide * 0.01f;
        var h = wide * 0.02f;
        gc.SetColor(255, 255, 255);
        gc.CornerRadius = w;
        for (var i = 0; i < lifes.Count; ++i)
        {
            var life = lifes[i];
            gc.RectAnchor = GcAnchor.UpperCenter;
            gc.FillRoundedRect(life.X, life.Y, w, h, life.R);
            gc.RectAnchor = GcAnchor.LowerCenter;
            gc.FillRoundedRect(life.X, life.Y, w, h, life.R + 60);
            gc.RectAnchor = GcAnchor.LowerCenter;
            gc.FillRoundedRect(life.X, life.Y, w, h, life.R - 60);
        }
    }
}
