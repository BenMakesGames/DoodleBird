using System.Collections.Frozen;
using PetDoodle.Data;

namespace PetDoodle.Encounters;

public static class EncounterExtensions
{
    private static readonly FrozenDictionary<Encounter, EncounterInfo> Info;

    static EncounterExtensions()
    {
        Info = new Dictionary<Encounter, EncounterInfo>
        {
            [Encounter.HollowLog] = new("Hollow Log", [
                new EncounterOption
                {
                    Label = "Crawl through",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Nothing inside."),
                        new SubstituteOutcome("Found mushrooms!", Encounter.Mushrooms),
                        new SubstituteOutcome("A giant toad!", Encounter.GiantToad),
                    ],
                },
                new EncounterOption
                {
                    Label = "Jump over",
                    Kind = OptionKind.Ignore,
                    Outcomes =
                    [
                        new FlavorOutcome("Cleared it!"),
                        new FlavorOutcome("Tripped and fell."),
                    ],
                },
                new EncounterOption
                {
                    Label = "Roll it away",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Rolled it away!"),
                        new FlavorOutcome("Too heavy."),
                        new SubstituteOutcome("Toad crawled out!", Encounter.GiantToad),
                    ],
                },
            ]),

            [Encounter.Snake] = new("Snake", [
                new EncounterOption
                {
                    Label = "Go around",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Wide berth given.")],
                },
                new EncounterOption
                {
                    Label = "Intimidate",
                    Kind = OptionKind.Engage,
                    Outcomes = [new FlavorOutcome("Snake fled.")],
                },
            ]),

