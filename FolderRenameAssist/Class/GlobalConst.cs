using System.Windows;

namespace FolderRenameAssist.Class
{
    public class GlobalConst
    {
        public GlobalConst()
        {

        }
        public const string EMPTY_STRING = "";
        public const string ZERO = "0";

        public const string RESULT_FOLDER_MERGED = "Folder Merged";
        public const string RESULT_RENAME_OK = "Rename OK";
        public const string RESULT_RENAME_FAIL = "Rename Fail";
        public const string RESULT_NO_RENAME = "Not Affected";
        public const string RESULT_UNDO_OK = "Undo OK";
        public const string RESULT_UNDO_FAIL = "Undo Fail";
        public const string RESULT_INVALID_NEW_FOLDERNAME = "Invalid new FolderName";
        public const string RESULT_INVALID_NEW_FILENAME_FILEEXTENSION = "Invalid new Filename or Extension";

        public const string FILETYPE_XML_FILTER = "XML Files (*.xml)|*.xml";
        public const string FILETYPE_XML = "xml";

        public const string MOVE_UP = "Up";
        public const string MOVE_DOWN = "Down";

        public const DragDropKeyStates KEY_CTRL = DragDropKeyStates.ControlKey;
        public const DragDropKeyStates KEY_SHIFT = DragDropKeyStates.ShiftKey;
    }
}
