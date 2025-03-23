using DirScaner;
using System.ComponentModel.DataAnnotations;
using System.Text;


Console.OutputEncoding = Encoding.UTF8; // Інакше українське "і" не виводилось.



//Console.BufferHeight = 1000;

Console.WriteLine("The current buffer height is {0} rows.",
                   Console.BufferHeight);

string path ="";
if (args.Length>0)
    path= args[0];

if (path == null || !Directory.Exists(path))
{
    Console.WriteLine("Enter the path of a directory.");
    path = Console.ReadLine();
}

if (!Directory.Exists(path))
{
    Console.WriteLine($"Directory {path} does not exist");
    return;
}
Console.WriteLine("-------------------------------------------------------------------------");
Console.WriteLine();
//ScanDir(path,1);
ScanDir_new(path);

void ScanDir(string dir, int level)
{
   if (level==1) 
    Console.WriteLine(" ".PadLeft((level-1)*3)+ dir);
   else
    Console.WriteLine(" ".PadLeft((level - 1) * 3) + Path.GetFileName(dir));
   // Подкаталоги
    string[] subDirs = Directory.GetDirectories(dir+@"\", "*", SearchOption.TopDirectoryOnly);
    foreach (string subDir in subDirs)    
        ScanDir(subDir, level+1);

    //Файлы
    string[] files = Directory.GetFiles(dir + @"\", "*", SearchOption.TopDirectoryOnly);
    foreach (string f in files)
        Console.WriteLine(" ".PadLeft((level - 1) * 3 + 1) + Path.GetFileName(f));

}




// Без рекурсии
void ScanDir_new(string dir)
{
    List<FileNode> fl = new();
    fl.Capacity = 15000;
    int level = 0;
    fl.Add(new FileNode(dir, true, 1));

    Console.WriteLine($"Изначально занято памяти {GC.GetTotalMemory(false)}" );    
    // Заполняем полный список файлов
    bool doScan = true;
    int  dirIndex = 0; // Индекс с которого начинается проверка каталогов
    while (doScan)
    {
        int prevCount = fl.Count;
        //        AddSubDir(fl, ++level);        
        AddSubDir_opt(fl, ++level, dirIndex);
        dirIndex = prevCount - 1;
        Console.WriteLine($"Level={level} Память {GC.GetTotalMemory(false)}");
    //    Console.WriteLine("Поколение fl = " + GC.GetGeneration(fl));
        doScan = fl.Count > prevCount;
    }
    
    var flSorted =
                from n in fl
                orderby n.SortName// сортируем по имени с полным путем, уложится почти правильно. Только некоторые файлы могут быть раньше каталогов??
                select n;

    //var flSorted = fl.OrderBy(n => n, new CustomComparer() ).ToList();
    Console.WriteLine(GC.GetTotalMemory(false));
    return;
    WriteToConsole(flSorted);

    void WriteToConsole(IOrderedEnumerable<FileNode> flSorted)
    {
        // Вывод в консоль
        foreach (var info in flSorted)
        {
            if (info.IsDirectory)
            {
                string dirText = " ".PadLeft((info.Level - 1) * 3) + info.FileName;
                Console.WriteLine(dirText);
            }
            else
                Console.WriteLine(" ".PadLeft((info.Level - 1) * 3) + info.FileName);
        }
    }

    // Обход каталогов предыдущего уровня. Оптимизирован. Исключено формирование промежуточного списка через LINQ 
    void AddSubDir_opt(List<FileNode> fileNodes, int scanLevel,  int startIndex )
    {
        Console.WriteLine($"Level={level} Память перед фильтрацией предыдущего уровня {GC.GetTotalMemory(false)}");
        var roots = fileNodes.Where(x => x.IsDirectory && x.Level == scanLevel).ToList();
        int countBefore = fileNodes.Count;
        
        for (int i = startIndex; i<countBefore;i++) 
        {
            if (fileNodes[i].IsDirectory && fileNodes[i].Level == scanLevel)
            {
                var dir = fileNodes[i];
                string[] subDirs = Directory.GetDirectories(dir.Name + @"\", "*", SearchOption.TopDirectoryOnly);
                foreach (string subDir in subDirs)
                {
                    fileNodes.Add(new FileNode(subDir, true, scanLevel + 1));
                }
                
                string[] files = Directory.GetFiles(dir.Name + @"\", "*", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                    fileNodes.Add(new FileNode(file, false, scanLevel + 1));
            }

        }        
    }

    void AddSubDir(List<FileNode> fileNodes, int scanLevel)
    {
        Console.WriteLine($"Level={level} Память перед фильтрацией предыдущего уровня {GC.GetTotalMemory(false)}");
        var roots = fileNodes.Where(x=> x.IsDirectory && x.Level== scanLevel).ToList();
        bool first = true;
        foreach (var dir in roots)
        {
            if (first)
            {
                Console.WriteLine($"Level={level} Память после применения фильтрации предыдущего уровня {GC.GetTotalMemory(false)}");
                first = false;
            }
             
            string[] subDirs = Directory.GetDirectories(dir.Name + @"\", "*", SearchOption.TopDirectoryOnly);
            foreach (string subDir in subDirs)
            {
                fileNodes.Add(new FileNode(subDir, true, scanLevel + 1));
            }

            string[] files = Directory.GetFiles(dir.Name + @"\", "*", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
                fileNodes.Add(new FileNode(file, false, scanLevel + 1));
            //Console.WriteLine(" ".PadLeft((level - 1) * 3 + 1) + Path.GetFileName(f));
        }
    }

    
}