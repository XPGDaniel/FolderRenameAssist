using System.Windows.Controls;
using System.Windows.Documents;

namespace FolderRenameAssist.Class
{
    public static class RichTextBoxHepler
    {
        public static void SetText(this RichTextBox richTextBox, string text)
        {
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(text.Trim())));
        }

        public static string GetText(this RichTextBox richTextBox)
        {
            return new TextRange(richTextBox.Document.ContentStart,
                richTextBox.Document.ContentEnd).Text.Trim();
        }
    }
}
