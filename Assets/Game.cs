#nullable enable
using GameCanvas;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

public sealed class Game : GameBase
{
    private enum Status
    {
        Menu, Game, Success, Fail
    }

    private MenuSpace? menuSpace;
    private AdventureSpace? adventureSpace;
    private int2 screen;
    private Status status = Status.Menu;


    public override void InitGame()
    {
        screen = gc.DeviceScreenSize * 2;
        gc.ChangeCanvasSize(screen.x, screen.y);
        menuSpace = new MenuSpace(screen);
        adventureSpace = new AdventureSpace(screen, new int2(3, 2));
    }

    public override void UpdateGame()
    {
        switch (status)
        {
            case Status.Menu:
                menuSpace?.Update(gc);
                if (menuSpace?.Start ?? false)
                {
                    status = Status.Game;
                }
                break;
            case Status.Game:
                break;
        }
    }

    public override void DrawGame()
    {
        switch (status)
        {
            case Status.Menu:
                menuSpace?.Draw(gc);
                break;
            case Status.Game:
                adventureSpace?.Draw(gc);
                break;
        }
    }

    internal class MenuSpace
    {
        internal bool Start = false;

        private int2 frame;

        internal MenuSpace(int2 screenSize)
        {
            frame = screenSize;
        }

        internal void Update(IGameCanvas gc)
        {
            Start = gc.IsTouched();
        }

        internal void Draw(IGameCanvas gc)
        {
            gc.SetBackgroundColor(0.203f, 0.286f, 0.368f);
            gc.ClearScreen();

            var centerX = frame.x / 2;
            var padY = frame.y * 0.03f;
            gc.SetColor(Color.white);

            // icon
            var startY = frame.y * 0.3f;
            var rocketSize = frame.y * 0.07f;
            gc.RectAnchor = GcAnchor.UpperCenter;
            gc.DrawImage(GcImage.RocketFire, new GcRect(centerX, startY, rocketSize, rocketSize));

            // title
            startY += rocketSize + padY;
            var fontSize = frame.y * 0.05f;
            gc.StringAnchor = GcAnchor.UpperCenter;
            gc.FontSize = (int)fontSize;
            gc.DrawString("Biomas Rocket Adventure", centerX, startY);

            // command
            startY += fontSize + padY * 4;
            gc.FontSize = (int)(frame.y * 0.03f);
            gc.DrawString("tap to start", centerX, startY);
        }
    }

    internal class AdventureSpace
    {
        private struct Cow
        {
            internal float X { get; set; }
            internal float Y { get; set; }
            internal float S { get; set; }

            internal bool Shit { get; set; }
            internal float ShitS { get => S * 0.25f; }
            internal float ShitX { get => X + S / 2 + ShitS; }
            internal float ShitY { get => Y + S / 2 - ShitS; }

            internal Cow(float x, float y, float s)
            {
                X = x;
                Y = y;
                S = s;
                Shit = true;
            }
        }

        private const float SKY_RATIO = 0.5f;

        private int2 frame;
        private List<Cow> cows;

        internal AdventureSpace(int2 screenSize, int2 cowNum)
        {
            frame = screenSize;
            cows = new List<Cow>();
            var cowW = frame.x / (cowNum.x * 2 + 2);
            var cowHH = frame.y * (1 - SKY_RATIO) / (cowNum.y * 4 + 1);
            var cowSize = math.min(cowW, cowHH * 2);
            var cowYBase = frame.y * SKY_RATIO + cowSize / 2;
            for (var j = 1; j < cowNum.y + 1; ++j)
            {
                for (var i = 1; i < cowNum.x + 1; ++i)
                {
                    var cowX = cowW * i * 2 - cowW / 2 + cowW * (j % 2);
                    var cowY = cowYBase + cowHH * (j * 2 + 1);
                    cows.Add(new Cow(cowX, cowY, cowSize));
                }
            }
        }

        internal void Update(IGameCanvas gc)
        {
        }

        internal void Draw(IGameCanvas gc)
        {
            gc.SetBackgroundColor(0.203f, 0.286f, 0.368f);
            gc.ClearScreen();

            var centerX = frame.x / 2;
            var groundR = frame.y;
            gc.RectAnchor = GcAnchor.MiddleCenter;
            gc.LineWidth = frame.x / 100;

            // planet
            var planetY = frame.y * SKY_RATIO / 2;
            var planetSize = frame.y * 0.13f;
            gc.DrawImage(GcImage.Planet, centerX, planetY, planetSize, planetSize);

            // ground
            var groundY = frame.y * SKY_RATIO;
            gc.SetColor(236, 240, 241);
            gc.CircleResolution = 64;
            gc.SetColor(Color.white);
            gc.FillCircle(centerX, groundY + groundR, groundR);

            // energy
            var rocketSize = frame.y * 0.1f;
            var rocketY = groundY + rocketSize * 0.4f;
            var factorySize = frame.y * 0.1f;
            var factoryX = centerX + frame.x * 0.3f;
            var factoryY = groundY + factorySize;
            gc.SetColor(Color.black);
            gc.DrawLine(centerX, rocketY, centerX, factoryY);
            gc.DrawLine(centerX, factoryY, factoryX, factoryY);

            // rocket
            gc.DrawImage(GcImage.Rocket, centerX, rocketY, rocketSize, rocketSize, 45);

            // meter
            var meterY = rocketY - rocketSize;
            gc.SetColor(Color.white);
            gc.FillRect(centerX, meterY, rocketSize * 2, gc.LineWidth * 3);
            gc.SetColor(46, 204, 113);
            gc.FillRect(centerX, meterY, rocketSize * 2 - gc.LineWidth, gc.LineWidth * 2);

            // factory
            gc.DrawImage(GcImage.Factory, factoryX, factoryY, factorySize, factorySize);

            // cow
            foreach (var cow in cows)
            {
                gc.DrawImage(GcImage.Cow, cow.X, cow.Y, cow.S, cow.S);
                gc.DrawImage(GcImage.Shit, cow.ShitX, cow.ShitY, cow.ShitS, cow.ShitS);
            }

            // button
            gc.SetColor(46, 204, 113);
            var buttonH = frame.y * 0.07f;
            var buttonW = frame.x * 0.5f;
            var buttonY = frame.y - buttonH * 0.75f;
            gc.FillRoundedRect(centerX, buttonY, buttonW, buttonH);
            gc.CircleResolution = 16;
            gc.CornerRadius = gc.LineWidth * 5;
            gc.SetColor(Color.white);
            gc.FontSize = (int)(buttonH * 0.6f);
            gc.StringAnchor = GcAnchor.MiddleCenter;
            gc.DrawString("SHIP!", centerX, buttonY);
        }
    }
}
