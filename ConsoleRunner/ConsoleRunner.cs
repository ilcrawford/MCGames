using System;
using System.IO;
using System.Runtime.Loader;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using ILCrawford.MCGame.MCCore;
namespace ILCrawford.MCGame.ConsoleRunner
{
    class ConsoleRunner
    {
        [ImportMany]
        static public IGame[] games { get; private set; }

        static private CompositionHost container;

        static void Main(string[] args)
        {
            Compose(AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine(games.Length);
            foreach (var game in games)
            {
                Console.WriteLine(game.Name);
                game.Run();
            }



        }



        static private void Compose(string dirToLoad)
        {

            var files = Directory.EnumerateFiles(dirToLoad, "*Command.dll", SearchOption.AllDirectories);
            var configuration = new ContainerConfiguration();

            var conventions = new ConventionBuilder();

            conventions.ForTypesDerivedFrom<IGame>()
                       .Export<IGame>()
                       .Shared();

            foreach (var file in files)
            {

                configuration.WithAssembly(AssemblyLoadContext.Default.LoadFromAssemblyPath(file), conventions);
            }

            container = configuration.CreateContainer();
            games = (IGame[])container.GetExports<IGame>();
            foreach (var item in games)
            {
                Console.WriteLine(item.ToString());
            }
        }


    }
}
