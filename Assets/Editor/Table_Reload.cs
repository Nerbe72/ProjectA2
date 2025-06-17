using UnityEditor;

public class Table_Reload : Editor
{
    [MenuItem("CS_Util/Table/CSV &F1", false, 1)]
    public static void Parser_Table_CSV()
    {
        TableMgr mgr = new TableMgr();


        mgr.Init();
        mgr.Save();
    }
}
