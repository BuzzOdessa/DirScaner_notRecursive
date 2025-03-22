namespace DirScaner
{
    internal class FileNode(string name, bool isDir, int level )
    {
        public string Name = name;
        public bool   IsDirectory = isDir;
        public int    Level = level;
        public string SortName  // Для того чтоб файлы были ниже  каталогов того же уровня невзирая на алфавит
                                // Правильней было бы кастомным компарером. Но  лениво
        {
            get {
                if (IsDirectory)
                    return Name;
                else
                 return $@"{Path.GetDirectoryName(name)}\zz{Path.GetFileName(name)}";
            }        
        }
        
        public string FileName = Path.GetFileName(name);
    }
    
 }
