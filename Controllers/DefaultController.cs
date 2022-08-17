using GeeksForLess_Test.Db;
using GeeksForLess_Test.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace GeeksForLess_Test.Controllers
{
    public class DefaultController : Controller
    {
        private readonly ILogger<DefaultController> _logger;
        private FolderRepo repo = new FolderRepo();

        public DefaultController(ILogger<DefaultController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string folderPath)
        {
            if (!string.IsNullOrWhiteSpace(folderPath) && !folderPath.EndsWith("/"))
            {
                return Redirect("/" + folderPath + "/");
            }

            var folderPathSegments = folderPath?
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => HttpUtility.UrlDecode(x))
                .ToArray() ?? new string[0];

            var folder = repo.GetFolder(folderPathSegments);

            return View(folder);      
        }

        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file != null)
            {
                string json;

                try
                {
                    using (var stream = file.OpenReadStream())
                    {
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            json = reader.ReadToEnd();
                        }
                    }
                }
                catch (Exception)
                {
                    ViewData["Alert"] = "Не удалось загрузить файл!";
                    return View();
                }

                Folder[] folderStructure;

                try
                {
                    folderStructure = JsonConvert.DeserializeObject<Folder[]>(json);
                }
                catch (Exception)
                {
                    ViewData["Alert"] = "Не удалось обработать загруженный файл!";
                    return View();
                }

                var duplicates = FindIdDuplicates(folderStructure);

                if (!duplicates.result)
                {
                    ViewData["Alert"] = "В импортируемом файле присутствуют записи с одинаковыми значениями полей Id: " + String.Join(", ", duplicates.Item1);
                    return View();
                }

                var foldersNamesValidation = CheckFolderNames(folderStructure);

                if (!foldersNamesValidation.result)
                {
                    ViewData["Alert"] = "В импортируемом файле присутствуют записи с недопустимыми значениями полей Title: " + String.Join(", ", foldersNamesValidation.Item1)
                                        + ". Использование следующих символов в названиях каталогов запрещено: \\ / : * ? \" < > |";
                    return View();
                }

                var backupJson = repo.ExportFolderStructure();
                var backupFileName = SaveExportFile(backupJson);

                repo.ImportFolderStructure(folderStructure);
                ViewData["Success"] = "Структура каталогов импортирована успешно";
                ViewData["ExportFileName"] = backupFileName;

                return View();
            }

            ViewData["Alert"] = "Файл пуст или не выбран! Пожалуйста, выберите корректный файл для импорта!";
            return View();
        }

        public IActionResult Export()
        {
            var json = repo.ExportFolderStructure();
            return GetFile(json);
        }

        public IActionResult File(string fileName)
        {
            try
            {
                return DownloadExportFile(fileName);
            }
            catch (Exception)
            {
                TempData["Alert"] = "Файл не найден!";
                return RedirectToAction("Import");
            }
            
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region [ FileOperations ]
        private FileContentResult GetFile(string json)
        {
            var fileName = $"export_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt";
            return File(System.Text.Encoding.UTF8.GetBytes(json), "binary/octet-stream", fileName);
        }

        private string SaveExportFile(string json)
        {
            var fileName = $"export_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt";
            var backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");
            var path = Path.Combine(backupFolder, fileName);
            if (!Directory.Exists(backupFolder)) { Directory.CreateDirectory(backupFolder); }
            System.IO.File.WriteAllText(path, json, Encoding.UTF8);

            return fileName;
        }

        private FileContentResult DownloadExportFile(string fileName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups", fileName);
            var exist = System.IO.File.Exists(path);

            if (exist)
            {
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
                return GetFile(json);
            }

            throw new InvalidOperationException();
        }
        #endregion

        #region [ DataValidation ]
        public (int[], bool result) FindIdDuplicates(Folder[] folderStructure)
        {
            var foldersIds = new List<int>(folderStructure.Length);

            foreach (var folder in folderStructure)
            {
                foldersIds.Add(folder.Id);
            }

            var duplicates = foldersIds.GroupBy(x => x)
                                        .Where(group => group.Count() > 1)
                                        .Select(group => group.Key);

            var result = duplicates.Count();

            if (result > 0)
            {
                return (duplicates.ToArray<int>(), false);
            }

            return (new int[0], true);
        }

        public (string[], bool result) CheckFolderNames(Folder[] folderStructure)
        {
            var badFolderNames = new List<string>();

            foreach (var folder in folderStructure)
            {
                if (Regex.IsMatch(folder.Title, @"[\\\/\:\*\?\""\<\>\|]"))
                {
                    badFolderNames.Add(folder.Title);
                }
            }

            var result = badFolderNames.Count();

            if (result > 0)
            {
                return (badFolderNames.ToArray<string>(), false);
            }

            return (new string[0], true);
        }
        #endregion
    }
}