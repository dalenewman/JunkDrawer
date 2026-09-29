using Eto;
using Eto.Forms;

namespace JunkDrawer.Eto.Core;

public static class GuiStartup {
    public static void Run(string[] args) {
        using var app = new Application(Platform.Detect);
        try {
            var (file, selectedArrangement) = GuiLaunchOptions.Parse(args);
            var arrangement = ArrangementLocator.Resolve(selectedArrangement);
            app.Run(new MainForm(arrangement, file));
        } catch (Exception ex) when (ex is ArgumentException or FileNotFoundException or InvalidOperationException) {
            MessageBox.Show(ex.Message, "Junk Drawer", MessageBoxType.Error);
        }
    }
}
