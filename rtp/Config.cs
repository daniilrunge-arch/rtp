using System;
using System.IO;
using Newtonsoft.Json;

namespace RTPPlugin
{
    public class Config
    {
        public int CooldownMinutes { get; set; } = 5;

        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Config Read(string path)
        {
            if (!File.Exists(path))
            {
                var conf = new Config();
                conf.Write(path);
            }
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path)) ?? new Config();
        }
    }
}