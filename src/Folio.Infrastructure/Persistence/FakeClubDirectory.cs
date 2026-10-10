namespace Folio.Infrastructure.Persistence;

/// <summary>
/// Fictional registry content: ~30 Czech ice hockey clubs that "report" player transfers, budgets, spending and income.
/// Single source for the fake registry clients, the dev seed data and the demo period.
/// </summary>
public static class FakeClubDirectory
{
    public sealed record Club(int RegistryEntityId, string Name, string Code);

    public static readonly Club[] All =
    [
        new(101, "HC Sparta Praha",          "SPA"),
        new(102, "HC Slavia Praha",          "SLA"),
        new(103, "HC Oceláři Třinec",        "TRI"),
        new(104, "HC Kometa Brno",           "KOM"),
        new(105, "HC Energie Karlovy Vary",  "KVA"),
        new(106, "HC Dynamo Pardubice",      "PAR"),
        new(107, "Bílí Tygři Liberec",       "LIB"),
        new(108, "HC Olomouc",               "OLO"),
        new(109, "HC Verva Litvínov",        "LIT"),
        new(110, "HC Vítkovice Ridera",      "VIT"),
        new(111, "Mountfield HK",            "HKR"),
        new(112, "Motor České Budějovice",   "CBU"),
        new(113, "HC Škoda Plzeň",           "PLZ"),
        new(114, "Rytíři Kladno",            "KLA"),
        new(115, "HC Dukla Jihlava",         "JIH"),
        new(116, "HC Stadion Litoměřice",    "LTM"),
        new(117, "BK Mladá Boleslav",        "MBO"),
        new(118, "HC Frýdek-Místek",         "FMI"),
        new(119, "HC Zubr Přerov",           "PRE"),
        new(120, "HC Slovan Ústí nad Labem", "UST"),
        new(121, "Horácká Slavia Třebíč",    "TRE"),
        new(122, "HC Benátky nad Jizerou",   "BEN"),
        new(123, "Berounští Medvědi",        "BER"),
        new(124, "HC Tábor",                 "TAB"),
        new(125, "HC Baník Sokolov",         "SOK"),
        new(126, "Jestřábi Prostějov",       "PRO"),
        new(127, "AZ Havířov",               "HAV"),
        new(128, "HC RT Torax Poruba",       "POR"),
        new(129, "HC Stadion Vrchlabí",      "VRC"),
        new(130, "SK Kadaň",                 "KAD"),
    ];
}