            [Encounter.LoneTree] = new("Lone Tree", [
                new EncounterOption
                {
                    Label = "Climb it",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Found bananas!"),
                        new SubstituteOutcome("A squirrel!", Encounter.FightSquirrel),
                    ],
                },
                new EncounterOption
                {
                    Label = "Ignore it",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Hopped past.")],
                },
            ]),

            [Encounter.Mushrooms] = new("Mushrooms", [
                new EncounterOption
                {
                    Label = "Eat one",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Tasty!"),
                        new FlavorOutcome("Bitter. Yuck!"),
                    ],
                },
                new EncounterOption
                {
                    Label = "Hop away",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Hopped away.")],
                },
            ]),

            [Encounter.GiantToad] = new("Giant Toad", [
                new EncounterOption
                {
                    Label = "Peck at it",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Toad hopped off."),
                        new EndAdventureOutcome("Knocked silly. Home."),
                    ],
                },
                new EncounterOption
                {
                    Label = "Flee",
                    Kind = OptionKind.Retreat,
                    Outcomes = [new EndAdventureOutcome("Flapped home!")],
                },
            ]),

            [Encounter.FightSquirrel] = new("Fight Squirrel", [
                new EncounterOption
                {
                    Label = "Peck",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Squirrel fled."),
                        new EndAdventureOutcome("Lost the fight."),
                    ],
                },
                new EncounterOption
                {
                    Label = "Glide to surface",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Glided down.")],
                },
            ]),

            [Encounter.MusclyTrout] = new("Muscly Trout", [
                new EncounterOption
                {
                    Label = "Eat",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Tasty fish!"),
                        new FlavorOutcome("Too strong to grab."),
                    ],
                },
                new EncounterOption
                {
                    Label = "Ignore",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Swam past.")],
                },
            ]),

            [Encounter.Rapids] = new("Rapids", [
                new EncounterOption
                {
                    Label = "Avoid rocks",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Avoided them."),
                        new EndAdventureOutcome("Hit a rock. Limped home."),
                    ],
                },
            ]),

            [Encounter.Sandcastle] = new("Sandcastle", [
                new EncounterOption
                {
                    Label = "Investigate",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Empty inside."),
                        new SubstituteOutcome("A crab inside!", Encounter.StartledCrab),
                    ],
                },
                new EncounterOption
                {
                    Label = "Destroy",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Stomped flat."),
                        new FlavorOutcome("Crab fled to sea."),
                    ],
                },
                new EncounterOption
                {
                    Label = "Ignore",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Hopped past.")],
                },
            ]),

            [Encounter.PurpleSeaweed] = new("Purple Seaweed", [
                new EncounterOption
                {
                    Label = "Eat",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Tasty!"),
                        new FlavorOutcome("Bitter. Yuck!"),
                    ],
                },
                new EncounterOption
                {
                    Label = "Ignore",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Hopped past.")],
                },
            ]),

            [Encounter.AggressiveSeagull] = new("Aggressive Seagull", [
                new EncounterOption
                {
                    Label = "Intimidate",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Seagull flew off."),
                        new EndAdventureOutcome("Tackled! Home."),
                    ],
                },
                new EncounterOption
                {
                    Label = "Retreat",
                    Kind = OptionKind.Retreat,
                    Outcomes = [new EndAdventureOutcome("Flapped home!")],
                },
            ]),

            [Encounter.StartledCrab] = new("Startled Crab", [
                new EncounterOption
                {
                    Label = "Peck",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Crab scuttled off."),
                        new EndAdventureOutcome("Pinched. Home."),
                    ],
                },
                new EncounterOption
                {
                    Label = "Ignore",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Hopped past.")],
                },
            ]),

            [Encounter.GlowingMushrooms] = new("Glowing Mushrooms", [
                new EncounterOption
                {
                    Label = "Ignore",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Hopped past.")],
                },
            ]),

            [Encounter.GiantBat] = new("Giant Bat", [
                new EncounterOption
                {
                    Label = "Intimidate",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Bat flew off."),
                        new EndAdventureOutcome("Rebuffed. Home."),
                    ],
                },
                new EncounterOption
                {
                    Label = "Sneak around",
                    Kind = OptionKind.Ignore,
                    Outcomes =
                    [
                        new FlavorOutcome("Slipped past."),
                        new FlavorOutcome("Spotted! Ran past."),
                        new EndAdventureOutcome("Caught! Home."),
                    ],
                },
                new EncounterOption
                {
                    Label = "Retreat",
                    Kind = OptionKind.Retreat,
                    Outcomes = [new EndAdventureOutcome("Flapped home!")],
                },
            ]),

            [Encounter.LargeBoulder] = new("Large Boulder", [
                new EncounterOption
                {
                    Label = "Move",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Too heavy."),
                        new FlavorOutcome("Shoved it past!"),
                    ],
                },
                new EncounterOption
                {
                    Label = "Ignore",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Hopped past.")],
                },
            ]),

            [Encounter.CarnivorousPlant] = new("Carnivorous Plant", [
                new EncounterOption
                {
                    Label = "Ignore",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Hopped past.")],
                },
                new EncounterOption
                {
                    Label = "Look inside",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new EndAdventureOutcome("Chomped! Home."),
                        new FlavorOutcome("Saved a butterfly!"),
                    ],
                },
            ]),

            [Encounter.Quicksand] = new("Quicksand", [
                new EncounterOption
                {
                    Label = "Get out!",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Wriggled free."),
                        new EndAdventureOutcome("Exhausted. Home."),
                    ],
                },
            ]),

            [Encounter.NanerTree] = new("Naner Tree", [
                new EncounterOption
                {
                    Label = "Climb",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Naners! Tasty!"),
                        new SubstituteOutcome("A Naner Bird!", Encounter.NanerBird),
                    ],
                },
                new EncounterOption
                {
                    Label = "Ignore",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Hopped past.")],
                },
            ]),

            [Encounter.NanerBird] = new("Naner Bird", [
                new EncounterOption
                {
                    Label = "Listen",
                    Kind = OptionKind.Engage,
                    Outcomes = [new FlavorOutcome("Words made no sense.")],
                },
                new EncounterOption
                {
                    Label = "Ignore",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Ate naners. Tasty!")],
                },
            ]),

            [Encounter.LongAbandonedVillage] = new("Abandoned Village", [
                new EncounterOption
                {
                    Label = "Explore",
                    Kind = OptionKind.Engage,
                    Outcomes =
                    [
                        new FlavorOutcome("Weird machines!"),
                        new SubstituteOutcome("Quicksand!", Encounter.Quicksand),
                        new SubstituteOutcome("A snake!", Encounter.Snake),
                        new FlavorOutcome("Nothing inside."),
                    ],
                },
                new EncounterOption
                {
                    Label = "Ignore",
                    Kind = OptionKind.Ignore,
                    Outcomes = [new FlavorOutcome("Hopped past.")],
                },
            ]),
        }.ToFrozenDictionary();

        foreach (var (encounter, info) in Info)
        {
            foreach (var option in info.Options)
            {
                if (option.Outcomes is null || option.Outcomes.Length == 0)
                    throw new InvalidOperationException(
                        $"Encounter {encounter} option '{option.Label}' has no outcomes."
                    );
            }
        }
    }

    public static EncounterInfo GetInfo(this Encounter encounter) => Info[encounter];
}
