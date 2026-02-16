using Jotunn.Entities;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class TestCommand : ConsoleCommand
    {
        public override string Name => "KnowsRecipe";

        public override string Help => "Test to see if recipe is known";

        public override void Run(string[] args)
        {
            if (args.Length == 0)
            {
                foreach (string recipe in Player.m_localPlayer.m_knownRecipes)
                {
                    Jotunn.Logger.LogWarning(recipe);
                }
                return;
            }

            Jotunn.Logger.LogWarning(Player.m_localPlayer.IsRecipeKnown(args[0]));
        }
    }
}
