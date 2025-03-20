// See https://aka.ms/new-console-template for more information

using MadWorldNL.EarCut.Logic;

var triangles = EarCut<double>.Calculate([0, 0, 0, 50, 50, 00]);
Console.Write("triangles: " + string.Join(", ", triangles));

Console.WriteLine("This is a simple example of using the EarCut algorithm in C#.");