using Jotunn.Entities;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class TestGUICommand : ConsoleCommand
    {
        public override string Name => "TestGUI";

        public override string Help => "Test GUI to test UI components in the game";

        public override void Run(string[] args)
        {
            if (args.Length > 0)
            {
                return;
            }

            WizshBoneTwitchIntegration.testGUI.ShowTestPanel();
        }
    }
}
