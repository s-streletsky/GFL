using Dapper;
using GeeksForLess_Test.Models;
using Newtonsoft.Json;

namespace GeeksForLess_Test.Db
{
    public class FolderRepo : RepoBase
    {
        public Folder GetFolder(string[] pathSegments)
        {
            using (var connection = GetConnection())
            {
                var folder = connection.QueryFirstOrDefault<Folder>("SELECT * FROM Folders WHERE ParentId is NULL LIMIT 1;");

                if (pathSegments.Any())
                {
                    foreach (var folderName in pathSegments)
                    {
                        folder = connection.QueryFirstOrDefault<Folder>("SELECT * FROM Folders WHERE Title = @folderName AND ParentId = @folderId", new { folderName = folderName, folderId = folder.Id });
                    }
                }

                folder.Subfolders = GetSubfolders(folder.Id);
                
                return folder;
            }
        }

        public IEnumerable<Folder> GetSubfolders(int id)
        {
            using (var connection = GetConnection())
            {
                return connection.Query<Folder>("SELECT * FROM Folders WHERE ParentId = @id;", new {id = id});               
            }
        }

        public string ExportFolderStructure()
        {
            IEnumerable<Folder> folderStructure = null;

            using (var connection = GetConnection())
            {
                folderStructure = connection.Query<Folder>("SELECT * FROM Folders WHERE ParentId IS NOT NULL");             
            }

            return JsonConvert.SerializeObject(folderStructure, Formatting.Indented);
        }

        public void ImportFolderStructure(Folder[] folderStructure)
        {
            using (var connection = GetConnection())
            {
                connection.Execute("DELETE FROM Folders;");

                connection.Execute($"INSERT INTO Folders (Id, Title) VALUES (@id, @title);", new { id = 1, title = "Virtual Root" });

                foreach (var folder in folderStructure)
                {
                    connection.Execute($"INSERT INTO Folders (Id, ParentId, Title) VALUES (@id, @parentId, @title);", new {id = folder.Id, parentId = folder.ParentId, title = folder.Title});
                }
            }
        }
    }
}
