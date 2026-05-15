using Autofac;
using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Model;
using PetDoodle;
using PetDoodle.GameStates;
using PetDoodle.Persistence;
using Serilog.Extensions.Autofac.DependencyInjection;
using Serilog;

/*if (!SteamHelpers.Startup())
    return;*/

DirectoryHelpers.EnsureDirectoryExists();

var gsmBuilder = new GameStateManagerBuilder();

gsmBuilder
    .SetWindowSize(128, 32, 2)
    .SetInitialGameState<Startup>()

    // TODO: set a better window title
    .SetWindowTitle("PetDoodle")

    // TODO: add any resources needed (refer to PlayPlayMini documentation for more info)
    .AddAssets([
        new FontMeta("Font", [
            new FontSheetMeta("Graphics/Font", 6, 8) { VerticalSpacing = 1, HorizontalSpacing = 0 }
        ], preLoaded: true),
        new PictureMeta(Pictures.Cursor, preLoaded: true),

        new PictureMeta(Pictures.TopGrass),
        new SpriteSheetMeta(Pictures.Bird, 15, 15),

        // new FontMeta(...)
        // new PictureMeta(...)
        // new SpriteSheetMeta(...)
        // new SongMeta(...)
        // new SoundEffectMeta(...)
    ])

    // TODO: any additional service registration (refer to PlayPlayMini and/or Autofac documentation for more info)
    .AddServices((s, c) => {
        var loggerConfig = new LoggerConfiguration()
            .WriteTo.File(Path.Join(DirectoryHelpers.LogDirectory, "Log.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
        ;

        s.RegisterSerilog(loggerConfig);

        s.RegisterType<SaveService>().AsSelf().SingleInstance();
    })
;

gsmBuilder.Run();

Log.Information("Shutting down - thanks for playing! :)");

/*SteamHelpers.Shutdown();*/
