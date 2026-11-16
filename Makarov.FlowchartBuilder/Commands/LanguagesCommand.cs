using Makarov.Framework.Core;

namespace Makarov.FlowchartBuilder.Commands;

public sealed class LanguagesCommand : CommandWithSubcommands
{
    public override IEnumerable<string> Commands =>
        Core.Instance.Translators.Languages.OrderBy(language => Core.Instance.CurrentTranslator[language], StringComparer.CurrentCulture);

    /// <summary>The language is applied on the next start, because windows are translated when they are created.</summary>
    public override void Run(string subcommand)
    {
        Settings.Environment.Languages.CurrentLanguage = subcommand;

        Translator translator = Core.Instance.CurrentTranslator;
        MessageBox.Show(
            Core.Instance.MainWindow,
            translator["Msg_Need_Restart_Application"],
            translator["Msg_Info"],
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    public override Bitmap? GetIcon(string subcommand) => Core.Instance.Icons.TryGetCopy(subcommand);
}
