namespace Game.Core.Common
{
    public sealed record ResourceDef(
        ResourceId Id,
        string Key, // "res.n1" / "iron_ore" (стабільний ключ)
        string Name,
        int StackLimit = 999
    );
}