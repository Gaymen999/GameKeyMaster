using System;
using System.IO;
using System.Text.Json;
using GameKeyMaster.Models;

namespace GameKeyMaster.Services
{
    public class DataService
    {
        private readonly string _filePath;

        public DataService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appData, "GameKeyMaster");
            Directory.CreateDirectory(appFolder);
            _filePath = Path.Combine(appFolder, "profiles.json");
        }

        public AppProfile LoadProfile()
        {
            if (!File.Exists(_filePath))
            {
                return new AppProfile();
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<AppProfile>(json) ?? new AppProfile();
            }
            catch
            {
                return new AppProfile();
            }
        }

        public void SaveProfile(AppProfile profile)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(profile, options);
            File.WriteAllText(_filePath, json);
        }
    }
}
