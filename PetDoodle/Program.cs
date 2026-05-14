using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Model;
using PetDoodle;
using PetDoodle.GameStates;
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
        ]),
        new PictureMeta(Pictures.Cursor, "Graphics/Cursor", true),
        new PictureMeta(Pictures.TopGrass, "Graphics/TopGrass", true),
        new PictureMeta(Pictures.Bird, "Graphics/Bird", true),

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
    })
;

gsmBuilder.Run();

Log.Information("Shutting down - thanks for playing! :)");

/*SteamHelpers.Shutdown();*/
