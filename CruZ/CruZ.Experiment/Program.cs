﻿
using System;
using System.IO;

using CruZ.GameEngine;

namespace CruZ.Experiment;

internal class Program
{
    static void Main(string[] args)
    {
        GameWrapper game = new PhysicExperiment();
        game.Run();
        //GameApplication gameApp = GameApplication.CreateContext(
        //    game, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource"));
        //gameApp.Run();
    }
}
